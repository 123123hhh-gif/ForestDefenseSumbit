using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class BagPanel : MonoBehaviour
{
    [Header("核心引用")]
    [SerializeField] private ShopSO _shopConfig; 
    [SerializeField] private GameObject _bagItemPrefab; 
    [SerializeField] private Transform _gridTransform;
    [SerializeField] private RectTransform _bagPanelRect;

    [Header("详情面板引用")]
    [SerializeField] private TextMeshProUGUI _nameText; 
    [SerializeField] private TextMeshProUGUI _descText; 
    [SerializeField] private Button _useBtn; 

    [Header("动画设置")]
    [SerializeField] private float _slideDuration = 0.3f; 
    [SerializeField] private float _visibleUIWidth = 500f; 
    private List<BagItem> _bagItems = new List<BagItem>(); 
    private BagItem _selectedItem; 
    private Vector2 _originalAnchoredPos; 

    private bool _isBagOpen = false;
    public bool IsBagOpen => _isBagOpen;

    private void Start()
    {
        _originalAnchoredPos = _bagPanelRect.anchoredPosition;
        ClearDetailPanel();
        gameObject.SetActive(false);


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


        var userProps = GameDataHub.Instance.GetAllProps();

        foreach (var userProp in userProps)
        {

            PropItemSO propSO = _shopConfig.sellItems.Find(so => so.itemName == userProp.propId);

            if (propSO != null)
            {

                GameObject itemObj = Instantiate(_bagItemPrefab, _gridTransform);
                BagItem bagItem = itemObj.GetComponent<BagItem>();

                if (bagItem != null)
                {

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


    public void onUserbtnClick()
    {

        if (_selectedItem == null)
        {
            Debug.LogWarning("BagPanel: 未选中任何道具，无法使用！");
            return;
        }

        PropItemSO currentProp = _selectedItem.CurrentProp;
        string propId = currentProp.itemName;

        int currentCount = GameDataHub.Instance.GetPropCount(propId);


        if (currentCount <= 0)
        {
            Debug.LogWarning($"BagPanel: 道具【{propId}】数量不足，无法使用！");
            return;
        }

        Debug.Log($"使用道具：{propId}，当前数量：{currentCount}");


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


    private void UpdatePropCountAfterUse(string propId)
    {
        int newCount = GameDataHub.Instance.GetPropCount(propId);
        Debug.Log($"道具【{propId}】剩余数量：{newCount}");


        if (newCount <= 0)
        {
            _selectedItem = null;
            ClearDetailPanel();
        }
        else
        {

            if (_selectedItem != null)
            {
                _selectedItem.UpdateCount(newCount);
            }
        }


        RefreshBagUI();
    }
    #endregion


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


    private void ClearDetailPanel()
    {
        _nameText.text = "";
        _descText.text = "Please select a prop to see details.";
        _useBtn.interactable = false;
    }
}