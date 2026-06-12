using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
//  MonsterHealthBar
//
//  Attach to monster prefabs OR let EnemyHealth add it automatically.
//  Creates a health bar above the monster using LineRenderer and a quad mesh.
//
//  Bar layout:
//    [========== ] ← green/red fill bar
//    [__________] ← dark grey background
// ─────────────────────────────────────────────────────────────────────────────
public class MonsterHealthBar : MonoBehaviour
{
    [Header("Bar Settings")]
    public float barWidth = 1.4f;   // total width of bar
    public float barHeight = 0.20f;  // height of bar
    public float barOffsetY = 1.0f;   // how far above monster centre

    [Header("Colors")]
    public Color fullColor = new Color(0.2f, 0.9f, 0.2f, 1f);  // green
    public Color lowColor = new Color(0.9f, 0.2f, 0.2f, 1f);  // red
    public Color bgColor = new Color(0.1f, 0.1f, 0.1f, 0.8f); // dark bg
    public Color borderColor = new Color(0f, 0f, 0f, 1f);   // black border

    private GameObject barRoot;
    private MeshRenderer fillRenderer;
    private Transform fillTransform;
    private float maxHP;
    private float currentHP;

    // ─────────────────────────────────────────────────────────────────────────
    public void Initialize(float max, float current)
    {
        maxHP = max;
        currentHP = current;
        CreateBar();
        UpdateBar();
        // Hide until first hit
        if (barRoot != null)
            barRoot.SetActive(false);
    }

    public void Show()
    {
        if (barRoot != null)
            barRoot.SetActive(true);
    }

    public void UpdateHealth(float current)
    {
        currentHP = current;
        UpdateBar();
    }

    public void Hide()
    {
        if (barRoot != null)
            barRoot.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────────────
    void CreateBar()
    {
        barRoot = new GameObject("HealthBar");
        barRoot.transform.SetParent(transform);
        barRoot.transform.localPosition = new Vector3(0f, barOffsetY, -0.05f);
        barRoot.transform.localRotation = Quaternion.identity;
        barRoot.transform.localScale = Vector3.one;

        // Background
        CreateQuad("BG", barRoot.transform,
            new Vector3(0f, 0f, 0.01f),
            new Vector3(barWidth, barHeight, 1f),
            bgColor, 4);

        // Fill bar — starts full width, anchored left
        GameObject fillObj = CreateQuad("Fill", barRoot.transform,
            new Vector3(0f, 0f, 0f),
            new Vector3(1f, 1f, 1f),
            fullColor, 5);

        fillRenderer = fillObj.GetComponent<MeshRenderer>();
        fillTransform = fillObj.transform;

        // Border outline
        CreateBorderOutline();
    }

    // ─────────────────────────────────────────────────────────────────────────
    void UpdateBar()
    {
        if (fillTransform == null || fillRenderer == null) return;

        float ratio = Mathf.Clamp01(currentHP / maxHP);

        // Anchor fill bar to LEFT side
        // Full width = barWidth, so at ratio 1.0 fill is centred at 0
        // At ratio 0.5 fill width is barWidth*0.5, centred at -barWidth*0.25
        float fillWidth = barWidth * ratio;
        fillTransform.localScale = new Vector3(fillWidth, barHeight * 0.7f, 1f);
        fillTransform.localPosition = new Vector3(
            (-barWidth * 0.5f) + (fillWidth * 0.5f),
            0f, 0f);

        // Color shifts from green to red as HP decreases
        fillRenderer.material.color = Color.Lerp(lowColor, fullColor, ratio);
    }

    // ─────────────────────────────────────────────────────────────────────────
    GameObject CreateQuad(string name, Transform parent,
        Vector3 localPos, Vector3 localScale, Color color, int sortOrder)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.transform.localPosition = localPos;
        obj.transform.localScale = localScale;
        obj.transform.localRotation = Quaternion.identity;

        MeshFilter mf = obj.AddComponent<MeshFilter>();
        MeshRenderer mr = obj.AddComponent<MeshRenderer>();

        // Simple quad mesh
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0f),
            new Vector3( 0.5f, -0.5f, 0f),
            new Vector3( 0.5f,  0.5f, 0f),
            new Vector3(-0.5f,  0.5f, 0f)
        };
        mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
        mesh.uv = new Vector2[]
        {
            new Vector2(0, 0), new Vector2(1, 0),
            new Vector2(1, 1), new Vector2(0, 1)
        };
        mesh.RecalculateNormals();
        mf.mesh = mesh;

        mr.material = new Material(Shader.Find("Sprites/Default"));
        mr.material.color = color;
        mr.sortingOrder = sortOrder;

        return obj;
    }

    void CreateBorderOutline()
    {
        GameObject borderObj = new GameObject("Border");
        borderObj.transform.SetParent(barRoot.transform);
        borderObj.transform.localPosition = new Vector3(0f, 0f, -0.01f);
        borderObj.transform.localRotation = Quaternion.identity;
        borderObj.transform.localScale = Vector3.one;

        LineRenderer lr = borderObj.AddComponent<LineRenderer>();
        lr.positionCount = 5;
        lr.useWorldSpace = false;
        lr.loop = false;
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.startColor = borderColor;
        lr.endColor = borderColor;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.material.color = borderColor;
        lr.sortingOrder = 6;

        float hw = barWidth * 0.5f;
        float hh = barHeight * 0.5f;
        lr.SetPosition(0, new Vector3(-hw, -hh, 0f));
        lr.SetPosition(1, new Vector3(hw, -hh, 0f));
        lr.SetPosition(2, new Vector3(hw, hh, 0f));
        lr.SetPosition(3, new Vector3(-hw, hh, 0f));
        lr.SetPosition(4, new Vector3(-hw, -hh, 0f));
    }
}