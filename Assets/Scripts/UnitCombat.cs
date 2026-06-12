using UnityEngine;

public class UnitCombat : MonoBehaviour
{
    [Header("Ranged Only")]
    public GameObject projectilePrefab;

    private UnitData unitData;
    private float attackTimer = 0f;
    private Transform currentTarget;

    private GameObject rangeCircle;
    private bool showingRange = false;

    // Drag detection
    private const float DragThreshold = 0.15f;
    private Vector3 pressWorldPos = Vector3.zero;
    private bool isPressing = false;
    private bool pressWasOnMe = false;

    private UnitDragHandler dragHandler;
    private RarityFootCircle footCircle;
    private CharacterUnitAnimator charAnimator;
    private UnitAnimator unitAnimator;

    // ─────────────────────────────────────────────────────────────────────────
    public void Initialize(UnitData data)
    {
        unitData = data;
        dragHandler = GetComponent<UnitDragHandler>();
        CreateRangeCircle();

        // Create rarity foot circle
        footCircle = gameObject.AddComponent<RarityFootCircle>();
        footCircle.Initialize(data.rarity);

        // Add animator
        unitAnimator = gameObject.AddComponent<UnitAnimator>();

        // Check for character maker animator
        charAnimator = GetComponent<CharacterUnitAnimator>();
        if (charAnimator != null)
            charAnimator.OnAttackHit += OnCharacterAttackHit;
    }

    // Gets the visual center of an enemy - transform.position already includes yOffset
    Vector3 GetEnemyCenter(GameObject enemy)
    {
        return enemy.transform.position;
    }

    public UnitData GetUnitData() => unitData;

    void OnDestroy()
    {
        if (charAnimator != null)
            charAnimator.OnAttackHit -= OnCharacterAttackHit;
    }

    void OnCharacterAttackHit()
    {
        if (currentTarget == null) return;
        if (unitData.unitType == UnitType.Melee) MeleeAttack();
        else RangedAttack();
    }

    // ─────────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (unitData == null) return;

        // Don't attack while being dragged
        bool currentlyDragging = dragHandler != null && dragHandler.IsDragging;

        if (!currentlyDragging)
        {
            FindClosestTarget();

            if (currentTarget != null)
            {
                attackTimer += Time.deltaTime;
                if (attackTimer >= unitData.attackCooldown)
                {
                    Attack();
                    attackTimer = 0f;
                }
            }
        }
        else
        {
            // While dragging, set timer near full so unit attacks immediately on drop
            currentTarget = null;
            if (unitData != null)
                attackTimer = unitData.attackCooldown * 0.7f;
        }

        HandleClickInput();
    }

    // ─────────────────────────────────────────────────────────────────────────
    void FindClosestTarget()
    {
        // Clear dead target
        if (currentTarget != null)
        {
            if (currentTarget.gameObject == null)
                currentTarget = null;
            else if (currentTarget.GetComponent<EnemyHealth>() == null)
                currentTarget = null;
        }

        // Use physics overlap circle to find enemies in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position, unitData.attackRange);

        float shortestDist = Mathf.Infinity;
        GameObject nearest = null;

        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;
            GameObject enemy = hit.gameObject;

            EnemyHealth hp = enemy.GetComponent<EnemyHealth>();
            if (hp == null) continue;

            DeathEffect de = enemy.GetComponent<DeathEffect>();
            if (de != null && de.IsDying) continue;

            float dist = Vector3.Distance(transform.position, hit.bounds.center);
            if (dist < shortestDist)
            {
                shortestDist = dist;
                nearest = enemy;
            }
        }

        currentTarget = nearest != null ? nearest.transform : null;
    }

    // ─────────────────────────────────────────────────────────────────────────
    void Attack()
    {
        if (currentTarget == null) return;

        // Only flip to face enemy when actually attacking
        if (unitAnimator != null)
            unitAnimator.FaceTarget(currentTarget);

        // If using character animator, it handles damage via OnAttackHit event
        if (charAnimator != null)
        {
            charAnimator.FaceTarget(currentTarget);
            charAnimator.PlayAttack();
            return; // damage dealt by OnCharacterAttackHit
        }

        if (unitData.unitType == UnitType.Melee) MeleeAttack();
        else RangedAttack();
    }

    void MeleeAttack()
    {
        EnemyHealth hp = currentTarget.GetComponent<EnemyHealth>();
        if (hp != null) hp.TakeDamage(unitData.attackDamage);
        if (unitAnimator != null) unitAnimator.PlayAttackAnimation(currentTarget);
    }

    void RangedAttack()
    {
        if (projectilePrefab == null) return;

        Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y, 0f);
        GameObject arrow = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        SimpleBullet bullet = arrow.GetComponent<SimpleBullet>()
                           ?? arrow.AddComponent<SimpleBullet>();

        // Use offset center for better accuracy on bosses
        Vector3 targetCenter = GetEnemyCenter(currentTarget.gameObject);
        bullet.SetTargetPosition(currentTarget, targetCenter, unitData.attackDamage);
        if (unitAnimator != null) unitAnimator.PlayAttackAnimation(currentTarget);
    }

    // ─────────────────────────────────────────────────────────────────────────
    void HandleClickInput()
    {
        if (InputManager.Instance == null) return;

        // Disable clicking when game is over
        if (GameOverScreen.IsGameOver) return;

        if (InputManager.Instance.ConsumedByUI)
        {
            isPressing = false;
            pressWasOnMe = false;
            return;
        }

        Vector3 wp = InputManager.Instance.WorldPosition;

        if (InputManager.Instance.PressedThisFrame)
        {
            pressWorldPos = wp;
            isPressing = true;
            pressWasOnMe = Vector3.Distance(transform.position, wp) < 0.45f;
        }

        if (InputManager.Instance.ReleasedThisFrame && isPressing)
        {
            isPressing = false;

            float movedDist = Vector3.Distance(pressWorldPos, wp);
            bool wasDrag = movedDist >= DragThreshold;

            if (wasDrag)
            {
                pressWasOnMe = false;
                return;
            }

            if (pressWasOnMe)
            {
                if (showingRange)
                    CloseCircle();
                else
                    OpenCircle();
            }
            else if (showingRange)
            {
                CloseCircle();
            }

            pressWasOnMe = false;
        }
    }

    void OpenCircle()
    {
        showingRange = true;
        if (rangeCircle != null) rangeCircle.SetActive(true);

        if (MergeManager.Instance != null)
            MergeManager.Instance.OnUnitClicked(gameObject, unitData);
    }

    void CloseCircle()
    {
        showingRange = false;
        if (rangeCircle != null) rangeCircle.SetActive(false);

        if (MergeManager.Instance != null)
            MergeManager.Instance.DeselectWithoutHiding();
    }

    // ─────────────────────────────────────────────────────────────────────────
    public void HideRangeCircle()
    {
        showingRange = false;
        if (rangeCircle != null) rangeCircle.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────────────
    void CreateRangeCircle()
    {
        if (unitData == null) return;

        rangeCircle = new GameObject("RangeCircle");
        rangeCircle.transform.SetParent(transform);
        rangeCircle.transform.localPosition = Vector3.zero;

        LineRenderer lr = rangeCircle.AddComponent<LineRenderer>();
        int segments = 64;
        lr.positionCount = segments + 1;
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.startWidth = 0.04f;
        lr.endWidth = 0.04f;

        Color c = GetRarityColor();
        lr.startColor = c;
        lr.endColor = c;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.material.color = c;
        lr.sortingOrder = 10;

        float radius = unitData.attackRange;
        float angleStep = 360f / segments;
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            lr.SetPosition(i, new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f));
        }

        rangeCircle.SetActive(false);
    }

    Color GetRarityColor()
    {
        if (unitData == null) return Color.white;
        switch (unitData.rarity)
        {
            case Rarity.Legendary: return new Color(1f, 0.85f, 0f);
            case Rarity.Epic: return new Color(0.6f, 0f, 1f);
            case Rarity.Rare: return new Color(0f, 0.5f, 1f);
            default: return new Color(0.6f, 0.6f, 0.6f);
        }
    }
}