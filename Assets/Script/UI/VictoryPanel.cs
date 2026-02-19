using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VictoryPanel : MonoBehaviour
{

    public Image[] starOnImages;  
    private void Awake()
    {

        // HideAllStars();
    }


    public void UpdateStarsByHp(float currentHp, float maxHp)
    {
       
        HideAllStars();
        
        float hpPercent = Mathf.Clamp01(currentHp / maxHp);
        
       Debug.Log("hpPercent = "+hpPercent);
        int starCount = 0;
        if (hpPercent >= 0.7f)      
        {
            starCount = 3;
        }
        else if (hpPercent >= 0.4f) 
        {
            starCount = 2;
        }
        else if (hpPercent > 0)     
        {
            starCount = 1;
        }
        Debug.Log("starCount = "+starCount);
         Debug.Log("starOnImages.Length = "+starOnImages.Length);

        for (int i = 0; i < starCount; i++)
        {

            if (i < starOnImages.Length)
            {
                Debug.Log("i = "+i);
                Debug.Log("starOnImages[i].gameObject = "+ starOnImages[i].gameObject);
                starOnImages[i].gameObject.SetActive(true);
            }
        }
    }


    private void HideAllStars()
    {
        if (starOnImages == null) return;
        foreach (var img in starOnImages)
        {
            if (img != null)
            {
                img.gameObject.SetActive(false);
            }
        }
    }
}
