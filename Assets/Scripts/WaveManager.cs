using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [Header("UI Components")]
    public TextMeshProUGUI waveTextDisplay;
    public TextMeshProUGUI timerTextDisplay;
    public TextMeshProUGUI livesTextDisplay;
    public TextMeshProUGUI speedTextDisplay;
    public TextMeshProUGUI goldTextDisplay;
    public TextMeshProUGUI summonButtonTextDisplay;

    [Header("Timer Banner")]
    public Image timerBannerImage;

    [Header("Speed Button")]
    public GameObject speedButton1x;
    public GameObject speedButton2x;

    [Header("Normal Monster Prefabs (7 tiers)")]
    [Tooltip("Index 0 = waves 1-9, Index 1 = waves 11-19, etc.")]
    public GameObject[] normalEnemyPrefabs;

    [Header("Boss Prefabs (7 bosses)")]
    [Tooltip("Index 0 = wave 10, Index 1 = wave 20, etc.")]
    public GameObject[] bossEnemyPrefabs;

    [Header("Wave Settings")]
    public float prepTimeBetweenWaves = 4f;
    public float startCountdown = 4f;
    public float spawnInterval = 0.5f;

    [Header("Lives")]
    public int playerLives = 20;

    [Header("Economy Settings")]
    public int startingGold = 100;
    public int initialSummonCost = 10;
    public int summonCostIncrease = 5;
    public int goldPerKill = 5;
    [Tooltip("Interest rate applied to gold at end of each wave (0.05 = 5%)")]
    public float goldInterestRate = 0.05f;

    private int currentGold;
    private int currentSummonCost;
    private bool isEndlessMode = false;
    private float stageHPMultiplier = 1f;
    private int currentStage = 1;
    private bool isSpawningActive = false;
    private bool isCheckingForClear = false;
    private bool isGameSpeedFast = false;
    private bool isGameOver = false;
    private bool isPrepPhase = false;
    private bool isInitialCountdown = false;
    private float prepTimer = 0f;
    private Transform spawnPoint;
    private GameOverScreen gameOverScreen;

    public int GetCurrentStage() => currentStage;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        GameObject pathObject = GameObject.Find("EnemyPath");
        if (pathObject != null && pathObject.transform.childCount > 0)
            spawnPoint = pathObject.transform.GetChild(0);

        currentGold = startingGold;
        currentSummonCost = initialSummonCost;

        // Check if endless mode
        isEndlessMode = PlayerPrefs.GetInt("EndlessMode", 0) == 1;

        // Load selected stage and apply HP multiplier
        // Endless mode always uses stage 2 difficulty (index 1)
        int selectedStage = isEndlessMode ? 1 : PlayerPrefs.GetInt("SelectedStage", 0);
        stageHPMultiplier = GetStageHPMultiplier(selectedStage);
        Debug.Log($"[WaveManager] Mode: {(isEndlessMode ? "Endless" : "Normal")} Stage: {selectedStage + 1}, HP multiplier: {stageHPMultiplier}");

        // Find GameOverScreen directly in case Instance not set yet
        gameOverScreen = FindAnyObjectByType<GameOverScreen>();
        Debug.Log("[WaveManager] Found GameOverScreen: " + gameOverScreen);

        Time.timeScale = 1f;
        UpdateSpeedButton();
        SetTimerBannerVisible(false);
        UpdateUI();
        StartCoroutine(InitialCountdown());
    }

    void Update()
    {
        if (isGameOver) return;

        if (isPrepPhase)
        {
            prepTimer -= Time.deltaTime;
            UpdateTimerText();

            if (prepTimer <= 0f)
            {
                isPrepPhase = false;
                SetTimerBannerVisible(false);
                StartCoroutine(SpawnWaveRoutine());
            }
            return;
        }

        if (isCheckingForClear && !isSpawningActive)
        {
            if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
                StartPrepPhase();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Get the correct monster prefab for the current stage
    // ─────────────────────────────────────────────────────────────────────────
    GameObject GetNormalPrefabForStage(int stage)
    {
        // Waves 1-9   → index 0
        // Waves 11-19 → index 1
        // Waves 21-29 → index 2
        // Waves 31-39 → index 3
        // Waves 41-49 → index 4
        // Waves 51-59 → index 5
        // Waves 1-9=0, 11-19=1, 21-29=2, 31-39=3, 41-49=4, 51-59=5, 61-69=6
        int tierIndex = Mathf.Clamp((stage - 1) / 10, 0, normalEnemyPrefabs.Length - 1);
        return normalEnemyPrefabs[tierIndex];
    }

    GameObject GetBossPrefabForStage(int stage)
    {
        // Wave 10 → index 0
        // Wave 20 → index 1
        // Wave 30 → index 2
        // Wave 40 → index 3
        // Wave 50 → index 4
        // Wave 60 → index 5
        // Wave 10=0, 20=1, 30=2, 40=3, 50=4, 60=5, 70=6
        int bossIndex = Mathf.Clamp((stage / 10) - 1, 0, bossEnemyPrefabs.Length - 1);
        return bossEnemyPrefabs[bossIndex];
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Boss HP scaling
    // ─────────────────────────────────────────────────────────────────────────
    float GetBossHP(int stage)
    {
        switch (stage)
        {
            case 10: return 10000f;
            case 20: return 20000f;
            case 30: return 40000f;
            case 40: return 70000f;
            case 50: return 110000f;
            case 60: return 160000f;
            case 70: return 220000f;
            default: return 10000f;
        }
    }

    int GetBossGold(int stage)
    {
        switch (stage)
        {
            case 10: return 500;
            case 20: return 700;
            case 30: return 1000;
            case 40: return 1400;
            case 50: return 1900;
            case 60: return 2500;
            case 70: return 0;  // no gold — game ends after final boss
            default: return 500;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    IEnumerator InitialCountdown()
    {
        isInitialCountdown = true;
        SetTimerBannerVisible(true);
        float timer = startCountdown;

        while (timer > 0f)
        {
            if (timerTextDisplay != null)
            {
                int secs = Mathf.CeilToInt(timer);
                timerTextDisplay.text = $"Starting in {secs}s";
            }
            timer -= Time.deltaTime;
            yield return null;
        }

        isInitialCountdown = false;
        SetTimerBannerVisible(false);
        StartCoroutine(SpawnWaveRoutine());
    }

    // ─────────────────────────────────────────────────────────────────────────
    public void EnemyReachedEnd(GameObject enemy)
    {
        if (isGameOver) return;

        EnemyHealth hp = enemy.GetComponent<EnemyHealth>();
        if (hp != null && hp.IsBoss)
        {
            TriggerGameOver();
            return;
        }

        playerLives = Mathf.Max(0, playerLives - 1);
        UpdateUI();

        if (playerLives <= 0)
            TriggerGameOver();
    }

    void StartPrepPhase()
    {
        isCheckingForClear = false;
        isPrepPhase = true;
        prepTimer = prepTimeBetweenWaves;
        currentStage++;

        // Game ends after wave 60 (unless endless mode)
        if (currentStage > 70 && !isEndlessMode)
        {
            TriggerVictory(70); // always show 70 waves completed
            return;
        }

        // Apply gold interest
        ApplyGoldInterest();

        SetTimerBannerVisible(true);
        UpdateUI();
    }

    void ApplyGoldInterest()
    {
        int interest = Mathf.FloorToInt(currentGold * goldInterestRate);
        if (interest > 0)
        {
            currentGold += interest;
            Debug.Log($"[WaveManager] Gold interest: +{interest} (Total: {currentGold})");
        }
    }

    IEnumerator SpawnWaveRoutine()
    {
        isSpawningActive = true;
        isCheckingForClear = false;

        if (currentStage % 10 == 0)
        {
            // Boss wave
            GameObject bossPrefab = GetBossPrefabForStage(currentStage);
            SpawnMonster(bossPrefab, GetBossHP(currentStage) * stageHPMultiplier, GetBossGold(currentStage), isBoss: true);
            yield return null;
        }
        else
        {
            // Fixed 30 monsters per wave
            int monsterCount = 30;
            GameObject normalPrefab = GetNormalPrefabForStage(currentStage);

            for (int i = 0; i < monsterCount; i++)
            {
                SpawnMonster(normalPrefab, (float)currentStage * 100f * stageHPMultiplier, goldPerKill, isBoss: false);
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        isSpawningActive = false;
        isCheckingForClear = true;
        UpdateTimerText();
    }

    [Header("Monster Speed")]
    public float normalMonsterSpeed = 2.5f;
    public float bossMonsterSpeed = 1.5f;

    float GetStageHPMultiplier(int stageIndex)
    {
        // Each stage is harder than the previous
        // Stage 1 = 1.0x, Stage 2 = 1.3x, Stage 3 = 1.7x etc.
        switch (stageIndex)
        {
            case 0: return 1.0f;  // Timber Trails
            case 1: return 1.3f;  // Witchwood
            case 2: return 1.7f;  // Sandstorm Desert
            case 3: return 2.2f;  // Glacial Ruins
            case 4: return 2.8f;  // Volcanic Rift
            case 5: return 3.5f;  // Abandoned Factory
            case 6: return 4.5f;  // Crystal Cave
            default: return 1.0f;
        }
    }

    void SpawnMonster(GameObject prefab, float healthValue, int goldReward, bool isBoss)
    {
        if (prefab == null || spawnPoint == null) return;

        Vector3 pos = new Vector3(spawnPoint.position.x, spawnPoint.position.y, 0f);
        GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);

        // Set movement speed
        MonsterMovement movement = enemy.GetComponent<MonsterMovement>();
        if (movement != null)
            movement.SetSpeed(isBoss ? bossMonsterSpeed : normalMonsterSpeed);

        EnemyHealth hp = enemy.GetComponent<EnemyHealth>()
                      ?? enemy.AddComponent<EnemyHealth>();
        hp.InitializeHealth(healthValue, goldReward, isBoss);
    }

    // ─────────────────────────────────────────────────────────────────────────
    void SetTimerBannerVisible(bool visible)
    {
        if (timerBannerImage != null)
            timerBannerImage.gameObject.SetActive(visible);
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateUI();

        // Show floating gold reward near gold UI
        if (FloatingTextManager.Instance != null)
            FloatingTextManager.Instance.ShowGoldReward(amount);
    }

    public void TrySummonUnit()
    {
        if (isGameOver || currentGold < currentSummonCost) return;

        UnitSpawner gridSpawner = FindAnyObjectByType<UnitSpawner>();
        if (gridSpawner == null) return;

        if (gridSpawner.CanSpawnUnit())
        {
            currentGold -= currentSummonCost;
            currentSummonCost += summonCostIncrease;
            UpdateUI();
            gridSpawner.SpawnRandomUnit();
        }
    }

    public void ToggleGameSpeed()
    {
        if (isGameOver) return;
        isGameSpeedFast = !isGameSpeedFast;
        Time.timeScale = isGameSpeedFast ? 2f : 1f;
        UpdateSpeedButton();
    }

    void UpdateSpeedButton()
    {
        if (speedButton1x != null) speedButton1x.SetActive(!isGameSpeedFast);
        if (speedButton2x != null) speedButton2x.SetActive(isGameSpeedFast);
    }

    void TriggerGameOver()
    {
        Debug.Log("[WaveManager] TriggerGameOver called! Stage: " + currentStage);
        Debug.Log("[WaveManager] GameOverScreen.Instance: " + GameOverScreen.Instance);
        isGameOver = true;
        StopAllCoroutines();
        Time.timeScale = 0f;

        // Hide game UI
        SetTimerBannerVisible(false);

        // Show defeat screen
        if (gameOverScreen != null)
            gameOverScreen.ShowDefeat(currentStage);
        else if (GameOverScreen.Instance != null)
            GameOverScreen.Instance.ShowDefeat(currentStage);
        else
            Debug.LogError("[WaveManager] GameOverScreen not found!");
    }

    void TriggerVictory(int wavesCompleted = -1)
    {
        if (wavesCompleted == -1) wavesCompleted = currentStage;
        isGameOver = true;
        StopAllCoroutines();
        Time.timeScale = 0f;
        SetTimerBannerVisible(false);

        // Award XP for completing all waves
        if (PlayerData.Instance != null)
            PlayerData.Instance.OnGameCompleted(wavesCompleted);

        // Unlock next stage if not endless mode
        if (!isEndlessMode)
        {
            int completedStage = PlayerPrefs.GetInt("SelectedStage", 0);
            StageSelectManager.CompleteStage(completedStage);
        }

        if (gameOverScreen != null)
            gameOverScreen.ShowVictory(wavesCompleted);
        else if (GameOverScreen.Instance != null)
            GameOverScreen.Instance.ShowVictory(wavesCompleted);
    }

    void UpdateTimerText()
    {
        if (isGameOver) return;

        if (isPrepPhase)
        {
            int secs = Mathf.CeilToInt(prepTimer);
            if (timerTextDisplay != null)
                timerTextDisplay.text = $"Next wave {secs}s";
            SetTimerBannerVisible(true);
        }
        else if (!isInitialCountdown)
        {
            SetTimerBannerVisible(false);
        }
    }

    void UpdateUI()
    {
        if (waveTextDisplay != null)
            waveTextDisplay.text = $"WAVE {currentStage}";
        if (livesTextDisplay != null)
            livesTextDisplay.text = $"{playerLives}";
        if (goldTextDisplay != null)
            goldTextDisplay.text = $"{currentGold}";
        if (summonButtonTextDisplay != null)
            summonButtonTextDisplay.text = $"Summon\n({currentSummonCost} G)";
        UpdateTimerText();
    }
}