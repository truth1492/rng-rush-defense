using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
//  PlayerData  —  Persistent player data using PlayerPrefs
//  Handles: energy, gold, diamonds, player level, XP, player name
// ─────────────────────────────────────────────────────────────────────────────
public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance;

    // ── Constants ─────────────────────────────────────────────────────────────
    public const int MAX_ENERGY = 30;
    public const int ENERGY_PER_GAME = 5;
    public const float ENERGY_REFILL_SECS = 300f; // 5 minutes per energy

    // ── Events ────────────────────────────────────────────────────────────────
    public System.Action OnDataChanged;

    // ── Properties ───────────────────────────────────────────────────────────
    public int Energy { get; private set; }
    public int Gold { get; private set; }
    public int Diamonds { get; private set; }
    public int PlayerLevel { get; private set; }
    public int PlayerXP { get; private set; }
    public string PlayerName { get; private set; }

    private float energyRefillTimer = 0f;

    // ─────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadData();
    }

    void Update()
    {
        // Energy refill over time
        if (Energy < MAX_ENERGY)
        {
            energyRefillTimer += Time.deltaTime;
            if (energyRefillTimer >= ENERGY_REFILL_SECS)
            {
                energyRefillTimer = 0f;
                AddEnergy(1);
            }
        }
        else
        {
            energyRefillTimer = 0f;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Energy
    // ─────────────────────────────────────────────────────────────────────────
    public bool HasEnoughEnergy() => Energy >= ENERGY_PER_GAME;

    public bool SpendEnergy()
    {
        if (!HasEnoughEnergy()) return false;
        Energy = Mathf.Max(0, Energy - ENERGY_PER_GAME);
        SaveData();
        OnDataChanged?.Invoke();
        return true;
    }

    public void AddEnergy(int amount)
    {
        Energy = Mathf.Min(MAX_ENERGY, Energy + amount);
        SaveData();
        OnDataChanged?.Invoke();
    }

    public float GetEnergyRefillProgress() => energyRefillTimer / ENERGY_REFILL_SECS;
    public float GetSecondsUntilNextEnergy() => ENERGY_REFILL_SECS - energyRefillTimer;

    // ─────────────────────────────────────────────────────────────────────────
    //  Gold and Diamonds (menu currency)
    // ─────────────────────────────────────────────────────────────────────────
    public void AddMenuGold(int amount)
    {
        Gold += amount;
        SaveData();
        OnDataChanged?.Invoke();
    }

    public bool SpendMenuGold(int amount)
    {
        if (Gold < amount) return false;
        Gold -= amount;
        SaveData();
        OnDataChanged?.Invoke();
        return true;
    }

    public void AddDiamonds(int amount)
    {
        Diamonds += amount;
        SaveData();
        OnDataChanged?.Invoke();
    }

    public bool SpendDiamonds(int amount)
    {
        if (Diamonds < amount) return false;
        Diamonds -= amount;
        SaveData();
        OnDataChanged?.Invoke();
        return true;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  XP and Leveling
    // ─────────────────────────────────────────────────────────────────────────
    public void AddXP(int amount)
    {
        PlayerXP += amount;

        // Check for level up
        while (PlayerXP >= GetXPForNextLevel())
        {
            PlayerXP -= GetXPForNextLevel();
            PlayerLevel++;
        }

        SaveData();
        OnDataChanged?.Invoke();
    }

    public int GetXPForNextLevel()
    {
        // XP required increases with level: 100, 150, 200, 250...
        return 100 + (PlayerLevel - 1) * 50;
    }

    public float GetLevelProgress()
    {
        return (float)PlayerXP / GetXPForNextLevel();
    }

    public void SetPlayerName(string name)
    {
        PlayerName = name;
        SaveData();
        OnDataChanged?.Invoke();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Save / Load
    // ─────────────────────────────────────────────────────────────────────────
    void SaveData()
    {
        PlayerPrefs.SetInt("Energy", Energy);
        PlayerPrefs.SetInt("Gold", Gold);
        PlayerPrefs.SetInt("Diamonds", Diamonds);
        PlayerPrefs.SetInt("PlayerLevel", PlayerLevel);
        PlayerPrefs.SetInt("PlayerXP", PlayerXP);
        PlayerPrefs.SetString("PlayerName", PlayerName);
        PlayerPrefs.Save();
    }

    void LoadData()
    {
        Energy = PlayerPrefs.GetInt("Energy", MAX_ENERGY);
        Gold = PlayerPrefs.GetInt("Gold", 0);
        Diamonds = PlayerPrefs.GetInt("Diamonds", 0);
        PlayerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
        PlayerXP = PlayerPrefs.GetInt("PlayerXP", 0);
        PlayerName = PlayerPrefs.GetString("PlayerName", "Player");
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Called when a game session ends — award XP based on wave reached
    // ─────────────────────────────────────────────────────────────────────────
    public void OnGameCompleted(int waveReached)
    {
        int xpReward = waveReached * 10;
        AddXP(xpReward);
    }
}