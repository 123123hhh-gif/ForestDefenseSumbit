using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class BagPanel : MonoBehaviour
{
    [Header("core citation")]
    [SerializeField] private ShopSO _shopConfig; 
    [SerializeField] private GameObject _bagItemPrefab; 
    [SerializeField] private Transform _gridTransform;
    [SerializeField] private RectTransform _bagPanelRect;

    [Header("Details panel reference")]
    [SerializeField] private TextMeshProUGUI _nameText; 
    [SerializeField] private TextMeshProUGUI _descText; 
    [SerializeField] private Button _useBtn; 

    [Header("Animation Settings")]
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
            Debug.LogError("BagPanel: The button _useBtn has not been assigned a value！");
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
            Debug.LogError("BagPanel: The required reference configurations are missing！");
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
                Debug.LogWarning($"In ShopConfig, no prop found with name {userProp.propId}！");
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
            Debug.LogWarning("BagPanel: No prop selected, cannot use！");
            return;
        }

        PropItemSO currentProp = _selectedItem.CurrentProp;
        string propId = currentProp.itemName;

        int currentCount = GameDataHub.Instance.GetPropCount(propId);


        if (currentCount <= 0)
        {
            Debug.LogWarning($"BagPanel: Prop 【{propId}】 is insufficient in quantity, cannot be used！");
            return;
        }

        Debug.Log($"Using prop: {propId}, Current quantity: {currentCount}");


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
                Debug.LogWarning($"BagPanel: Unimplemented prop type 【{currentProp.itemType}】 usage logic！");
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

    #region Specific logic for using props (no modification, adapting to existing logic)
    private bool UseHealthBoostProp(PropItemSO propSO)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("BagPanel: GameManager.Instance is null！");
            return false;
        }
        GameManager.Instance.HealPlayer((int)propSO.value);
        Debug.Log($"Player health increased by {propSO.value} points！");
        return true;
    }

    private bool UseAttackSpeedBoostProp(PropItemSO propSO)
    {
        BaseTower[] allTowers = GetAllActiveTowers();
        if (allTowers.Length == 0)
        {
            Debug.LogWarning("BagPanel: Scene has no available towers, cannot use attack speed boost prop！");
            return false;
        }
        foreach (BaseTower tower in allTowers)
        {
            tower.ApplyBuff(Buff.BuffType.AttackSpeed, propSO.value, 5);
        }
        Debug.Log($"All towers' attack speed increased by {propSO.value * 100}%, lasting for 5 seconds！");
        return true;
    }

    private bool UseAttackDamageBoostProp(PropItemSO propSO)
    {
        BaseTower[] allTowers = GetAllActiveTowers();
        if (allTowers.Length == 0)
        {
            Debug.LogWarning("BagPanel: Scene has no available towers, cannot use attack damage boost prop！");
            return false;
        }
        foreach (BaseTower tower in allTowers)
        {
            tower.ApplyBuff(Buff.BuffType.Damage, propSO.value, 10);
        }
        Debug.Log($"All towers' damage increased by {propSO.value * 100}%, lasting for 10 seconds！");
        return true;
    }

    private bool UseEnemySpeedDebuffProp(PropItemSO propSO)
    {
        if (EnemyManager.Instance == null)
        {
            Debug.LogError("BagPanel: EnemyManager.Instance is null！");
            return false;
        }
        List<BaseEnemy> allEnemies = EnemyManager.Instance.GetAllAliveEnemies();
        if (allEnemies.Count == 0)
        {
            Debug.LogWarning("BagPanel: Scene has no alive enemies, cannot use slow prop！");
            return false;
        }
        foreach (BaseEnemy enemy in allEnemies)
        {
            if (enemy != null && !enemy.IsDead)
            {
                enemy.ApplyBuff(Buff.BuffType.MoveSpeed, propSO.value, 5);
            }
        }
        Debug.Log($"All enemies' movement speed changed by {propSO.value * 100}%, lasting for 5 seconds！");
        return true;
    }
    #endregion

    #region auxiliary method

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
        Debug.Log($"Prop 【{propId}】 Remaining Quantity: {newCount}");


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