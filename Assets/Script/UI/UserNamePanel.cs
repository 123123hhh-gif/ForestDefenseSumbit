using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserNamePanel : MonoBehaviour
{

    public TextMeshProUGUI txt_NameTip;
    public TMP_InputField input_Name;
    public Button btn_StartGame;
    public Button btn_SaveName;
    

    void Start()
    {
       
        InitUserNameUI();
        BindButtonEvents();
    }
    private void InitUserNameUI()
    {

        string currentName = PlayerPrefs.GetString(GameDataHub.KEY_CURRENT_USER, "");
        if (!string.IsNullOrEmpty(currentName))
        {
            txt_NameTip.text = $"Current nickname:{currentName}";
            input_Name.text = currentName;
        }
        else
        {
            txt_NameTip.text = "No nickname yet! Please set one first!";
            input_Name.placeholder.GetComponent<TextMeshProUGUI>().text = "Please enter your game nickname";
        }
    }
    private void BindButtonEvents()
    {
        if (btn_StartGame != null)
        {
            btn_StartGame.onClick.AddListener(OnStartGameClick);
        }

        if (btn_SaveName != null)
        {
            btn_SaveName.onClick.AddListener(OnSaveNameClick);
        }
    }
    private void OnStartGameClick()
    {

        string currentName = PlayerPrefs.GetString(GameDataHub.KEY_CURRENT_USER, "");
        if (!string.IsNullOrEmpty(currentName))
        {
            txt_NameTip.text = $"Current nickname:{currentName}\nReady to start the game!";

            GameDataHub.Instance.SwitchUser(currentName); 
            Invoke("startGame", 2f);
        }
        else
        {
            txt_NameTip.text = "No nickname yet! It is recommended to set a nickname before starting the game!";
        }
    }

    private void startGame()
    {
        MainScript.Instance.startGame();
    }


    private void OnSaveNameClick()
    {

        string newName = input_Name.text.Trim();

        if (string.IsNullOrEmpty(newName))
        {
            txt_NameTip.text = "Nickname cannot be empty!";
            return;
        }
        if (newName.Length > 10)
        {
            txt_NameTip.text = "Nickname length cannot exceed 10 characters!";
            return;
        }


        PlayerPrefs.SetString(GameDataHub.KEY_CURRENT_USER, newName);
        PlayerPrefs.Save(); 


        txt_NameTip.text = $"Nickname saved successfully:{newName}";
        Debug.Log($"用户名已保存：{newName}");
    }

    public void ClearUserName()
    {
        PlayerPrefs.DeleteKey(GameDataHub.KEY_CURRENT_USER);
        PlayerPrefs.Save();
        InitUserNameUI(); 
        txt_NameTip.text = "The nickname has been cleared. Please reset it!";
    }
}
