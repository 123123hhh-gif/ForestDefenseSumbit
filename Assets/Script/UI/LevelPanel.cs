using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPanel : MonoBehaviour
{
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
        SceneManager.LoadScene("L4");
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
