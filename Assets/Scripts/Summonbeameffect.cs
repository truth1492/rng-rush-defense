using UnityEngine;
using System.Collections;

// ─────────────────────────────────────────────────────────────────────────────
//  SummonBeamEffect
//  Creates a laser beam from the summon button to the grid spawn slot.
//  Color based on unit rarity.
//  Attach to GameManager or UnitSpawner.
// ─────────────────────────────────────────────────────────────────────────────
public class SummonBeamEffect : MonoBehaviour
{
    public static SummonBeamEffect Instance;

    [Header("Beam Settings")]
    public float beamDuration = 0.3f;   // total beam lifetime
    public float beamWidth = 0.08f;  // thickness of beam
    public float flashDuration = 0.05f;  // initial flash width multiplier

    [Header("Impact Settings")]
    public float impactRadius = 0.3f;   // size of impact circle at slot
    public float impactDuration = 0.2f;   // how long impact stays

    void Awake()
    {
        Instance = this;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Call this when a unit is summoned
    //  startWorldPos = summon button world position
    //  endWorldPos   = grid slot world position
    //  rarity        = unit rarity for color
    // ─────────────────────────────────────────────────────────────────────────
    public void PlayBeam(Vector3 startWorldPos, Vector3 endWorldPos, Rarity rarity)
    {
        StartCoroutine(BeamCoroutine(startWorldPos, endWorldPos, rarity));
    }

    IEnumerator BeamCoroutine(Vector3 start, Vector3 end, Rarity rarity)
    {
        Color beamColor = GetRarityColor(rarity);

        // ── Create beam LineRenderer ─────────────────────────────────────────
        GameObject beamObj = new GameObject("SummonBeam");
        LineRenderer lr = beamObj.AddComponent<LineRenderer>();

        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.SetPosition(0, new Vector3(start.x, start.y, 0f));
        lr.SetPosition(1, new Vector3(end.x, end.y, 0f));
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.material.color = beamColor;
        lr.startColor = beamColor;
        lr.endColor = beamColor;
        lr.sortingOrder = 50;

        // ── Animate beam — flash wide then narrow then fade ──────────────────
        float timer = 0f;
        while (timer < beamDuration)
        {
            timer += Time.deltaTime;
            float t = timer / beamDuration;

            // Width: starts wide, narrows, then fades
            float width = beamWidth * (1f - t);
            lr.startWidth = width * 2f; // wider at button end
            lr.endWidth = width;

            // Alpha fade out
            Color c = beamColor;
            c.a = Mathf.Lerp(1f, 0f, t);
            lr.startColor = c;
            lr.endColor = c;
            lr.material.color = c;

            yield return null;
        }

        Destroy(beamObj);

        // ── Impact circle at grid slot ───────────────────────────────────────
        StartCoroutine(ImpactCircle(end, beamColor));
    }

    IEnumerator ImpactCircle(Vector3 pos, Color color)
    {
        GameObject impactObj = new GameObject("SummonImpact");
        impactObj.transform.position = new Vector3(pos.x, pos.y, 0f);

        LineRenderer lr = impactObj.AddComponent<LineRenderer>();
        int segments = 32;
        lr.positionCount = segments + 1;
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.sortingOrder = 50;

        float timer = 0f;
        while (timer < impactDuration)
        {
            timer += Time.deltaTime;
            float t = timer / impactDuration;
            float radius = Mathf.Lerp(0f, impactRadius, t);

            // Draw expanding circle
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * (360f / segments) * Mathf.Deg2Rad;
                lr.SetPosition(i, new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    0f));
            }

            // Fade out
            Color c = color;
            c.a = Mathf.Lerp(1f, 0f, t);
            lr.startColor = c;
            lr.endColor = c;
            lr.material.color = c;

            yield return null;
        }

        Destroy(impactObj);
    }

    Color GetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Legendary: return new Color(1f, 0.85f, 0f, 1f); // gold
            case Rarity.Epic: return new Color(0.6f, 0f, 1f, 1f); // purple
            case Rarity.Rare: return new Color(0.3f, 0.7f, 1f, 1f); // blue
            default: return new Color(0.85f, 0.85f, 0.85f, 1f); // white grey
        }
    }
}