using UnityEngine;
using System.Collections;

// ─────────────────────────────────────────────────────────────────────────────
//  MonsterAnimator
//  Handles walk and death animations for monsters using Animator.
//  Attach to monster prefabs alongside EnemyHealth.
// ─────────────────────────────────────────────────────────────────────────────
public class MonsterAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    public float deathAnimDuration = 0.6f;

    [Header("Death Effect")]
    public GameObject deathPrefab;        // assign your death prefab here
    public Vector3 deathPrefabOffset = Vector3.zero;
    public Vector3 deathPrefabScale = Vector3.one;

    private Animator animator;
    private SpriteRenderer sr;
    private Vector3 lastPosition;
    private bool isDead = false;

    // Animator parameter
    private static readonly int IsDeadParam = Animator.StringToHash("IsDead");

    void Start()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        lastPosition = transform.position;


    }

    void Update()
    {
        if (isDead) return;

        AutoFlip();
        lastPosition = transform.position;
    }


    // ─────────────────────────────────────────────────────────────────────────
    //  Auto flip based on movement direction
    // ─────────────────────────────────────────────────────────────────────────
    void AutoFlip()
    {
        if (sr == null) return;
        float moveX = transform.position.x - lastPosition.x;
        if (Mathf.Abs(moveX) > 0.001f)
            sr.flipX = moveX < 0;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Called by EnemyHealth when monster dies
    // ─────────────────────────────────────────────────────────────────────────
    public void TriggerDeath()
    {
        if (isDead) return;
        isDead = true;

        // Stop movement
        MonsterMovement movement = GetComponent<MonsterMovement>();
        if (movement != null) movement.enabled = false;

        // Hide health bar
        MonsterHealthBar healthBar = GetComponent<MonsterHealthBar>();
        if (healthBar != null) healthBar.Hide();

        // Hide the monster's own sprite renderer
        if (sr != null) sr.enabled = false;

        // Disable animator to stop walk animation showing
        if (animator != null)
        {
            animator.enabled = false;
        }

        // Spawn separate death prefab if assigned
        if (deathPrefab != null)
        {
            Vector3 spawnPos = transform.position + deathPrefabOffset;
            GameObject death = Instantiate(deathPrefab, spawnPos, Quaternion.identity);
            death.transform.localScale = deathPrefabScale;
            Destroy(death, deathAnimDuration + 0.5f);
        }

        // Destroy after animation completes
        StartCoroutine(DestroyAfterDeath());
    }

    IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(deathAnimDuration);
        Destroy(gameObject);
    }
}