using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{

    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _descText;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private TextMeshProUGUI _curNumText;
    [SerializeField] private Button _buyButton;


    private PropItemSO _currentProp;
    private ShopPanel _shopPanel;
    public void InitItem(PropItemSO prop, ShopPanel shopPanel)
    {
        _currentProp = prop;
        _shopPanel = shopPanel;

        if (_currentProp != null)
        {
            _icon.sprite = _currentProp.icon;
            _nameText.text = _currentProp.itemName;
            _descText.text = _currentProp.desc;
            _priceText.text = _currentProp.price.ToString();
            RefreshCurNum();

            _buyButton.onClick.RemoveAllListeners();
            _buyButton.onClick.AddListener(OnBuyClicked);
        }
    }

    public void RefreshCurNum()
    {
        if (_currentProp != null)
        {
            int count = GameDataHub.Instance.GetPropCount(_currentProp.itemName);
            _curNumText.text = count.ToString();
        }
    }


    private void OnBuyClicked()
    {
        if (_currentProp == null || _shopPanel == null) return;

        bool success = GameDataHub.Instance.PurchaseProp(_currentProp.itemName, _currentProp.price);

        if (success)
        {
            RefreshCurNum();
            _shopPanel.RefreshGoldUI();
            _shopPanel.ShowTip($"Successfully purchased item");
        }
        else
        {
            _shopPanel.ShowTip($"Failed to purchase item: Not enough gold!");
        }
    }
}