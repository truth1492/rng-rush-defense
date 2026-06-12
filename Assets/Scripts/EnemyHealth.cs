using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private float maxHP = 1f;
    private float currentHP;
    private int goldReward = 5;
    private bool isBoss = false;
    private MonsterHealthBar healthBar;
    private MonsterAnimator monsterAnimator;

    public void InitializeHealth(float hpValue, int rewardAmount, bool boss = false)
    {
        maxHP = hpValue;
        currentHP = maxHP;
        goldReward = rewardAmount;
        isBoss = boss;

        // Get or create health bar
        healthBar = gameObject.GetComponent<MonsterHealthBar>();
        if (healthBar == null)
            healthBar = gameObject.AddComponent<MonsterHealthBar>();

        if (isBoss)
        {
            healthBar.barWidth = 1.5f;
            healthBar.barOffsetY = 1.4f;
            healthBar.barHeight = 0.2f;
        }

        healthBar.Initialize(maxHP, currentHP);

        // Get monster animator
        monsterAnimator = GetComponent<MonsterAnimator>();
    }

    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        currentHP = Mathf.Max(0f, currentHP);

        // Show damage number
        if (FloatingTextManager.Instance != null)
            FloatingTextManager.Instance.ShowDamage(transform.position, amount);

        // Show health bar on first hit
        if (healthBar != null)
        {
            healthBar.Show();
            healthBar.UpdateHealth(currentHP);
        }

        if (currentHP <= 0)
        {
            if (WaveManager.Instance != null)
                WaveManager.Instance.AddGold(goldReward);

            // Use MonsterAnimator for death if available
            if (monsterAnimator != null)
            {
                monsterAnimator.TriggerDeath();
            }
            else
            {
                // Fallback to old DeathEffect
                DeathEffect deathEffect = GetComponent<DeathEffect>();
                if (deathEffect != null)
                    deathEffect.TriggerDeath();
                else
                    Destroy(gameObject);
            }
        }
    }

    public bool IsBoss => isBoss;
}