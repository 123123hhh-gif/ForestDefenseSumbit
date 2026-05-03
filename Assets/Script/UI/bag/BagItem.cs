using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BagItem : MonoBehaviour
{
    [Header("Component Reference")]
    [SerializeField] private Image _icon; 
    [SerializeField] private TextMeshProUGUI _numText; 
    [SerializeField] private GameObject _focus; 
    [SerializeField] private Button _itemButton; 

    private PropItemSO _currentProp; 
    private int _currentCount; 
    private BagPanel _bagPanel; 


    public void InitItem(PropItemSO propSO, int count, BagPanel bagPanel)
    {
        _currentProp = propSO;
        _currentCount = count;
        _bagPanel = bagPanel;

        if (_currentProp != null)
        {
            _icon.sprite = _currentProp.icon;
            _numText.text = _currentCount.ToString();
            _focus.SetActive(false); 


            _itemButton.onClick.RemoveAllListeners();
            _itemButton.onClick.AddListener(OnItemClicked);
        }
    }


    public void SetFocus(bool isFocused)
    {
        _focus.SetActive(isFocused);
    }


    private void OnItemClicked()
    {
        if (_bagPanel != null)
        {
            _bagPanel.SelectItem(this);
        }
    }

    public void UpdateCount(int newCount)
    {
        _currentCount = newCount;
        _numText.text = _currentCount.ToString();
    }


    public PropItemSO CurrentProp => _currentProp;
}