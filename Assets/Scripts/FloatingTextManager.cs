using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

// ─────────────────────────────────────────────────────────────────────────────
//  FloatingTextManager
//
//  Attach to GameManager.
//  Handles spawning:
//    1. Damage numbers above monsters
//    2. Gold reward floating near gold UI
// ─────────────────────────────────────────────────────────────────────────────
public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance;

    [Header("Damage Numbers")]
    public float damageRiseSpeed = 1.5f;
    public float damageDuration = 0.8f;
    public float damageFontSize = 3f;

    [Header("Gold Reward UI")]
    [Tooltip("Assign your GoldDisplay or GoldsText RectTransform here")]
    public RectTransform goldUITransform;
    public Sprite goldIconSprite;   // assign your coin sprite
    public Canvas mainCanvas;

    [Header("Gold Float Settings")]
    public float goldRiseSpeed = 1.2f;
    public float goldDuration = 1.2f;
    public float goldFontSize = 2.5f;

    void Awake()
    {
        Instance = this;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Spawn damage number above a monster
    // ─────────────────────────────────────────────────────────────────────────
    public void ShowDamage(Vector3 worldPos, float damage)
    {
        GameObject obj = new GameObject("DamageNumber");
        obj.transform.position = worldPos + new Vector3(
            Random.Range(-0.1f, 0.1f), 0.3f, 0f);

        FloatingText ft = obj.AddComponent<FloatingText>();

        // All damage numbers white
        Color dmgColor = Color.white;

        string dmgText = damage >= 1 ? ((int)damage).ToString() : damage.ToString("F1");
        ft.Initialize(dmgText, dmgColor, damageFontSize, damageRiseSpeed, damageDuration);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Show gold reward floating near gold UI
    // ─────────────────────────────────────────────────────────────────────────
    public void ShowGoldReward(int amount)
    {
        if (goldUITransform == null) return;

        Vector3 worldPos = GetGoldUIWorldPos();

        // Root object
        GameObject root = new GameObject("GoldReward");
        root.transform.position = worldPos;

        // Gold icon sprite
        if (goldIconSprite != null)
        {
            GameObject iconObj = new GameObject("GoldIcon");
            iconObj.transform.SetParent(root.transform);
            iconObj.transform.localPosition = new Vector3(-0.35f, 0f, 0f);
            iconObj.transform.localScale = new Vector3(0.15f, 0.15f, 1f);

            SpriteRenderer sr = iconObj.AddComponent<SpriteRenderer>();
            sr.sprite = goldIconSprite;
            sr.sortingOrder = 20;
        }

        // Gold text
        GameObject textObj = new GameObject("GoldText");
        textObj.transform.SetParent(root.transform);
        textObj.transform.localPosition = new Vector3(0.1f, 0f, 0f);

        FloatingText ft = textObj.AddComponent<FloatingText>();
        ft.Initialize($"+{amount}", new Color(1f, 0.85f, 0f), goldFontSize,
                      goldRiseSpeed, goldDuration, childMode: true);

        // Animate the root object
        root.AddComponent<FloatingRoot>().Initialize(goldRiseSpeed, goldDuration);
    }

    Vector3 GetGoldUIWorldPos()
    {
        if (goldUITransform == null || mainCanvas == null)
            return Vector3.zero;

        // Get screen position of gold UI element
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
            mainCanvas.worldCamera, goldUITransform.position);

        // Convert screen position to world position
        Vector3 worldPos = mainCanvas.worldCamera.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y,
                Mathf.Abs(mainCanvas.worldCamera.transform.position.z)));
        worldPos.z = 0f;

        // Offset below the gold UI
        worldPos.y -= 1.2f;
        return worldPos;
    }
}