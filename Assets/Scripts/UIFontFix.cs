using UnityEngine;
using TMPro;

// ─────────────────────────────────────────────────────────────────────────────
//  UIFontFix
//
//  Attach to GameManager.
//  Disables Auto Size on all UI text objects so font size stays fixed
//  during gameplay and doesn't grow or shrink as text content changes.
//
//  Run this once at Start — no ongoing performance cost.
// ─────────────────────────────────────────────────────────────────────────────
public class UIFontFix : MonoBehaviour
{
    void Start()
    {
        // Find every TextMeshProUGUI in the scene and lock its font size
        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (TextMeshProUGUI tmp in allTexts)
        {
            tmp.enableAutoSizing = false;
        }
    }
}