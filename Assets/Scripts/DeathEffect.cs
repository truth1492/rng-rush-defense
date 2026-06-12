using UnityEngine;
using System.Collections;

// ─────────────────────────────────────────────────────────────────────────────
//  DeathEffect
//
//  Attach to each monster prefab.
//  Assign the matching death effect sprite in the Inspector.
//  When TriggerDeath() is called:
//    1. Monster sprite is hidden
//    2. Death effect sprite appears
//    3. Death effect fades out and shrinks over 0.6 seconds
//    4. GameObject is destroyed
// ─────────────────────────────────────────────────────────────────────────────
public class DeathEffect : MonoBehaviour
{
    [Header("Death Effect Sprite")]
    [Tooltip("Assign the matching _death sprite for this monster")]
    public Sprite deathSprite;

    [Header("Settings")]
    public float deathDuration = 0.6f;  // how long death effect lasts
    public float deathScale = 1.5f;  // how big the effect gets before fading

    private SpriteRenderer spriteRenderer;
    private bool isDying = false;
    public bool IsDying => isDying;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Called by EnemyHealth when HP reaches 0
    public void TriggerDeath()
    {
        if (isDying) return;
        isDying = true;

        // Disable movement and combat immediately
        MonsterMovement movement = GetComponent<MonsterMovement>();
        if (movement != null) movement.enabled = false;

        StartCoroutine(PlayDeathEffect());
    }

    IEnumerator PlayDeathEffect()
    {
        // Swap to death sprite
        if (spriteRenderer != null && deathSprite != null)
            spriteRenderer.sprite = deathSprite;

        // Animate — scale up then fade out
        float timer = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = startScale * deathScale;
        Color startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        while (timer < deathDuration)
        {
            float t = timer / deathDuration;

            // Scale up
            if (spriteRenderer != null)
            {
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                // Fade out
                Color c = startColor;
                c.a = Mathf.Lerp(1f, 0f, t);
                spriteRenderer.color = c;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}