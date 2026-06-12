using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Player Info")]
    public Image playerIconImage;
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI playerLevelText;

    [Header("Top Currency")]
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI energyTimerText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI diamondText;

    [Header("Currency Buttons")]
    public Button energyAddButton;
    public Button goldAddButton;
    public Button diamondAddButton;
    public Sprite addIconSprite;  // assign Icon_Add_02

    [Header("Play Buttons")]
    public Button playButton;
    public Button endlessButton;

    [Header("Right Side Buttons")]
    public Button menuButton;
    public Button calendarButton;
    public Button questButton;

    [Header("Bottom Nav Buttons")]
    public Button shopButton;
    public Button unitUpgradeButton;
    public Button homeButton;
    public Button runeButton;
    public Button chestButton;

    [Header("Bottom Nav Labels")]
    public TextMeshProUGUI shopLabel;
    public TextMeshProUGUI unitUpgradeLabel;
    public TextMeshProUGUI homeLabel;
    public TextMeshProUGUI runeLabel;
    public TextMeshProUGUI chestLabel;

    [Header("Nav Button Sprites")]
    public Sprite navButtonNormal;    // dark background
    public Sprite navButtonSelected;  // light background

    [Header("Nav Button Sizes")]
    public float normalHeight = 120f;
    public float selectedHeight = 145f;
    public float normalWidth = 120f;
    public float iconNormalY = 0f;    // icon Y position when normal
    public float iconSelectedY = 8f;   // icon Y position when selected (moves up)

    [Header("Scene Names")]
    public string gameSceneName = "SampleScene";
    public string endlessSceneName = "SampleScene";

    private Button currentSelected;

    void Start()
    {
        // Wire play buttons
        if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
        if (endlessButton != null) endlessButton.onClick.AddListener(OnEndlessClicked);

        // Currency add buttons — redirect to shop
        if (energyAddButton != null) energyAddButton.onClick.AddListener(() => OnNavClicked(shopButton));
        if (goldAddButton != null) goldAddButton.onClick.AddListener(() => OnNavClicked(shopButton));
        if (diamondAddButton != null) diamondAddButton.onClick.AddListener(() => OnNavClicked(shopButton));

        // Wire right side buttons
        if (menuButton != null) menuButton.onClick.AddListener(() => ShowComingSoon("Menu"));
        if (calendarButton != null) calendarButton.onClick.AddListener(() => ShowComingSoon("Calendar"));
        if (questButton != null) questButton.onClick.AddListener(() => ShowComingSoon("Quest"));

        // Wire nav buttons
        if (shopButton != null) shopButton.onClick.AddListener(() => OnNavClicked(shopButton));
        if (unitUpgradeButton != null) unitUpgradeButton.onClick.AddListener(() => OnNavClicked(unitUpgradeButton));
        if (homeButton != null) homeButton.onClick.AddListener(() => OnNavClicked(homeButton));
        if (runeButton != null) runeButton.onClick.AddListener(() => OnNavClicked(runeButton));
        if (chestButton != null) chestButton.onClick.AddListener(() => OnNavClicked(chestButton));

        // Home is selected by default
        SelectNavButton(homeButton);

        UpdateUI();

        if (PlayerData.Instance != null)
            PlayerData.Instance.OnDataChanged += UpdateUI;
    }

    void OnDestroy()
    {
        if (PlayerData.Instance != null)
            PlayerData.Instance.OnDataChanged -= UpdateUI;
    }

    void Update()
    {
        UpdateEnergyTimer();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Nav tab switching
    // ─────────────────────────────────────────────────────────────────────────
    void OnNavClicked(Button clicked)
    {
        SelectNavButton(clicked);

        // Coming soon for non-home buttons
        if (clicked != homeButton)
        {
            string name = GetNavName(clicked);
            Debug.Log($"[MainMenu] {name} — Coming Soon!");
        }
    }

    void SelectNavButton(Button selected)
    {
        currentSelected = selected;

        // Update all 5 nav buttons
        UpdateNavButton(shopButton, shopLabel, selected);
        UpdateNavButton(unitUpgradeButton, unitUpgradeLabel, selected);
        UpdateNavButton(homeButton, homeLabel, selected);
        UpdateNavButton(runeButton, runeLabel, selected);
        UpdateNavButton(chestButton, chestLabel, selected);
    }

    void UpdateNavButton(Button btn, TextMeshProUGUI label, Button selected)
    {
        if (btn == null) return;

        bool isSelected = btn == selected;

        // Swap sprite
        Image img = btn.GetComponent<Image>();
        if (img != null)
            img.sprite = isSelected ? navButtonSelected : navButtonNormal;

        // Resize height
        RectTransform rt = btn.GetComponent<RectTransform>();
        if (rt != null)
            rt.sizeDelta = new Vector2(normalWidth,
                isSelected ? selectedHeight : normalHeight);

        // Show/hide label
        if (label != null)
            label.gameObject.SetActive(isSelected);

        // Move icon up when selected
        Transform icon = btn.transform.Find("Icon");
        if (icon != null)
        {
            RectTransform iconRt = icon.GetComponent<RectTransform>();
            if (iconRt != null)
                iconRt.anchoredPosition = new Vector2(
                    iconRt.anchoredPosition.x,
                    isSelected ? iconSelectedY : iconNormalY);
        }
    }

    string GetNavName(Button btn)
    {
        if (btn == shopButton) return "Shop";
        if (btn == unitUpgradeButton) return "Unit Upgrade";
        if (btn == homeButton) return "Battle";
        if (btn == runeButton) return "Runes";
        if (btn == chestButton) return "Chest";
        return "Unknown";
    }

    // ─────────────────────────────────────────────────────────────────────────
    void UpdateUI()
    {
        if (PlayerData.Instance == null) return;

        if (playerNameText != null) playerNameText.text = PlayerData.Instance.PlayerName;
        if (playerLevelText != null) playerLevelText.text = PlayerData.Instance.PlayerLevel.ToString();

        if (energyText != null)
            energyText.text = $"{PlayerData.Instance.Energy}/{PlayerData.MAX_ENERGY}";
        if (goldText != null)
        {
            int gold = PlayerData.Instance.Gold;
            goldText.text = gold.ToString();
            goldText.fontSize = gold >= 1000000 ? 36 : 42;
            goldText.alignment = gold >= 1000000
                ? TextAlignmentOptions.BottomLeft
                : TextAlignmentOptions.TopLeft;
        }
        if (diamondText != null)
        {
            int diamonds = PlayerData.Instance.Diamonds;
            diamondText.text = diamonds.ToString();
            diamondText.fontSize = diamonds >= 100000 ? 36 : 42;
            diamondText.alignment = diamonds >= 100000
                ? TextAlignmentOptions.BottomLeft
                : TextAlignmentOptions.TopLeft;
        }

        if (playButton != null)
        {
            bool canPlay = PlayerData.Instance.HasEnoughEnergy();
            playButton.interactable = canPlay;
            Image btnImg = playButton.GetComponent<Image>();
            if (btnImg != null)
                btnImg.color = canPlay ? Color.white : new Color(0.6f, 0.6f, 0.6f);
        }
    }

    void UpdateEnergyTimer()
    {
        if (PlayerData.Instance == null || energyTimerText == null) return;

        if (PlayerData.Instance.Energy >= PlayerData.MAX_ENERGY)
        {
            energyTimerText.gameObject.SetActive(false);
            return;
        }

        energyTimerText.gameObject.SetActive(true);
        float secs = PlayerData.Instance.GetSecondsUntilNextEnergy();
        int minutes = Mathf.FloorToInt(secs / 60f);
        int seconds = Mathf.FloorToInt(secs % 60f);
        energyTimerText.text = $"{minutes:00}:{seconds:00}";
    }

    // ─────────────────────────────────────────────────────────────────────────
    void ShowComingSoon(string featureName)
    {
        Debug.Log($"[MainMenu] {featureName} — Coming Soon!");
    }

    void OnPlayClicked()
    {
        if (PlayerData.Instance == null) return;
        if (!PlayerData.Instance.HasEnoughEnergy()) return;

        PlayerData.Instance.SpendEnergy();
        PlayerPrefs.SetInt("EndlessMode", 0);
        int stage = StageSelectManager.Instance != null ? StageSelectManager.Instance.GetSelectedStageIndex() : 0;
        PlayerPrefs.SetInt("SelectedStage", stage);
        PlayerPrefs.SetString("SceneToLoad", gameSceneName);
        PlayerPrefs.Save();
        SceneManager.LoadScene("LoadingScene");
    }

    void OnEndlessClicked()
    {
        if (PlayerData.Instance == null) return;
        if (!PlayerData.Instance.HasEnoughEnergy()) return;

        PlayerData.Instance.SpendEnergy();
        PlayerPrefs.SetInt("EndlessMode", 1);
        PlayerPrefs.SetString("SceneToLoad", endlessSceneName);
        PlayerPrefs.Save();
        SceneManager.LoadScene("LoadingScene");
    }
}