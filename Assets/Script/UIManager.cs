using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("升级面板")]
    public GameObject upgradePanel;
    public TextMeshProUGUI towerNameText;
    public TextMeshProUGUI upgradeCostText;
    public Button upgradeButton;

    public Button sellButton;
    // public Button closeButton;

    public GameObject victoryPanel;
    public GameObject LosePanel;

    public GameObject SettingPanel;

    public GameObject TowerSelectPanel;

    public GameObject tipsPanel;

    private BaseTower _currentSelectedTower;

    private TowerPlace _curPlace;

    private TipsType tipsType;





    public Slider bgmSlider;

    public Slider sfxSlider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

       
        upgradePanel.SetActive(false);
       
        upgradeButton.onClick.AddListener(OnUpgradeButtonClick);
        sellButton.onClick.AddListener(OnSellButtonClick);
        // closeButton.onClick.AddListener(HideUpgradePanel);
    }

    void Start()
    {

        bgmSlider.value = AudioManager.Instance.bgmVolume;
        sfxSlider.value = AudioManager.Instance.battleVolume;
        
        bgmSlider.onValueChanged.AddListener(AudioManager.Instance.SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetBattleVolume);
    }





    public void HideUpgradePanel()
    {
        hideGameObjectPanel(upgradePanel);
        _currentSelectedTower = null;
    }


    public void ShowUpgradePanel(BaseTower tower)
    {
        _currentSelectedTower = tower;
        TowerData nextData = _currentSelectedTower.CurrentData.nextLevelData;

         Debug.Log("加载升级面板，_currentSelectedTower = "+ _currentSelectedTower +" , name = "+ _currentSelectedTower.CurrentData.towerName);

        if (nextData == null)
        {
            towerNameText.text = $"{_currentSelectedTower.CurrentData.towerName}\n → \n(max level)";
            upgradeCostText.text = "no information";
            upgradeButton.interactable = false;
        }
        else
        {
            towerNameText.text = $"{_currentSelectedTower.CurrentData.towerName} \n → \n{nextData.towerName}";
            upgradeCostText.text = $"upgrade cost:{nextData.cost}";
            upgradeButton.interactable = GameManager.Instance.CheckEnoughGold(nextData.cost);
        }

        
        showGameObjectPanel(upgradePanel);
    }

    private void OnSellButtonClick()
    {

        
        ShowTipsPanel(TipsType.SellTower);  
    }
    private void ShowTipsPanel(TipsType _tipsType)
    {

        tipsType = _tipsType;
        string tipsStr = "";

        switch(tipsType)
        {
            case TipsType.SellTower:
                //字符串换行怎么添加
                tipsStr = $"Confirm the sale? \n Selling a defense tower will only return 70% of the spent gold!";
                break;
            default:
                break;
        }

        // Text tipsText = tipsPanel.transform.Find("desc").GetComponent<Text>();

        //查找 tipsPanel 下的 desc 组件 并赋值
        TextMeshProUGUI tipsText = tipsPanel.transform.Find("desc").GetComponent<TextMeshProUGUI>();
        if(tipsText != null)
        {
            tipsText.text = tipsStr;
        }

        tipsPanel.transform.SetAsLastSibling();
        showGameObjectPanel(tipsPanel);
    }

    public void OnSellConfirmed()
    {
        if(tipsType == TipsType.SellTower)
        {
            if (_currentSelectedTower != null)
            {
                _currentSelectedTower.towerPlace.RemoveTower(true);
                HideUpgradePanel();
            }
        }

        hideGameObjectPanel(tipsPanel);

    }

  
    private void OnUpgradeButtonClick()
    {
        if (_currentSelectedTower != null)
        {
            bool isOK = GameManager.Instance.CheckEnoughGold(_currentSelectedTower.CurrentData.nextLevelData.cost);
            if (!isOK)
                return;

            _currentSelectedTower.towerPlace.RemoveTower();

           
            BaseTower newTower = GameManager.Instance.PlaceTower(_currentSelectedTower.towerPlace,_currentSelectedTower.CurrentData.nextLevelData);
           
           Debug.Log("升级成功，newTower = "+ newTower +" , name = "+ newTower.CurrentData.towerName);
            ShowUpgradePanel(newTower);
        }
    }

    public void onResetGame()
    {
        GameManager.Instance.ResetGame();
    }
    public void onCloseTowerSelectPanel()
    {
        _curPlace = null;
        hideGameObjectPanel(TowerSelectPanel);
    }
    public void onTowerSelectPanel(TowerPlace place)
    {
        _curPlace = place;
        showGameObjectPanel(TowerSelectPanel);

        // GameManager.Instance.PlaceTower(this);
    }

    public void onOpenVictory()
    {
        VictoryPanel victoryP = victoryPanel.GetComponent<VictoryPanel>();
        victoryP.UpdateStarsByHp(EnemySpawner.Instance.playerHP,EnemySpawner.Instance.playerHPMax);
        showGameObjectPanel(victoryPanel);
    }
    public void onCloseVictory()
    {
        hideGameObjectPanel(victoryPanel);
    }

    public void onConfirmBtnClick()
    {

        hideGameObjectPanel(tipsPanel);
    }

    public void onOpenLose()
    {
        showGameObjectPanel(LosePanel);
    }
    public void onCloseLose()
    {
        hideGameObjectPanel(LosePanel);
    }

    public void onOpenSetting()
    {
        showGameObjectPanel(SettingPanel);
    }

    public void onCloseSetting()
    {
        hideGameObjectPanel(SettingPanel);
    }

    public void startPlaceTower(TowerData _data)
    {
        GameManager.Instance.PlaceTower(_curPlace,_data);
    }

    public void hideGameObjectPanel(GameObject obj)
    {
        if(obj != null)
        {
            obj.SetActive(false);
        }
        else
        {
            Debug.LogError("obj not value !! obj = "+ obj);
        }

    }
    public void showGameObjectPanel(GameObject obj)
    {
        if(obj != null)
        {
             obj.SetActive(true);
        }
        else
        {
            Debug.LogError("obj not value !! obj = "+ obj);
        }
    }

    public void onSceneSwitcher()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void onTest()
    {
        Debug.Log("23222222222222222");
    }
}


public enum TipsType
{
    None,       
    SellTower,     // 弓箭塔
}