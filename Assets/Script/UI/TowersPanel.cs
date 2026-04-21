using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowersPanel : MonoBehaviour
{
    public LevelData currentLevelData;

    public GameObject gridGroup;

    public GameObject scrollRect;


    public GameObject tipsPanel;

    private float autoHideDelay = 0.5f;

    void Start()
    {
        tipsPanel.SetActive(false);
        init();
    }


    public void init()
    {
        if(currentLevelData == null)
        {
            Debug.LogError("currentLevelData is null");
            return;
        }
            
        if(currentLevelData.availableTowers.Count <= 0)
        {
            Debug.LogError("currentLevelData.availableTowers is empty");
             return;
        }

        if(currentLevelData.towerItemPrefab == null)
        {
            Debug.LogError("currentLevelData.towerItemPrefab is null");
             return;
        }
           

        for(int i=0;i< currentLevelData.availableTowers.Count;i++)
        {
            TowerData data = currentLevelData.availableTowers[i];
            GameObject itemObj = Instantiate(currentLevelData.towerItemPrefab,gridGroup.transform);
            TowerItem itemComp = itemObj.GetComponent<TowerItem>();
            itemComp.parentPanel = this;
            itemComp.init(data);
        }

    }   


    public void ShowGoldNotEnoughTip()
    {
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