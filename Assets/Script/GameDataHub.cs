using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameDataHub : MonoBehaviour
{
    private static GameDataHub _instance;
    private const string DEFAULT_USER_NAME = "Guest";
    public const string KEY_CURRENT_USER = "Game_User_Name";

    public bool isLogin = false;


    private const string RANK_DATA_KEY = "PlayerRankData";

    private const int MAX_RANK_COUNT = 10;


    private List<PlayerData> _rankList = new List<PlayerData>();


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

        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);


        LoadGameData();
        LoadRankData();
    }


    private string GetUserPrefKey(string baseKey)
    {
        return $"{CurrentUserName}_{baseKey}";
    }


    public void SwitchUser(string newUserName)
    {
        CurrentUserName = newUserName;
    }


    public void SetCurrentLevel(int level)
    {
        if (level < 1) return; 
        CurrentLevel = level;
        SaveGameData();
    }


    public void AddGold(int amount)
    {
        if (amount < 0) return; 
        Gold += amount;
        SaveGameData();
    }


    public bool SubtractGold(int amount)
    {
        if (amount < 0 || Gold < amount) return false;
        Gold -= amount;
        SaveGameData();
        return true;
    }

    private void LoadGameData()
    {

        if (PlayerPrefs.HasKey(GetUserPrefKey("CurrentLevel")))
            CurrentLevel = PlayerPrefs.GetInt(GetUserPrefKey("CurrentLevel"));
        else
            CurrentLevel = 1; 


        if (PlayerPrefs.HasKey(GetUserPrefKey("Gold")))
            Gold = PlayerPrefs.GetInt(GetUserPrefKey("Gold"));
        else
            Gold = 0; 


        LoadPropData();
    }


    private void SaveGameData()
    {

        PlayerPrefs.SetInt(GetUserPrefKey("CurrentLevel"), CurrentLevel);

        PlayerPrefs.SetInt(GetUserPrefKey("Gold"), Gold);


        SavePropData();

        PlayerPrefs.Save(); 
    }


    public void ResetGameData()
    {
        CurrentLevel = 1;
        Gold = 0;
       
        _propCountDict.Clear();
        SaveGameData();
    }

    public void DeleteUserData(string userName)
    {
        string validName = string.IsNullOrEmpty(userName) ? DEFAULT_USER_NAME : userName.Trim();

        PlayerPrefs.DeleteKey($"{validName}_CurrentLevel");
        PlayerPrefs.DeleteKey($"{validName}_Gold");

        PlayerPrefs.DeleteKey($"{validName}_PurchasedProps");

        PlayerPrefs.Save();

        if (validName == CurrentUserName)
        {
            LoadGameData();
        }
    }

    #region RankIng System

    private void LoadRankData()
    {
        try
        {

            string jsonData = PlayerPrefs.GetString(RANK_DATA_KEY, "");

            if (!string.IsNullOrEmpty(jsonData))
            {

                _rankList = JsonUtility.FromJson<RankDataWrapper>(jsonData).playerList;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("加载排行榜数据失败: " + e.Message);
            _rankList = new List<PlayerData>();
        }
    }


    private void SaveRankData()
    {
        try
        {

            RankDataWrapper wrapper = new RankDataWrapper();
            wrapper.playerList = _rankList;

            string jsonData = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(RANK_DATA_KEY, jsonData);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogError("保存排行榜数据失败: " + e.Message);
        }
    }


    public void UpdatePlayerStars(string playerId, int addStars)
    {
        if (addStars < 0)
        {
            Debug.LogWarning("新增星数不能为负数");
            return;
        }


        PlayerData targetPlayer = _rankList.FirstOrDefault(p => p.playerId == playerId);

        if (targetPlayer != null)
        {

            targetPlayer.totalStars += addStars;
        }
        else
        {

            _rankList.Add(new PlayerData(playerId, addStars));
        }


        SortAndLimitRankList();


        SaveRankData();

        Debug.Log($"玩家 {playerId} 星数已更新，当前总星数: {GetPlayerTotalStars(playerId)}");
    }


    private void SortAndLimitRankList()
    {

        _rankList = _rankList.OrderByDescending(p => p.totalStars).ToList();


        if (_rankList.Count > MAX_RANK_COUNT)
        {
            _rankList = _rankList.Take(MAX_RANK_COUNT).ToList();
        }
    }


    public List<PlayerData> GetRankList()
    {

        return new List<PlayerData>(_rankList);
    }


    public int GetPlayerTotalStars(string playerId)
    {
        PlayerData targetPlayer = _rankList.FirstOrDefault(p => p.playerId == playerId);
        return targetPlayer?.totalStars ?? 0;
    }


    public void ClearRankData()
    {
        _rankList.Clear();
        PlayerPrefs.DeleteKey(RANK_DATA_KEY);
        PlayerPrefs.Save();
        Debug.Log("排行榜数据已清空");
    }


    [Serializable]
    private class RankDataWrapper
    {
        public List<PlayerData> playerList = new List<PlayerData>();
    }
    #endregion

    #region Prop System

    private void LoadPropData()
    {
        _propCountDict.Clear();
        string purchasedStr = PlayerPrefs.GetString(GetUserPrefKey("PurchasedProps"), "");
        if (!string.IsNullOrEmpty(purchasedStr))
        {

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


    private void SavePropData()
    {

        List<string> propStrList = new List<string>();
        foreach (var kvp in _propCountDict)
        {
            propStrList.Add($"{kvp.Key}:{kvp.Value}");
        }
        string purchasedStr = string.Join(",", propStrList);
        PlayerPrefs.SetString(GetUserPrefKey("PurchasedProps"), purchasedStr);
    }

    public bool PurchaseProp(string propId, int price, int buyCount = 1)
    {
        if (string.IsNullOrEmpty(propId) || price < 0 || buyCount < 1)
        {
            Debug.LogWarning("购买道具参数错误");
            return false;
        }


        if (!SubtractGold(price * buyCount))
        {
            Debug.Log("金币不足，购买失败");
            return false;
        }


        if (_propCountDict.ContainsKey(propId))
        {
            _propCountDict[propId] += buyCount;
        }
        else
        {
            _propCountDict[propId] = buyCount;
        }

        SaveGameData(); 
        Debug.Log($"成功购买道具 {propId} x{buyCount}，当前数量：{_propCountDict[propId]}");
        return true;
    }


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


        _propCountDict[propId] -= useCount;

        if (_propCountDict[propId] <= 0)
        {
            _propCountDict.Remove(propId);
        }

        SaveGameData();
        Debug.Log($"成功使用道具 {propId} x{useCount}");
        return true;
    }


    public int GetPropCount(string propId)
    {
        if (string.IsNullOrEmpty(propId) || !_propCountDict.ContainsKey(propId))
        {
            return 0;
        }
        return _propCountDict[propId];
    }


    public List<PropData> GetAllProps()
    {
        List<PropData> propList = new List<PropData>();
        foreach (var kvp in _propCountDict)
        {
            propList.Add(new PropData(kvp.Key, kvp.Value));
        }
        return propList;
    }


    [Serializable]
    public class PropData
    {
        public string propId; 
        public int count;   

        public PropData(string id, int cnt)
        {
            propId = id;
            count = cnt;
        }
    }
    #endregion
}
