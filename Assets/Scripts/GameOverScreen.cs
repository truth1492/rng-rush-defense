using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverScreen : MonoBehaviour
{
    public static bool IsGameOver = false;
    public static GameOverScreen Instance;

    [Header("Screen Root")]
    public GameObject screenRoot;

    [Header("Overlay")]
    public Image overlayImage;

    [Header("Result Banner")]
    public Image resultBannerImage;
    public Sprite defeatBannerSprite;
    public Sprite victoryBannerSprite;
    public TextMeshProUGUI resultTitleText;

    [Header("Info Text")]
    public TextMeshProUGUI waveReachedText;

    [Header("Buttons")]
    public Button restartButton;
    public Button mainMenuButton;

    [Header("Victory Title Offset")]
    public float victoryTitleOffsetY = 0f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        screenRoot.SetActive(false);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    public void ShowDefeat(int waveReached)
    {
        IsGameOver = true;
        screenRoot.SetActive(true);

        if (resultBannerImage != null && defeatBannerSprite != null)
            resultBannerImage.sprite = defeatBannerSprite;

        if (resultTitleText != null)
            resultTitleText.text = "DEFEAT";

        if (waveReachedText != null)
        {
            waveReachedText.gameObject.SetActive(true);
            waveReachedText.text = $"Reached Wave {waveReached}";
        }
    }

    public void ShowVictory(int waveReached)
    {
        IsGameOver = true;
        screenRoot.SetActive(true);

        if (resultBannerImage != null && victoryBannerSprite != null)
            resultBannerImage.sprite = victoryBannerSprite;

        if (resultTitleText != null)
        {
            resultTitleText.text = "VICTORY";
            // Only adjust victory title position
            RectTransform rt = resultTitleText.GetComponent<RectTransform>();
            if (rt != null)
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, victoryTitleOffsetY);
        }

        if (waveReachedText != null)
            waveReachedText.gameObject.SetActive(false);
    }

    void OnRestartClicked()
    {
        IsGameOver = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnMainMenuClicked()
    {
        IsGameOver = false;
        Time.timeScale = 1f;
        if (Application.CanStreamedLevelBeLoaded("MainMenu"))
            SceneManager.LoadScene("MainMenu");
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}