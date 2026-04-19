using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TowerItem : MonoBehaviour
{

    public TowerData initialData; 
    public TextMeshProUGUI valueTxt;

    public GameObject tipsPanel;

    private TowerData _currentData;
    private float autoHideDelay = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
            tipsPanel.SetActive(false);
           _currentData = initialData;

           valueTxt.text = _currentData.cost+"";
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
            ShowGoldNotEnoughTip();
            
        }
    }


      
    private void ShowGoldNotEnoughTip()
    {
       
        tipsPanel.SetActive(true);
        CancelInvoke(nameof(HideGoldNotEnoughTip));
        Invoke(nameof(HideGoldNotEnoughTip), autoHideDelay);
    }


    private void HideGoldNotEnoughTip()
    {
        tipsPanel.SetActive(false);
    }
}
