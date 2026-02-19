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
    // Start is called before the first frame update
    void Start()
    {

    }

    public void init(TowerData data)
    {

        _currentData = data;
        valueTxt.text = _currentData.cost + "";
        nameTxt.text = _currentData.towerName;

        //load Icon
        Sprite iconSprite = Resources.Load<Sprite>(_currentData.iconPath);
        iconImage.sprite = iconSprite;
    }



    public void onBtnBuild()
    {

        bool isOK = GameManager.Instance.CheckEnoughGold(_currentData.cost);
        if (isOK)
        {

            // GameManager.Instance.
            UIManager.Instance.startPlaceTower(_currentData);
        }
        else
        {
            parentPanel.ShowGoldNotEnoughTip();

        }
    }
}