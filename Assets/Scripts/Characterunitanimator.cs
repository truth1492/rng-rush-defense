using UnityEngine;
using LayerLab.ArtMakerUnity;

// ─────────────────────────────────────────────────────────────────────────────
//  CharacterUnitAnimator
//  Bridges UnitCombat to the 2D Minimal Character Maker PartsManager.
//  Attach to character prefabs alongside UnitCombat.
// ─────────────────────────────────────────────────────────────────────────────
public class CharacterUnitAnimator : MonoBehaviour
{
    [Header("Animation Names")]
    public string idleAnim = "Idle";
    public string attackAnim = "Attack";

    [Header("Animation Speed")]
    public float attackAnimSpeed = 1.5f;
    public float idleAnimSpeed = 1.0f;

    [Header("Facing")]
    [Tooltip("Default facing direction of the sprite. True = faces right by default")]
    public bool defaultFacingRight = true;

    private PartsManager partsManager;
    private AnimationEventReceiver animEventReceiver;
    private Animator animator;
    private bool isAttacking = false;

    // Callback — UnitCombat subscribes to this to deal damage at right moment
    public System.Action OnAttackHit;

    void Start()
    {
        partsManager = GetComponent<PartsManager>();
        animEventReceiver = GetComponent<AnimationEventReceiver>();
        animator = GetComponent<Animator>();

        // NOTE: Do NOT call partsManager.Init() here
        // CharacterPrefabData.Start() handles Init() and restores saved appearance

        // Wire attack hit event from animator
        if (animEventReceiver != null)
            animEventReceiver.OnAttackHitEvent += HandleAttackHit;

        // Delay idle to let CharacterPrefabData.Start() restore appearance first
        Invoke(nameof(PlayIdle), 0.1f);
        SetDefaultFacing();
    }

    void OnDestroy()
    {
        if (animEventReceiver != null)
            animEventReceiver.OnAttackHitEvent -= HandleAttackHit;
    }

    // ─────────────────────────────────────────────────────────────────────────
    public void PlayIdle()
    {
        isAttacking = false;
        if (animator != null) animator.speed = idleAnimSpeed;
        if (partsManager != null)
            partsManager.PlayAnimation(idleAnim);
    }

    public void PlayAttack()
    {
        if (isAttacking) return;
        isAttacking = true;

        if (animator != null) animator.speed = attackAnimSpeed;
        if (partsManager != null)
            partsManager.PlayAnimation(attackAnim);

        // If no AnimationEventReceiver, deal damage immediately
        if (animEventReceiver == null)
        {
            HandleAttackHit();
            Invoke(nameof(PlayIdle), 0.3f);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  Call this to face toward a target
    // ─────────────────────────────────────────────────────────────────────
    public void FaceTarget(Transform target)
    {
        if (target == null) return;

        bool enemyIsOnRight = target.position.x > transform.position.x;

        // If default faces right:  right = scale +1, left = scale -1
        // If default faces left:   left  = scale +1, right = scale -1
        float scaleX = defaultFacingRight
            ? (enemyIsOnRight ? 1f : -1f)
            : (enemyIsOnRight ? -1f : 1f);

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * scaleX;
        transform.localScale = scale;
    }

    // Set default facing on start
    void SetDefaultFacing()
    {
        Vector3 scale = transform.localScale;
        // Default facing left means scale.x should be negative if sprite faces right
        scale.x = defaultFacingRight ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    void HandleAttackHit()
    {
        // Notify UnitCombat to deal damage
        OnAttackHit?.Invoke();

        // Return to idle after attack
        Invoke(nameof(PlayIdle), 0.05f);
    }
}