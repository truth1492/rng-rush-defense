using UnityEngine;

public class RarityFootCircle : MonoBehaviour
{
    private GameObject circleObj;
    private LineRenderer lr;
    private Rarity rarity;

    private bool isLegendary = false;
    private float pulseTimer = 0f;
    private float pulseSpeed = 2.5f;
    private float pulseMinAlpha = 0.3f;
    private float pulseMaxAlpha = 0.85f;
    private float pulseMinScale = 0.9f;
    private float pulseMaxScale = 1.1f;

    [Header("Position and Size")]
    public float offsetY = -0.1f;  // negative = down toward feet
    public float radius = 0.2f;   // circle size

    public void Initialize(Rarity unitRarity)
    {
        rarity = unitRarity;
        CreateCircle();
    }

    void Update()
    {
        if (!isLegendary || lr == null) return;

        pulseTimer += Time.deltaTime * pulseSpeed;
        float t = (Mathf.Sin(pulseTimer) + 1f) * 0.5f;

        Color c = GetRarityColor(rarity);
        c.a = Mathf.Lerp(pulseMinAlpha, pulseMaxAlpha, t);
        lr.startColor = c;
        lr.endColor = c;
        lr.material.color = c;

        float scale = Mathf.Lerp(pulseMinScale, pulseMaxScale, t);
        circleObj.transform.localScale = new Vector3(scale, scale, 1f);
    }

    void CreateCircle()
    {
        circleObj = new GameObject("FootCircle");
        circleObj.transform.SetParent(transform);
        circleObj.transform.localPosition = new Vector3(0f, offsetY, 0.1f);
        circleObj.transform.localScale = Vector3.one;

        // ── Filled disc ──────────────────────────────────────────────────────
        GameObject fillObj = new GameObject("FootCircleFill");
        fillObj.transform.SetParent(circleObj.transform);
        fillObj.transform.localPosition = Vector3.zero;
        fillObj.transform.localScale = Vector3.one;

        MeshFilter mf = fillObj.AddComponent<MeshFilter>();
        MeshRenderer mr = fillObj.AddComponent<MeshRenderer>();

        mr.material = new Material(Shader.Find("Sprites/Default"));
        Color fillColor = GetRarityColor(rarity);
        fillColor.a = 0.2f;
        mr.material.color = fillColor;
        mr.sortingOrder = 2;

        int segments = 48;
        Mesh mesh = new Mesh();
        Vector3[] verts = new Vector3[segments + 1];
        int[] tris = new int[segments * 3];

        verts[0] = Vector3.zero;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * (360f / segments) * Mathf.Deg2Rad;
            verts[i + 1] = new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius * 0.3f,
                0f);
        }
        for (int i = 0; i < segments; i++)
        {
            tris[i * 3] = 0;
            tris[i * 3 + 1] = i + 1;
            tris[i * 3 + 2] = (i + 1) % segments + 1;
        }
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        mf.mesh = mesh;

        // ── Outline ring ─────────────────────────────────────────────────────
        lr = circleObj.AddComponent<LineRenderer>();
        lr.positionCount = segments + 1;
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.sortingOrder = 3;

        Color outlineColor = GetRarityColor(rarity);
        outlineColor.a = 0.5f;
        lr.startColor = outlineColor;
        lr.endColor = outlineColor;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.material.color = outlineColor;

        float angleStep = 360f / segments;
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            lr.SetPosition(i, new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius * 0.3f,
                0f));
        }

        isLegendary = rarity == Rarity.Legendary;
    }

    Color GetRarityColor(Rarity r)
    {
        switch (r)
        {
            case Rarity.Legendary: return new Color(1f, 0.85f, 0f, 0.7f);
            case Rarity.Epic: return new Color(0.6f, 0f, 1f, 0.7f);
            case Rarity.Rare: return new Color(0.3f, 0.7f, 1f, 0.7f);
            default: return new Color(1f, 1f, 1f, 0.5f);
        }
    }
}