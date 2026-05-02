using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPanel : MonoBehaviour
{

    public TextMeshProUGUI userNameText;
    void Start()
    {
        initData();
    }

    public void initData()
    {
        string userName = GameDataHub.Instance.CurrentUserName;
        userNameText.text = userName;
    }


    public void onBtnLevel1()
    {
        SceneManager.LoadScene("L1");
    }
 
    public void onBtnLevel2()
    {
        SceneManager.LoadScene("L2");
    }

    public void onBtnLevel3()
    {
        SceneManager.LoadScene("L3");
    }

    public void onBtnLevel4()
    {
        // SceneManager.LoadScene("L4");
        SceneManager.LoadScene("L4NavMesh");
    }

    public void onBtnLevel5()
    {
        SceneManager.LoadScene("L5");
    }

    public void onBtnLevel6()
    {
        SceneManager.LoadScene("SampleScene");
    }

}
