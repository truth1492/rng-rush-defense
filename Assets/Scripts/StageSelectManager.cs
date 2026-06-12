using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StageSelectManager : MonoBehaviour
{
    public static StageSelectManager Instance;

    [Header("Stage Images")]
    public Sprite[] stageSprites;

    [Header("Stage Names")]
    public string[] stageNames = {
        "Timber Trails",
        "Witchwood",
        "Sandstorm Desert",
        "Glacial Ruins",
        "Volcanic Rift",
        "Abandoned Factory",
        "Crystal Cave"
    };

    [Header("Main Menu Stage Display")]
    public Image mainMenuStageImage;
    public TextMeshProUGUI mainMenuStageName;
    public Image mainMenuBackground;

    [Header("Main Menu Background Sprites")]
    public Sprite[] menuBackgrounds;

    [Header("Popup Panel")]
    public GameObject popupPanel;
    public Image popupStageImage;
    public TextMeshProUGUI popupStageName;
    public Button leftArrowButton;
    public Button rightArrowButton;
    public Button selectButton;
    public Button closeButton;

    [Header("Lock UI")]
    public GameObject lockOverlay;    // dark overlay on locked stage
    public Image lockIcon;       // lock icon image
    public TextMeshProUGUI lockText;       // "Complete Stage X" text
    public Sprite lockIconSprite; // assign your lock icon sprite

    private int currentStageIndex = 0;
    private int selectedStageIndex = 0;

    void Awake() { Instance = this; }

    void Start()
    {
        if (leftArrowButton != null) leftArrowButton.onClick.AddListener(OnLeftArrow);
        if (rightArrowButton != null) rightArrowButton.onClick.AddListener(OnRightArrow);
        if (selectButton != null) selectButton.onClick.AddListener(OnSelectStage);
        if (closeButton != null) closeButton.onClick.AddListener(ClosePopup);

        Button stageBtn = mainMenuStageImage?.GetComponent<Button>();
        if (stageBtn != null) stageBtn.onClick.AddListener(OpenPopup);

        selectedStageIndex = PlayerPrefs.GetInt("SelectedStage", 0);

        // Make sure selected stage is still unlocked
        if (!IsStageUnlocked(selectedStageIndex))
            selectedStageIndex = 0;

        currentStageIndex = selectedStageIndex;

        if (popupPanel != null) popupPanel.SetActive(false);

        UpdateMainMenuDisplay();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Stage unlock logic
    // ─────────────────────────────────────────────────────────────────────────
    public bool IsStageUnlocked(int stageIndex)
    {
        if (stageIndex == 0) return true; // Stage 1 always unlocked
        return PlayerPrefs.GetInt($"Stage{stageIndex}Completed", 0) == 1;
    }

    // Called by WaveManager when all 60 waves completed
    public static void CompleteStage(int stageIndex)
    {
        PlayerPrefs.SetInt($"Stage{stageIndex}Completed", 1);
        PlayerPrefs.Save();
        Debug.Log($"[StageSelect] Stage {stageIndex + 1} completed! Stage {stageIndex + 2} unlocked!");
    }

    // ─────────────────────────────────────────────────────────────────────────
    public void OpenPopup()
    {
        currentStageIndex = selectedStageIndex;
        if (popupPanel != null) popupPanel.SetActive(true);
        UpdatePopupDisplay();
    }

    public void ClosePopup()
    {
        if (popupPanel != null) popupPanel.SetActive(false);
    }

    void OnLeftArrow()
    {
        currentStageIndex--;
        if (currentStageIndex < 0)
            currentStageIndex = stageSprites.Length - 1;
        UpdatePopupDisplay();
    }

    void OnRightArrow()
    {
        currentStageIndex++;
        if (currentStageIndex >= stageSprites.Length)
            currentStageIndex = 0;
        UpdatePopupDisplay();
    }

    void OnSelectStage()
    {
        // Only allow selecting unlocked stages
        if (!IsStageUnlocked(currentStageIndex))
        {
            Debug.Log($"[StageSelect] Stage {currentStageIndex + 1} is locked!");
            return;
        }

        selectedStageIndex = currentStageIndex;
        PlayerPrefs.SetInt("SelectedStage", selectedStageIndex);
        PlayerPrefs.Save();
        UpdateMainMenuDisplay();
        ClosePopup();
    }

    // ─────────────────────────────────────────────────────────────────────────
    void UpdatePopupDisplay()
    {
        if (stageSprites == null || stageSprites.Length == 0) return;

        bool isUnlocked = IsStageUnlocked(currentStageIndex);

        // Stage image
        if (popupStageImage != null && stageSprites[currentStageIndex] != null)
        {
            popupStageImage.sprite = stageSprites[currentStageIndex];
            // Grey out if locked
            if (isUnlocked)
                popupStageImage.color = Color.white;
            else if (currentStageIndex == 1) // Stage 2 is naturally darker
                popupStageImage.color = new Color(0.85f, 0.85f, 0.85f, 1f);
            else
                popupStageImage.color = new Color(0.65f, 0.65f, 0.65f, 1f);
        }

        // Stage name
        if (popupStageName != null)
            popupStageName.text = "Stage " + (currentStageIndex + 1) + "\n" + stageNames[currentStageIndex];

        // Lock overlay
        if (lockOverlay != null)
            lockOverlay.SetActive(!isUnlocked);

        // Lock icon
        if (lockIcon != null && lockIconSprite != null)
            lockIcon.sprite = lockIconSprite;

        // Lock text — "Complete Stage X"
        if (lockText != null)
            lockText.text = isUnlocked ? "" : $"Complete Stage {currentStageIndex}";

        // Select button — grey out if locked
        if (selectButton != null)
        {
            selectButton.interactable = isUnlocked;
            Image btnImg = selectButton.GetComponent<Image>();
            if (btnImg != null)
                btnImg.color = isUnlocked ? Color.white : new Color(0.6f, 0.6f, 0.6f, 1f);
        }

        // Arrow buttons
        if (leftArrowButton != null)
            leftArrowButton.gameObject.SetActive(currentStageIndex > 0);
        if (rightArrowButton != null)
            rightArrowButton.gameObject.SetActive(currentStageIndex < stageSprites.Length - 1);
    }

    void UpdateMainMenuDisplay()
    {
        if (stageSprites == null || stageSprites.Length == 0) return;

        if (mainMenuStageImage != null && stageSprites[selectedStageIndex] != null)
            mainMenuStageImage.sprite = stageSprites[selectedStageIndex];

        if (mainMenuStageName != null)
            mainMenuStageName.text = stageNames[selectedStageIndex];

        // Swap main menu background
        if (mainMenuBackground != null && menuBackgrounds != null &&
            selectedStageIndex < menuBackgrounds.Length &&
            menuBackgrounds[selectedStageIndex] != null)
            mainMenuBackground.sprite = menuBackgrounds[selectedStageIndex];
    }

    public int GetSelectedStageIndex() => selectedStageIndex;
}