using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Upgrade Panel")]
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

        // Hide upgrade panel by default
        upgradePanel.SetActive(false);
        // Register button click events
        upgradeButton.onClick.AddListener(OnUpgradeButtonClick);
        sellButton.onClick.AddListener(OnSellButtonClick);
        // closeButton.onClick.AddListener(HideUpgradePanel);
    }

    void Start()
    {
        //bgmSlider.value = AudioManager.Instance.bgmVolume;
        //sfxSlider.value = AudioManager.Instance.battleVolume;

        //bgmSlider.onValueChanged.AddListener(AudioManager.Instance.SetBGMVolume);
        //sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetBattleVolume);
    }

    // Hide upgrade panel and clear selected tower reference
    public void HideUpgradePanel()
    {
        hideGameObjectPanel(upgradePanel);
        _currentSelectedTower = null;
    }

    // Show upgrade panel with tower data
    public void ShowUpgradePanel(BaseTower tower)
    {
        _currentSelectedTower = tower;
        TowerData nextData = _currentSelectedTower.CurrentData.nextLevelData;

        Debug.Log("Loading upgrade panel, _currentSelectedTower = " + _currentSelectedTower + " , name = " + _currentSelectedTower.CurrentData.towerName);

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

        // Show upgrade panel
        showGameObjectPanel(upgradePanel);
    }

    // Triggered when sell button is clicked
    private void OnSellButtonClick()
    {
        // Show sell confirmation tips panel
        ShowTipsPanel(TipsType.SellTower);
    }

    // Show tips panel with specified type and content
    private void ShowTipsPanel(TipsType _tipsType)
    {
        tipsType = _tipsType;
        string tipsStr = "";

        switch (tipsType)
        {
            case TipsType.SellTower:
                // How to add line breaks to string
                tipsStr = $"Confirm the sale? \n Selling a defense tower will only return 70% of the spent gold!";
                break;
            default:
                break;
        }

        // Text tipsText = tipsPanel.transform.Find("desc").GetComponent<Text>();

        // Find the desc component under tipsPanel and assign value
        TextMeshProUGUI tipsText = tipsPanel.transform.Find("desc").GetComponent<TextMeshProUGUI>();
        if (tipsText != null)
        {
            tipsText.text = tipsStr;
        }

        tipsPanel.transform.SetAsLastSibling();
        showGameObjectPanel(tipsPanel);
    }

    // Confirm to sell the selected tower
    public void OnSellConfirmed()
    {
        if (tipsType == TipsType.SellTower)
        {
            if (_currentSelectedTower != null)
            {
                _currentSelectedTower.towerPlace.RemoveTower(true);
                HideUpgradePanel();
            }
        }

        hideGameObjectPanel(tipsPanel);
    }

    // Triggered when upgrade button is clicked
    private void OnUpgradeButtonClick()
    {
        if (_currentSelectedTower != null)
        {
            bool isOK = GameManager.Instance.CheckEnoughGold(_currentSelectedTower.CurrentData.nextLevelData.cost);
            if (!isOK)
                return;

            _currentSelectedTower.towerPlace.RemoveTower();

            // Place upgraded tower at the same position
            BaseTower newTower = GameManager.Instance.PlaceTower(_currentSelectedTower.towerPlace, _currentSelectedTower.CurrentData.nextLevelData);

            Debug.Log("Upgrade successful, newTower = " + newTower + " , name = " + newTower.CurrentData.towerName);
            ShowUpgradePanel(newTower);
        }
    }

    // Reset game state
    public void onResetGame()
    {
        GameManager.Instance.ResetGame();
    }

    // Close tower selection panel
    public void onCloseTowerSelectPanel()
    {
        _curPlace = null;
        hideGameObjectPanel(TowerSelectPanel);
    }

    // Open tower selection panel for specific tower place
    public void onTowerSelectPanel(TowerPlace place)
    {
        _curPlace = place;
        showGameObjectPanel(TowerSelectPanel);

        //GameManager.Instance.PlaceTower(this);
    }

    // Open victory panel
    public void onOpenVictory()
    {
        //VictoryPanel victoryP = victoryPanel.GetComponent<VictoryPanel>();
        //victoryP.UpdateStarsByHp(EnemySpawner.Instance.playerHP,EnemySpawner.Instance.playerHPMax);
        //showGameObjectPanel(victoryPanel);
    }

    // Close victory panel
    public void onCloseVictory()
    {
        hideGameObjectPanel(victoryPanel);
    }

    // Triggered when confirm button is clicked (tips panel)
    public void onConfirmBtnClick()
    {
        hideGameObjectPanel(tipsPanel);
    }

    // Open lose panel
    public void onOpenLose()
    {
        showGameObjectPanel(LosePanel);
    }

    // Close lose panel
    public void onCloseLose()
    {
        hideGameObjectPanel(LosePanel);
    }

    // Open setting panel
    public void onOpenSetting()
    {
        showGameObjectPanel(SettingPanel);
    }

    // Close setting panel
    public void onCloseSetting()
    {
        hideGameObjectPanel(SettingPanel);
    }

    // Start placing tower with specified tower data
    public void startPlaceTower(TowerData _data)
    {
        GameManager.Instance.PlaceTower(_curPlace, _data);
    }

    // Hide specified game object panel
    public void hideGameObjectPanel(GameObject obj)
    {
        if (obj != null)
        {
            obj.SetActive(false);
        }
        else
        {
            Debug.LogError("obj is null !! obj = " + obj);
        }
    }

    // Show specified game object panel
    public void showGameObjectPanel(GameObject obj)
    {
        if (obj != null)
        {
            obj.SetActive(true);
        }
        else
        {
            Debug.LogError("obj is null !! obj = " + obj);
        }
    }

    // Switch to main scene
    public void onSceneSwitcher()
    {
        SceneManager.LoadScene("MainScene");
    }

    // Test method
    public void onTest()
    {
        Debug.Log("23222222222222222");
    }
}

// Tips panel type enumeration
public enum TipsType
{
    None,
    SellTower,     // Sell tower confirmation
}