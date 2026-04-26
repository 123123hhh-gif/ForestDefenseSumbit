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
    // 调整为：背包UI部分的宽度（即你希望滑入的那部分宽度）
    [SerializeField] private float _visibleUIWidth = 500f;
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
    /// <param name="item">被选中的道具项</param>
    public void SelectItem(BagItem item)
    {
        // 取消之前选中项的焦点
        if (_selectedItem != null)
        {
            _selectedItem.SetFocus(false);
        }

        // 设置新选中项的焦点
        _selectedItem = item;
        _selectedItem.SetFocus(true);

        // 更新详情面板
        UpdateDetailPanel(_selectedItem.CurrentProp);
    }

    /// <summary>
    /// 更新详情面板显示的信息
    /// </summary>
    /// <param name="propSO">选中的道具SO</param>
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