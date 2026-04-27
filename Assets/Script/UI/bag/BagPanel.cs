using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class BagPanel : MonoBehaviour
{
    [Header("核心引用")]
    [SerializeField] private ShopSO _shopConfig; // 商店配置表，用于查找PropItemSO
    [SerializeField] private GameObject _bagItemPrefab; // 道具项预制体
    [SerializeField] private Transform _gridTransform; // GridLayoutGroup的父物体
    [SerializeField] private RectTransform _bagPanelRect; // 背包面板的RectTransform

    [Header("详情面板引用")]
    [SerializeField] private TextMeshProUGUI _nameText; // 道具名称文本
    [SerializeField] private TextMeshProUGUI _descText; // 道具描述文本
    [SerializeField] private Button _useBtn; // 使用按钮（功能后续添加）

    [Header("动画设置")]
    [SerializeField] private float _slideDuration = 0.3f; // 滑入/滑出动画时长
    [SerializeField] private float _visibleUIWidth = 500f; // 背包UI宽度
    private List<BagItem> _bagItems = new List<BagItem>(); // 所有生成的道具项列表
    private BagItem _selectedItem; // 当前选中的道具项
    private Vector2 _originalAnchoredPos; // 背包面板的初始锚点位置

    private bool _isBagOpen = false;
    public bool IsBagOpen => _isBagOpen;

    private void Start()
    {
        _originalAnchoredPos = _bagPanelRect.anchoredPosition;
        ClearDetailPanel();
        gameObject.SetActive(false);

        // 绑定使用按钮监听
        if (_useBtn != null)
        {
            _useBtn.onClick.RemoveAllListeners();
            _useBtn.onClick.AddListener(onUserbtnClick);
        }
        else
        {
            Debug.LogError("BagPanel: 使用按钮 _useBtn 未赋值！");
        }
    }

    // 原有方法：OpenBag、CloseBag、ToggleBag 保持不变
    public void OpenBag()
    {
        gameObject.SetActive(true);
        RefreshBagUI();

        _bagPanelRect.anchoredPosition = new Vector2(_originalAnchoredPos.x + _visibleUIWidth, _originalAnchoredPos.y);
        _bagPanelRect.DOAnchorPos(_originalAnchoredPos, _slideDuration).SetEase(Ease.OutQuad);
        _isBagOpen = true;
    }

    public void CloseBag()
    {
        _bagPanelRect.DOAnchorPos(new Vector2(_originalAnchoredPos.x + _visibleUIWidth, _originalAnchoredPos.y), _slideDuration).SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
                _selectedItem = null;
                ClearDetailPanel();
                _isBagOpen = false;
            });
    }

    public void ToggleBag()
    {
        if (_isBagOpen)
        {
            CloseBag();
        }
        else
        {
            OpenBag();
        }
    }

    // 原有方法：RefreshBagUI、ClearAllItems、LoadBagItems 保持不变
    public void RefreshBagUI()
    {
        ClearAllItems();
        LoadBagItems();
    }

    private void ClearAllItems()
    {
        foreach (Transform child in _gridTransform)
        {
            Destroy(child.gameObject);
        }
        _bagItems.Clear();
    }

    private void LoadBagItems()
    {
        if (_shopConfig == null || _bagItemPrefab == null || _gridTransform == null)
        {
            Debug.LogError("BagPanel: 缺少必要的引用配置！");
            return;
        }

        // 从GameDataHub获取当前用户的所有道具
        var userProps = GameDataHub.Instance.GetAllProps();

        foreach (var userProp in userProps)
        {
            // 根据道具名称从ShopSO中查找对应的PropItemSO
            PropItemSO propSO = _shopConfig.sellItems.Find(so => so.itemName == userProp.propId);

            if (propSO != null)
            {
                // 实例化道具项
                GameObject itemObj = Instantiate(_bagItemPrefab, _gridTransform);
                BagItem bagItem = itemObj.GetComponent<BagItem>();

                if (bagItem != null)
                {
                    // 初始化道具项
                    bagItem.InitItem(propSO, userProp.count, this);
                    _bagItems.Add(bagItem);
                }
            }
            else
            {
                Debug.LogWarning($"在ShopConfig中未找到名为 {userProp.propId} 的道具！");
            }
        }
    }

    /// <summary>
    /// 选中一个道具项
    /// </summary>
    public void SelectItem(BagItem item)
    {
        if (_selectedItem != null)
        {
            _selectedItem.SetFocus(false);
        }

        _selectedItem = item;
        _selectedItem.SetFocus(true);
        UpdateDetailPanel(_selectedItem.CurrentProp);
    }

    /// <summary>
    /// 优化后的道具使用逻辑（完全适配GameDataHub的实际方法）
    /// </summary>
    public void onUserbtnClick()
    {
        // 1. 基础空值校验
        if (_selectedItem == null)
        {
            Debug.LogWarning("BagPanel: 未选中任何道具，无法使用！");
            return;
        }

        PropItemSO currentProp = _selectedItem.CurrentProp;
        string propId = currentProp.itemName;
        // 2. 从GameDataHub获取当前道具数量（使用你实际的GetPropCount方法）
        int currentCount = GameDataHub.Instance.GetPropCount(propId);

        // 3. 道具数量校验
        if (currentCount <= 0)
        {
            Debug.LogWarning($"BagPanel: 道具【{propId}】数量不足，无法使用！");
            return;
        }

        Debug.Log($"使用道具：{propId}，当前数量：{currentCount}");

        // 4. 根据道具类型执行对应逻辑
        bool useSuccess = false;
        switch (currentProp.itemType)
        {
            case ItemType.HealthBoost:
                useSuccess = UseHealthBoostProp(currentProp);
                break;
            case ItemType.AttackSpeedBoost:
                useSuccess = UseAttackSpeedBoostProp(currentProp);
                break;
            case ItemType.AttackDamageBoost:
                useSuccess = UseAttackDamageBoostProp(currentProp);
                break;
            case ItemType.EnemySpeedDebuff:
                useSuccess = UseEnemySpeedDebuffProp(currentProp);
                break;
            default:
                Debug.LogWarning($"BagPanel: 未实现的道具类型【{currentProp.itemType}】使用逻辑！");
                break;
        }

        // 5. 使用成功后，调用GameDataHub的UseProp方法扣减数量
        if (useSuccess)
        {
            bool usePropSuccess = GameDataHub.Instance.UseProp(propId, 1);
            if (usePropSuccess)
            {
                UpdatePropCountAfterUse(propId);
            }
        }
    }

    #region 道具使用具体逻辑（无修改，适配原有逻辑）
    private bool UseHealthBoostProp(PropItemSO propSO)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("BagPanel: GameManager.Instance 为空！");
            return false;
        }
        GameManager.Instance.HealPlayer((int)propSO.value);
        Debug.Log($"玩家生命值增加{propSO.value}点！");
        return true;
    }

    private bool UseAttackSpeedBoostProp(PropItemSO propSO)
    {
        BaseTower[] allTowers = GetAllActiveTowers();
        if (allTowers.Length == 0)
        {
            Debug.LogWarning("BagPanel: 场景中无可用的塔，无法使用攻速提升道具！");
            return false;
        }
        foreach (BaseTower tower in allTowers)
        {
            tower.ApplyBuff(Buff.BuffType.AttackSpeed, propSO.value, 5);
        }
        Debug.Log($"所有塔的攻速增加{propSO.value * 100}%，持续5秒！");
        return true;
    }

    private bool UseAttackDamageBoostProp(PropItemSO propSO)
    {
        BaseTower[] allTowers = GetAllActiveTowers();
        if (allTowers.Length == 0)
        {
            Debug.LogWarning("BagPanel: 场景中无可用的塔，无法使用伤害提升道具！");
            return false;
        }
        foreach (BaseTower tower in allTowers)
        {
            tower.ApplyBuff(Buff.BuffType.Damage, propSO.value, 10);
        }
        Debug.Log($"所有塔的伤害增加{propSO.value * 100}%，持续10秒！");
        return true;
    }

    private bool UseEnemySpeedDebuffProp(PropItemSO propSO)
    {
        if (EnemyManager.Instance == null)
        {
            Debug.LogError("BagPanel: EnemyManager.Instance 为空！");
            return false;
        }
        List<BaseEnemy> allEnemies = EnemyManager.Instance.GetAllAliveEnemies();
        if (allEnemies.Count == 0)
        {
            Debug.LogWarning("BagPanel: 场景中无存活敌人，无法使用减速道具！");
            return false;
        }
        foreach (BaseEnemy enemy in allEnemies)
        {
            if (enemy != null && !enemy.IsDead)
            {
                enemy.ApplyBuff(Buff.BuffType.MoveSpeed, propSO.value, 5);
            }
        }
        Debug.Log($"所有敌人的移动速度变化{propSO.value * 100}%，持续5秒！");
        return true;
    }
    #endregion

    #region 辅助方法
    /// <summary>
    /// 获取场景中所有激活的塔
    /// </summary>
    private BaseTower[] GetAllActiveTowers()
    {
        BaseTower[] allTowers = FindObjectsOfType<BaseTower>();
        List<BaseTower> activeTowers = new List<BaseTower>();
        foreach (var tower in allTowers)
        {
            if (tower != null && tower.gameObject.activeInHierarchy)
            {
                activeTowers.Add(tower);
            }
        }
        return activeTowers.ToArray();
    }

    /// <summary>
    /// 使用道具后更新UI（适配GameDataHub的PropData模型）
    /// </summary>
    private void UpdatePropCountAfterUse(string propId)
    {
        int newCount = GameDataHub.Instance.GetPropCount(propId);
        Debug.Log($"道具【{propId}】剩余数量：{newCount}");

        // 数量为0时清空选中状态和详情面板
        if (newCount <= 0)
        {
            _selectedItem = null;
            ClearDetailPanel();
        }
        else
        {
            // 更新当前选中项的显示数量（需BagItem实现UpdateCount方法）
            if (_selectedItem != null)
            {
                _selectedItem.UpdateCount(newCount);
            }
        }

        // 刷新背包UI，保证数据和视图同步
        RefreshBagUI();
    }
    #endregion

    /// <summary>
    /// 更新详情面板
    /// </summary>
    private void UpdateDetailPanel(PropItemSO propSO)
    {
        if (propSO != null)
        {
            _nameText.text = propSO.itemName;
            _descText.text = propSO.desc;
            _useBtn.interactable = true;
        }
        else
        {
            ClearDetailPanel();
        }
    }

    /// <summary>
    /// 清空详情面板
    /// </summary>
    private void ClearDetailPanel()
    {
        _nameText.text = "";
        _descText.text = "Please select a prop to see details.";
        _useBtn.interactable = false;
    }
}