using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BagItem : MonoBehaviour
{
    [Header("组件引用")]
    [SerializeField] private Image _icon; // 道具图标
    [SerializeField] private TextMeshProUGUI _numText; // 道具数量
    [SerializeField] private GameObject _focus; // 焦点高亮图片
    [SerializeField] private Button _itemButton; // 道具按钮

    private PropItemSO _currentProp; // 当前道具的SO
    private int _currentCount; // 当前道具数量
    private BagPanel _bagPanel; // 所属背包面板

    /// <summary>
    /// 初始化道具项
    /// </summary>
    /// <param name="propSO">道具SO</param>
    /// <param name="count">道具数量</param>
    /// <param name="bagPanel">所属背包面板</param>
    public void InitItem(PropItemSO propSO, int count, BagPanel bagPanel)
    {
        _currentProp = propSO;
        _currentCount = count;
        _bagPanel = bagPanel;

        if (_currentProp != null)
        {
            _icon.sprite = _currentProp.icon;
            _numText.text = _currentCount.ToString();
            _focus.SetActive(false); // 初始隐藏焦点

            // 绑定点击事件
            _itemButton.onClick.RemoveAllListeners();
            _itemButton.onClick.AddListener(OnItemClicked);
        }
    }

    /// <summary>
    /// 设置焦点状态
    /// </summary>
    /// <param name="isFocused">是否获得焦点</param>
    public void SetFocus(bool isFocused)
    {
        _focus.SetActive(isFocused);
    }

    /// <summary>
    /// 道具项被点击时的回调
    /// </summary>
    private void OnItemClicked()
    {
        if (_bagPanel != null)
        {
            _bagPanel.SelectItem(this);
        }
    }

    /// <summary>
    /// 获取当前道具的SO
    /// </summary>
    public PropItemSO CurrentProp => _currentProp;
}