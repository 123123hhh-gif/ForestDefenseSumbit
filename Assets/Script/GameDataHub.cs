using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameDataHub : MonoBehaviour
{
    private static GameDataHub _instance;
    private const string DEFAULT_USER_NAME = "Guest";
    public const string KEY_CURRENT_USER = "Game_User_Name";

    // 排行榜数据存储键（PlayerPrefs用）
    private const string RANK_DATA_KEY = "PlayerRankData";
    // 排行榜最大显示数量
    private const int MAX_RANK_COUNT = 10;

    // 当前排行榜数据
    private List<PlayerData> _rankList = new List<PlayerData>();

    // 替换原有List，用Dictionary存储道具ID和对应的数量（支持多数量）
    private Dictionary<string, int> _propCountDict = new Dictionary<string, int>();

    public static GameDataHub Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameDataHub>();
                if (_instance == null)
                {
                    GameObject dataHubObj = new GameObject("[GameDataHub]");
                    _instance = dataHubObj.AddComponent<GameDataHub>();
                }
            }
            return _instance;
        }
    }

    private string _currentUserName;
    public string CurrentUserName
    {
        get => string.IsNullOrEmpty(_currentUserName) ? DEFAULT_USER_NAME : _currentUserName;
        set
        {
            string validName = string.IsNullOrEmpty(value) ? DEFAULT_USER_NAME : value.Trim();
            if (_currentUserName != validName)
            {
                _currentUserName = validName;
                PlayerPrefs.SetString(KEY_CURRENT_USER, _currentUserName);
                LoadGameData();
            }
        }
    }

    public int CurrentLevel { get; private set; } = 1;
    public int Gold { get; private set; } = 0;

    private void Awake()
    {
        // 保障单例唯一性
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // 加载该用户的存档数据
        LoadGameData();
        LoadRankData();
    }

    // -------------------------- 核心工具方法：生成带用户名前缀的存档Key --------------------------
    /// <summary>
    /// 生成带用户名前缀的存档Key（避免不同用户数据冲突）
    /// </summary>
    /// <param name="baseKey">基础Key（如CurrentLevel、Gold）</param>
    /// <returns>拼接后的Key（如Guest_CurrentLevel）</returns>
    private string GetUserPrefKey(string baseKey)
    {
        return $"{CurrentUserName}_{baseKey}";
    }

    // -------------------------- 数据操作方法（封装修改逻辑） --------------------------
    /// <summary>
    /// 切换当前用户（外部调用此方法切换存档）
    /// </summary>
    /// <param name="newUserName">新用户名</param>
    public void SwitchUser(string newUserName)
    {
        CurrentUserName = newUserName;
    }

    /// <summary>
    /// 更新当前关卡（自动存档）
    /// </summary>
    public void SetCurrentLevel(int level)
    {
        if (level < 1) return; // 防错：关卡数不能小于1
        CurrentLevel = level;
        SaveGameData();
    }

    /// <summary>
    /// 增加金币（自动存档）
    /// </summary>
    public void AddGold(int amount)
    {
        if (amount < 0) return; // 防错：避免传入负数（扣金币可单独写方法）
        Gold += amount;
        SaveGameData();
    }

    /// <summary>
    /// 扣除金币（新增：道具购买时用，自动存档）
    /// </summary>
    /// <returns>是否扣除成功（金币足够返回true）</returns>
    public bool SubtractGold(int amount)
    {
        if (amount < 0 || Gold < amount) return false;
        Gold -= amount;
        SaveGameData();
        return true;
    }

    // -------------------------- 按用户隔离的存档/读档逻辑 --------------------------
    /// <summary>
    /// 加载当前用户的本地存档数据
    /// </summary>
    private void LoadGameData()
    {
        // 加载当前用户的关卡进度
        if (PlayerPrefs.HasKey(GetUserPrefKey("CurrentLevel")))
            CurrentLevel = PlayerPrefs.GetInt(GetUserPrefKey("CurrentLevel"));
        else
            CurrentLevel = 1; // 新用户默认从第1关开始

        // 加载当前用户的金币
        if (PlayerPrefs.HasKey(GetUserPrefKey("Gold")))
            Gold = PlayerPrefs.GetInt(GetUserPrefKey("Gold"));
        else
            Gold = 0; // 新用户默认0金币

        // 加载当前用户的道具数据（新增）
        LoadPropData();
    }

    /// <summary>
    /// 保存当前用户的数据到本地
    /// </summary>
    private void SaveGameData()
    {
        // 保存当前用户的关卡进度
        PlayerPrefs.SetInt(GetUserPrefKey("CurrentLevel"), CurrentLevel);
        // 保存当前用户的金币
        PlayerPrefs.SetInt(GetUserPrefKey("Gold"), Gold);

        // 保存当前用户的道具数据（新增）
        SavePropData();

        PlayerPrefs.Save(); // 强制保存
    }

    /// <summary>
    /// 重置当前用户的所有数据（测试/重玩时用）
    /// </summary>
    public void ResetGameData()
    {
        CurrentLevel = 1;
        Gold = 0;
        // 重置道具数据（新增）
        _propCountDict.Clear();
        SaveGameData();
    }

    public void DeleteUserData(string userName)
    {
        string validName = string.IsNullOrEmpty(userName) ? DEFAULT_USER_NAME : userName.Trim();

        PlayerPrefs.DeleteKey($"{validName}_CurrentLevel");
        PlayerPrefs.DeleteKey($"{validName}_Gold");
        // 删除道具数据（新增）
        PlayerPrefs.DeleteKey($"{validName}_PurchasedProps");

        PlayerPrefs.Save();

        if (validName == CurrentUserName)
        {
            LoadGameData();
        }
    }

    #region RankIng System
    /// <summary>
    /// 从本地加载排行榜数据
    /// </summary>
    private void LoadRankData()
    {
        try
        {
            // 从PlayerPrefs获取存储的JSON字符串
            string jsonData = PlayerPrefs.GetString(RANK_DATA_KEY, "");

            if (!string.IsNullOrEmpty(jsonData))
            {
                // 反序列化为列表
                _rankList = JsonUtility.FromJson<RankDataWrapper>(jsonData).playerList;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("加载排行榜数据失败: " + e.Message);
            _rankList = new List<PlayerData>();
        }
    }

    /// <summary>
    /// 保存排行榜数据到本地
    /// </summary>
    private void SaveRankData()
    {
        try
        {
            // 包装类用于序列化列表
            RankDataWrapper wrapper = new RankDataWrapper();
            wrapper.playerList = _rankList;

            // 序列化为JSON字符串并保存
            string jsonData = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(RANK_DATA_KEY, jsonData);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogError("保存排行榜数据失败: " + e.Message);
        }
    }

    /// <summary>
    /// 更新玩家星数（过关时调用）
    /// </summary>
    /// <param name="playerId">玩家ID/名称</param>
    /// <param name="addStars">本次过关新增的星数</param>
    public void UpdatePlayerStars(string playerId, int addStars)
    {
        if (addStars < 0)
        {
            Debug.LogWarning("新增星数不能为负数");
            return;
        }

        // 查找该玩家是否已存在
        PlayerData targetPlayer = _rankList.FirstOrDefault(p => p.playerId == playerId);

        if (targetPlayer != null)
        {
            // 存在则更新星数
            targetPlayer.totalStars += addStars;
        }
        else
        {
            // 不存在则添加新玩家
            _rankList.Add(new PlayerData(playerId, addStars));
        }

        // 重新排序并截取前10名
        SortAndLimitRankList();

        // 保存更新后的数据
        SaveRankData();

        Debug.Log($"玩家 {playerId} 星数已更新，当前总星数: {GetPlayerTotalStars(playerId)}");
    }

    /// <summary>
    /// 对排行榜进行排序并限制数量为前10名
    /// </summary>
    private void SortAndLimitRankList()
    {
        // 按总星数降序排序（星数高的在前）
        _rankList = _rankList.OrderByDescending(p => p.totalStars).ToList();

        // 如果数量超过10，则截取前10名
        if (_rankList.Count > MAX_RANK_COUNT)
        {
            _rankList = _rankList.Take(MAX_RANK_COUNT).ToList();
        }
    }

    /// <summary>
    /// 获取当前排行榜数据（前10名）
    /// </summary>
    /// <returns>排序后的排行榜列表</returns>
    public List<PlayerData> GetRankList()
    {
        // 返回副本，避免外部修改原数据
        return new List<PlayerData>(_rankList);
    }

    /// <summary>
    /// 获取指定玩家的总星数
    /// </summary>
    /// <param name="playerId">玩家ID/名称</param>
    /// <returns>总星数，不存在则返回0</returns>
    public int GetPlayerTotalStars(string playerId)
    {
        PlayerData targetPlayer = _rankList.FirstOrDefault(p => p.playerId == playerId);
        return targetPlayer?.totalStars ?? 0;
    }

    /// <summary>
    /// 清空排行榜数据（测试用）
    /// </summary>
    public void ClearRankData()
    {
        _rankList.Clear();
        PlayerPrefs.DeleteKey(RANK_DATA_KEY);
        PlayerPrefs.Save();
        Debug.Log("排行榜数据已清空");
    }

    // 辅助包装类，用于序列化List<PlayerData>（JsonUtility不支持直接序列化列表）
    [Serializable]
    private class RankDataWrapper
    {
        public List<PlayerData> playerList = new List<PlayerData>();
    }
    #endregion

    #region Prop System
    /// <summary>
    /// 加载当前用户的道具数据（按用户隔离）
    /// </summary>
    private void LoadPropData()
    {
        _propCountDict.Clear();
        string purchasedStr = PlayerPrefs.GetString(GetUserPrefKey("PurchasedProps"), "");
        if (!string.IsNullOrEmpty(purchasedStr))
        {
            // 数据格式：道具ID:数量,道具ID:数量 （如 "item1:3,item2:1"）
            string[] propPairs = purchasedStr.Split(',');
            foreach (string pair in propPairs)
            {
                string[] keyValue = pair.Split(':');
                if (keyValue.Length == 2 && !string.IsNullOrEmpty(keyValue[0]) && int.TryParse(keyValue[1], out int count))
                {
                    _propCountDict[keyValue[0]] = count;
                }
            }
        }
    }

    /// <summary>
    /// 保存当前用户的道具数据到本地
    /// </summary>
    private void SavePropData()
    {
        // 拼接数据：道具ID:数量,道具ID:数量
        List<string> propStrList = new List<string>();
        foreach (var kvp in _propCountDict)
        {
            propStrList.Add($"{kvp.Key}:{kvp.Value}");
        }
        string purchasedStr = string.Join(",", propStrList);
        PlayerPrefs.SetString(GetUserPrefKey("PurchasedProps"), purchasedStr);
    }

    /// <summary>
    /// 购买道具（自动扣金币+存档）
    /// </summary>
    /// <param name="propId">道具唯一ID</param>
    /// <param name="price">道具售价</param>
    /// <param name="buyCount">购买数量（默认1）</param>
    /// <returns>是否购买成功</returns>
    public bool PurchaseProp(string propId, int price, int buyCount = 1)
    {
        if (string.IsNullOrEmpty(propId) || price < 0 || buyCount < 1)
        {
            Debug.LogWarning("购买道具参数错误");
            return false;
        }

        // 扣金币
        if (!SubtractGold(price * buyCount))
        {
            Debug.Log("金币不足，购买失败");
            return false;
        }

        // 更新道具数量
        if (_propCountDict.ContainsKey(propId))
        {
            _propCountDict[propId] += buyCount;
        }
        else
        {
            _propCountDict[propId] = buyCount;
        }

        SaveGameData(); // 自动存档
        Debug.Log($"成功购买道具 {propId} x{buyCount}，当前数量：{_propCountDict[propId]}");
        return true;
    }

    /// <summary>
    /// 使用道具（减少数量，数量为0时移除该道具）
    /// </summary>
    /// <param name="propId">道具唯一ID</param>
    /// <param name="useCount">使用数量（默认1）</param>
    /// <returns>是否使用成功</returns>
    public bool UseProp(string propId, int useCount = 1)
    {
        if (string.IsNullOrEmpty(propId) || useCount < 1)
        {
            Debug.LogWarning("使用道具参数错误");
            return false;
        }

        if (!_propCountDict.ContainsKey(propId) || _propCountDict[propId] < useCount)
        {
            Debug.Log($"道具 {propId} 数量不足，使用失败");
            return false;
        }

        // 减少数量
        _propCountDict[propId] -= useCount;
        // 数量为0时移除键（避免冗余）
        if (_propCountDict[propId] <= 0)
        {
            _propCountDict.Remove(propId);
        }

        SaveGameData(); // 自动存档
        Debug.Log($"成功使用道具 {propId} x{useCount}");
        return true;
    }

    /// <summary>
    /// 获取指定道具的数量
    /// </summary>
    /// <param name="propId">道具唯一ID</param>
    /// <returns>道具数量（无则返回0）</returns>
    public int GetPropCount(string propId)
    {
        if (string.IsNullOrEmpty(propId) || !_propCountDict.ContainsKey(propId))
        {
            return 0;
        }
        return _propCountDict[propId];
    }

    /// <summary>
    /// 获取当前用户拥有的所有道具（ID+数量），以列表形式返回
    /// </summary>
    /// <returns>道具列表（每个元素包含ID和数量）</returns>
    public List<PropData> GetAllProps()
    {
        List<PropData> propList = new List<PropData>();
        foreach (var kvp in _propCountDict)
        {
            propList.Add(new PropData(kvp.Key, kvp.Value));
        }
        return propList;
    }

    /// <summary>
    /// 道具数据模型（用于外部获取道具列表）
    /// </summary>
    [Serializable]
    public class PropData
    {
        public string propId; // 道具唯一ID
        public int count;     // 道具数量

        public PropData(string id, int cnt)
        {
            propId = id;
            count = cnt;
        }
    }
    #endregion
}
