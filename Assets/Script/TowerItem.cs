//using System.Collections;
//using System.Collections.Generic;

//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class TowerItem : MonoBehaviour
//{


//    public TextMeshProUGUI valueTxt;
//    public TextMeshProUGUI nameTxt;


//    private TowerData _currentData;

//    public Image iconImage;
//    [HideInInspector]
//    public TowersPanel parentPanel;
//    // Start is called before the first frame update
//    void Start()
//    {

//    }

//    public void init(TowerData data)
//    {

//        _currentData = data;
//        valueTxt.text = _currentData.cost + "";
//        nameTxt.text = _currentData.towerName;

//        //load Icon
//        Sprite iconSprite = Resources.Load<Sprite>(_currentData.iconPath);
//        iconImage.sprite = iconSprite;
//    }



//    public void onBtnBuild()
//    {

//        bool isOK = GameManager.Instance.CheckEnoughGold(_currentData.cost);
//        if (isOK)
//        {

//            // GameManager.Instance.
//            UIManager.Instance.startPlaceTower(_currentData);
//        }
//        else
//        {
//            parentPanel.ShowGoldNotEnoughTip();

//        }
//    }




//}






using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerItem : MonoBehaviour
{
    public TextMeshProUGUI valueTxt;
    public TextMeshProUGUI nameTxt;
    private TowerData _currentData;
    public Image iconImage;
    [HideInInspector]
    public TowersPanel parentPanel;

    void Start()
    {
        // 空方法，保持原有结构
    }

    public void init(TowerData data)
    {
        // 检查传入的 TowerData 是否为空
        if (data == null)
        {
            Debug.LogError("TowerData data is null in TowerItem.init!");
            return;
        }

        _currentData = data;

        // 检查文本组件是否为空
        if (valueTxt != null)
        {
            valueTxt.text = _currentData.cost + "";
        }
        else
        {
            Debug.LogError("valueTxt component is not assigned!");
        }

        if (nameTxt != null)
        {
            nameTxt.text = _currentData.towerName;
        }
        else
        {
            Debug.LogError("nameTxt component is not assigned!");
        }

        // 加载图标并检查
        if (iconImage != null)
        {
            Sprite iconSprite = Resources.Load<Sprite>(_currentData.iconPath);
            if (iconSprite != null)
            {
                iconImage.sprite = iconSprite;
            }
            else
            {
                Debug.LogError($"Failed to load icon from path: {_currentData.iconPath}");
            }
        }
        else
        {
            Debug.LogError("iconImage component is not assigned!");
        }
    }

    public void onBtnBuild()
    {
        // 检查关键引用是否为空
        if (_currentData == null)
        {
            Debug.LogError("TowerData is null, cannot build tower!");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null!");
            return;
        }

        bool isOK = GameManager.Instance.CheckEnoughGold(_currentData.cost);
        if (isOK)
        {
            if (UIManager.Instance == null)
            {
                Debug.LogError("UIManager.Instance is null!");
                return;
            }
            UIManager.Instance.startPlaceTower(_currentData);
        }
        else
        {
            if (parentPanel == null)
            {
                Debug.LogError("parentPanel is null!");
                return;
            }
            parentPanel.ShowGoldNotEnoughTip();
        }
    }
}
