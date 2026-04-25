using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RankPanel : MonoBehaviour
{

    public List<GameObject> rankItemList;


    private const string EMPTY_TEXT = "Waiting for you";

    void Start()
    {
  
        RefreshRankUI();
    }

    public void RefreshRankUI()
    {
        List<PlayerData> rankData = GameDataHub.Instance.GetRankList();


        for (int i = 0; i < rankItemList.Count; i++)
        {

            GameObject item = rankItemList[i];
            if (item == null) continue;

            TextMeshProUGUI userNameText = null;
            Transform txtTransform = item.transform.Find("userNameTxt");
            if (txtTransform != null)
            {
                userNameText = txtTransform.GetComponent<TextMeshProUGUI>();
            }

            if (i < rankData.Count)
            {

                if (userNameText != null)
                {
                    userNameText.text = rankData[i].playerId;
                }

                item.SetActive(true);
            }
            else
            {

                if (userNameText != null)
                {
                    userNameText.text = EMPTY_TEXT;
                }

                // item.SetActive(false);
            }
        }
    }

    public void OnPlayerStarsUpdated()
    {
        RefreshRankUI();
    }
}