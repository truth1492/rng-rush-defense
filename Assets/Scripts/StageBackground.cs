using UnityEngine;
using UnityEngine.UI;

// ─────────────────────────────────────────────────────────────────────────────
//  StageBackground
//  Attach to your Background Image in SampleScene.
//  Reads selected stage from PlayerPrefs and swaps background sprite.
// ─────────────────────────────────────────────────────────────────────────────
public class StageBackground : MonoBehaviour
{
    [Header("Stage Backgrounds — assign in order")]
    public Sprite[] backgrounds;
    // Index 0 = wood background        (Timber Trails)
    // Index 1 = darkwood background    (Witchwood)
    // Index 2 = desert background      (Sandstorm Desert)
    // Index 3 = ice background         (Glacial Ruins)
    // Index 4 = lava background        (Volcanic Rift)
    // Index 5 = ruined factory background (Abandoned Factory)
    // Index 6 = crystal background     (Crystal Cave)

    [Header("Endless Mode Background")]
    public Sprite endlessBackground;

    void Start()
    {
        bool isEndless = PlayerPrefs.GetInt("EndlessMode", 0) == 1;

        if (isEndless && endlessBackground != null)
        {
            Image img = GetComponent<Image>();
            if (img != null) img.sprite = endlessBackground;
        }
        else
        {
            int selectedStage = PlayerPrefs.GetInt("SelectedStage", 0);
            SetBackground(selectedStage);
        }
    }

    void SetBackground(int stageIndex)
    {
        if (backgrounds == null || backgrounds.Length == 0) return;

        // Clamp to valid range
        stageIndex = Mathf.Clamp(stageIndex, 0, backgrounds.Length - 1);

        Image img = GetComponent<Image>();
        if (img != null && backgrounds[stageIndex] != null)
            img.sprite = backgrounds[stageIndex];
    }
}