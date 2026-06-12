using UnityEngine;
using System.Collections;

// ─────────────────────────────────────────────────────────────────────────────
//  UnitAnimator
//
//  Handles all unit animations purely in code.
//  IMPORTANT: Bob animation moves a VISUAL CHILD object, not the root transform.
//  This means the unit's actual world position never changes — drag and click
//  detection always works correctly.
// ─────────────────────────────────────────────────────────────────────────────
public class UnitAnimator : MonoBehaviour
{
    [Header("Idle Bob")]
    public float bobHeight = 0.04f;
    public float bobSpeed = 2.0f;

    [Header("Attack Lunge")]
    public float lungeDistance = 0.15f;
    public float lungeSpeed = 12f;
    public float returnSpeed = 8f;

    private SpriteRenderer sr;
    private bool defaultFlipX = false; // stores original sprite direction
    private GameObject visualChild;   // child that bobs up/down
    private bool isLunging = false;
    private float bobTimer = 0f;

    // ─────────────────────────────────────────────────────────────────────────
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // Move SpriteRenderer to a child object so bob doesn't affect root pos
        SetupVisualChild();

        // Randomise bob phase so units don't all bob in sync
        bobTimer = Random.Range(0f, Mathf.PI * 2f);
    }

    void SetupVisualChild()
    {
        // Create a child GameObject to hold the visual
        visualChild = new GameObject("Visual");
        visualChild.transform.SetParent(transform);
        visualChild.transform.localPosition = Vector3.zero;
        visualChild.transform.localScale = Vector3.one;
        visualChild.transform.localRotation = Quaternion.identity;

        // Move sprite renderer to child
        if (sr != null)
        {
            // Store original flipX — this is the DEFAULT facing direction
            bool originalFlipX = sr.flipX;

            SpriteRenderer childSr = visualChild.AddComponent<SpriteRenderer>();
            childSr.sprite = sr.sprite;
            childSr.color = sr.color;
            childSr.sortingOrder = sr.sortingOrder;
            childSr.sortingLayerID = sr.sortingLayerID;
            childSr.flipX = originalFlipX;
            childSr.flipY = sr.flipY;

            // Store as default so FaceTarget uses it as base
            defaultFlipX = originalFlipX;

            // Disable original renderer — visual is now on child
            sr.enabled = false;

            // Update sr reference to child
            sr = childSr;
        }

        // Move RarityFootCircle to child too if it exists
        RarityFootCircle fc = GetComponent<RarityFootCircle>();
        if (fc != null)
        {
            fc.transform.SetParent(visualChild.transform);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (!isLunging)
            ApplyIdleBob();
    }

    void ApplyIdleBob()
    {
        bobTimer += Time.deltaTime * bobSpeed;
        float yOffset = Mathf.Sin(bobTimer) * bobHeight;

        if (visualChild != null)
            visualChild.transform.localPosition = new Vector3(0f, yOffset, 0f);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Called by UnitCombat when attacking
    // ─────────────────────────────────────────────────────────────────────────
    public void PlayAttackAnimation(Transform target)
    {
        if (isLunging || target == null || visualChild == null) return;
        StartCoroutine(LungeToward(target));
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Auto flip sprite to face target
    // ─────────────────────────────────────────────────────────────────────────
    public void FaceTarget(Transform target)
    {
        if (sr == null || target == null) return;
        // Account for sprite's default facing direction
        // defaultFlipX=false means sprite faces LEFT by default
        // defaultFlipX=true  means sprite faces RIGHT by default
        bool enemyOnRight = target.position.x > transform.position.x;
        if (defaultFlipX)
            sr.flipX = !enemyOnRight; // right-facing default: flip when enemy is on LEFT
        else
            sr.flipX = enemyOnRight;  // left-facing default: flip when enemy is on RIGHT
    }

    // ─────────────────────────────────────────────────────────────────────────
    IEnumerator LungeToward(Transform target)
    {
        isLunging = true;

        Vector3 startPos = Vector3.zero; // local zero = resting position
        Vector3 direction = (target.position - transform.position).normalized;
        Vector3 lungePos = direction * lungeDistance;
        lungePos.z = 0f;

        // Lunge forward
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * lungeSpeed;
            if (visualChild != null)
                visualChild.transform.localPosition = Vector3.Lerp(startPos, lungePos, Mathf.Clamp01(t));
            yield return null;
        }

        // Snap back
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * returnSpeed;
            if (visualChild != null)
                visualChild.transform.localPosition = Vector3.Lerp(lungePos, startPos, Mathf.Clamp01(t));
            yield return null;
        }

        if (visualChild != null)
            visualChild.transform.localPosition = startPos;

        isLunging = false;
    }
}