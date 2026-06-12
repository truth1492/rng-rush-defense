using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
//  PortalVortex
//  Attach to portal GameObjects to make them spin continuously.
// ─────────────────────────────────────────────────────────────────────────────
public class PortalVortex : MonoBehaviour
{
    [Header("Rotation")]
    public float rotationSpeed = 60f;  // degrees per second
    public bool clockwise = true;

    [Header("Pulse Scale")]
    public bool doPulse = true;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.08f; // how much it grows/shrinks

    private Vector3 baseScale;
    private float pulseTimer;

    void Start()
    {
        baseScale = transform.localScale;
        pulseTimer = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        // Rotate
        float direction = clockwise ? -1f : 1f;
        transform.Rotate(0f, 0f, direction * rotationSpeed * Time.deltaTime);

        // Pulse scale
        if (doPulse)
        {
            pulseTimer += Time.deltaTime * pulseSpeed;
            float pulse = 1f + Mathf.Sin(pulseTimer) * pulseAmount;
            transform.localScale = baseScale * pulse;
        }
    }
}