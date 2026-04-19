using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("升级面板")]
    public GameObject upgradePanel;
    public Text towerNameText;
    public Text upgradeCostText;
    public Button upgradeButton;
    // public Button closeButton;

    public GameObject victoryPanel;
    public GameObject LosePanel;

    public GameObject SettingPanel;

    public GameObject TowerSelectPanel;

    private BaseTower _currentSelectedTower;

    private TowerPlace _curPlace;



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
        // closeButton.onClick.AddListener(HideUpgradePanel);
    }

    void Start()
    {

        bgmSlider.value = AudioManager.Instance.bgmVolume;
        sfxSlider.value = AudioManager.Instance.battleVolume;
        
        bgmSlider.onValueChanged.AddListener(AudioManager.Instance.SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetBattleVolume);
    }


    public void ShowUpgradePanel(BaseTower tower)
    {
        _currentSelectedTower = tower;
        TowerData nextData = tower.CurrentData.nextLevelData;

        if (nextData == null)
        {
            towerNameText.text = $"{tower.CurrentData.towerName}（满级）";
            upgradeCostText.text = "无";
            upgradeButton.interactable = false;
        }
        else
        {
            towerNameText.text = $"{tower.CurrentData.towerName} → {nextData.towerName}";
            upgradeCostText.text = $"升级费用：{nextData.cost}";
            upgradeButton.interactable = GameManager.Instance.CheckEnoughGold(nextData.cost);
        }

        
        showGameObjectPanel(upgradePanel);
    }


    public void HideUpgradePanel()
    {
        hideGameObjectPanel(upgradePanel);
        _currentSelectedTower = null;
    }

  
    private void OnUpgradeButtonClick()
    {
        if (_currentSelectedTower != null)
        {
            bool upgradeSuccess = _currentSelectedTower.Upgrade();
            if (upgradeSuccess)
            {
                ShowUpgradePanel(_currentSelectedTower); 
            }
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