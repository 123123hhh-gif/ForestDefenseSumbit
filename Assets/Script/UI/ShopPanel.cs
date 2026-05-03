using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour
{
    [Header("Reference component")]
    [SerializeField] private ShopSO _shopConfig;
    [SerializeField] private GameObject _shopItemPrefab;
    [SerializeField] private Transform _gridTransform;
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private GameObject tipsPanel;
    [SerializeField] private TextMeshProUGUI tipsText;


    private float autoHideDelay = 0.8f;

    void Start()
    {
        tipsPanel.SetActive(false);
    }
    private void OnEnable()
    {
        RefreshShopUI();
        RefreshGoldUI();
    }


    public void RefreshShopUI()
    {
        ClearShopItems();
        LoadShopItems();
    }


    public void RefreshGoldUI()
    {
        _goldText.text = GameDataHub.Instance.Gold.ToString();
    }


    private void ClearShopItems()
    {
        foreach (Transform child in _gridTransform)
        {
            Destroy(child.gameObject);
        }
    }


    private void LoadShopItems()
    {
        if (_shopConfig == null || _shopItemPrefab == null || _gridTransform == null)
        {
            Debug.LogError("ShopPanel: Missing necessary reference configuration！");
            return;
        }

        foreach (var prop in _shopConfig.sellItems)
        {
            if (prop == null) continue;

            GameObject itemObj = Instantiate(_shopItemPrefab, _gridTransform);
            ShopItem shopItem = itemObj.GetComponent<ShopItem>();

            if (shopItem != null)
            {
                shopItem.InitItem(prop, this);
            }
        }
    }


    public void ShowTip(string message)
    {

        tipsText.text = message;
        tipsPanel.transform.SetAsLastSibling();
        tipsPanel.SetActive(true);
        CancelInvoke(nameof(HideGoldNotEnoughTip));
        Invoke(nameof(HideGoldNotEnoughTip), autoHideDelay);
    }


    private void HideGoldNotEnoughTip()
    {
        tipsPanel.SetActive(false);
    }

}