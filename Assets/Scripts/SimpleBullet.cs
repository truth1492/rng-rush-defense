using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
//  SimpleBullet
//  Attach to projectile prefabs (arrows, bolts etc.)
//  Moves toward target and deals damage on hit.
// ─────────────────────────────────────────────────────────────────────────────
public class SimpleBullet : MonoBehaviour
{
    public float speed = 10f;
    public float hitRadius = 0.3f;

    private Transform target;
    private Vector3 targetOffset = Vector3.zero;
    private float damage;

    public void SetTarget(Transform newTarget, float dmg)
    {
        target = newTarget;
        targetOffset = Vector3.zero;
        damage = dmg;
    }

    public void SetTargetPosition(Transform newTarget, Vector3 aimPos, float dmg)
    {
        target = newTarget;
        targetOffset = aimPos - newTarget.position;
        damage = dmg;
    }

    void Update()
    {
        if (target == null) { Destroy(gameObject); return; }

        Vector3 flatPos = new Vector3(transform.position.x, transform.position.y, 0f);
        Vector3 flatTarget = new Vector3(target.position.x + targetOffset.x,
                                         target.position.y + targetOffset.y, 0f);

        // Rotate arrow to face direction of travel
        Vector3 dir = (flatTarget - flatPos).normalized;
        if (dir != Vector3.zero)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        transform.position = Vector3.MoveTowards(flatPos, flatTarget, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, flatTarget) <= hitRadius)
        {
            EnemyHealth hp = target.GetComponent<EnemyHealth>();
            if (hp != null) hp.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}