using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainScript : MonoBehaviour
{

    private static MainScript _instance;

    public static MainScript Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MainScript>();

                if (_instance == null)
                {
                    GameObject singletonObj = new GameObject("MainScript_Singleton");
                    _instance = singletonObj.AddComponent<MainScript>();
                }
            }
            return _instance;
        }
    }


    public AudioClip bgmWarriors;
    public GameObject userNamePanel;
    public GameObject LevelPanel;
    public GameObject BgPanel;
    public GameObject StoryPanel;


    private void Awake()
    {

        if (_instance == null)
        {
            _instance = this;

          //   DontDestroyOnLoad(gameObject);
        }
        else
        {

            if (this != _instance)
            {
                Destroy(gameObject);
            }
        }
    }


    void Start()
    {
        AudioManager.Instance.PlayBGM(bgmWarriors);
        loginState();
    }


    void Update()
    {
        
    }


    public void startGame()
    {
        userNamePanel.SetActive(false);
        LevelPanel.SetActive(true);
        BgPanel.SetActive(false);
        StoryPanel.SetActive(false);
    }

    public void loginState()
    {
        if (GameDataHub.Instance.isLogin)
        {
            userNamePanel.SetActive(false);
            LevelPanel.SetActive(true);
            BgPanel.SetActive(false);
            StoryPanel.SetActive(false);
        }
        else
        {
            userNamePanel.SetActive(false);
            LevelPanel.SetActive(false);
            BgPanel.SetActive(true);
            StoryPanel.SetActive(false);
        }
    }
}