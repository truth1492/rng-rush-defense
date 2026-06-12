using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MergeManager : MonoBehaviour
{
    public static MergeManager Instance;

    [Header("References")]
    public UnitDatabase unitDatabase;

    [Header("Merge Button Appearance")]
    public float buttonOffsetY = -0.55f;
    public float buttonWidth = 0.8f;
    public float buttonHeight = 0.28f;

    private GameObject selectedUnit;
    private UnitData selectedUnitData;
    private List<GameObject> mergeTargets = new List<GameObject>();

    private GameObject mergeButtonObj;
    private TextMeshPro mergeButtonLabel;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CreateWorldMergeButton();
    }

    void Update()
    {
        // Keep merge button following the selected unit
        if (mergeButtonObj != null && mergeButtonObj.activeSelf && selectedUnit != null)
        {
            Vector3 pos = selectedUnit.transform.position;
            pos.y += buttonOffsetY;
            pos.z = 0f;
            mergeButtonObj.transform.position = pos;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Called by InputManager when a press lands on the merge button
    // ─────────────────────────────────────────────────────────────────────────
    public void TryExecuteMerge(Vector3 worldPos)
    {
        if (!IsPositionOnMergeButton(worldPos)) return;
        ExecuteMerge();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Called by UnitCombat when a unit is tapped
    // ─────────────────────────────────────────────────────────────────────────
    public void OnUnitClicked(GameObject unit, UnitData data)
    {
        if (selectedUnit == unit)
        {
            Deselect();
            return;
        }

        // Hide previous unit's circle
        if (selectedUnit != null)
        {
            UnitCombat prevCombat = selectedUnit.GetComponent<UnitCombat>();
            if (prevCombat != null) prevCombat.HideRangeCircle();
        }

        selectedUnit = unit;
        selectedUnitData = data;

        if (data.rarity == Rarity.Legendary)
        {
            HideMergeButton();
            return;
        }

        mergeTargets = FindMatchingUnits(data);

        if (mergeTargets.Count >= 3)
        {
            mergeTargets = mergeTargets.GetRange(0, 3);
            ShowMergeButton(unit);
        }
        else
        {
            HideMergeButton();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    List<GameObject> FindMatchingUnits(UnitData data)
    {
        List<GameObject> matches = new List<GameObject>();

        if (selectedUnit != null)
            matches.Add(selectedUnit);

        GameObject[] allUnits = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject unit in allUnits)
        {
            if (unit == selectedUnit) continue;
            UnitCombat combat = unit.GetComponent<UnitCombat>();
            if (combat != null && combat.GetUnitData() == data)
                matches.Add(unit);
        }

        return matches;
    }

    // ─────────────────────────────────────────────────────────────────────────
    void ExecuteMerge()
    {
        if (mergeTargets.Count < 3 || selectedUnitData == null) return;
        if (unitDatabase == null) return;

        int spawnSlotIndex = UnitSpawner.GetSlotIndex(mergeTargets[0].transform.position);

        foreach (GameObject unit in mergeTargets)
        {
            int slotIdx = UnitSpawner.GetSlotIndex(unit.transform.position);
            if (slotIdx >= 0)
                UnitSpawner.SlotOccupants[slotIdx] = null;
            Destroy(unit);
        }

        Rarity nextRarity = (Rarity)((int)selectedUnitData.rarity + 1);
        UnitData newData = unitDatabase.GetRandomUnitOfRarity(nextRarity);

        if (newData == null || newData.prefab == null)
        {
            Debug.LogWarning("[MergeManager] No unit found for rarity: " + nextRarity);
            Deselect();
            return;
        }

        if (spawnSlotIndex >= 0)
        {
            Vector3 spawnPos = UnitSpawner.GridPositions[spawnSlotIndex];
            GameObject newUnit = Instantiate(newData.prefab, spawnPos, Quaternion.identity);
            newUnit.transform.position = spawnPos;

            UnitCombat combat = newUnit.GetComponent<UnitCombat>()
                             ?? newUnit.AddComponent<UnitCombat>();
            combat.Initialize(newData);

            UnitSpawner.SlotOccupants[spawnSlotIndex] = newUnit;
        }

        Deselect();
    }

    // ─────────────────────────────────────────────────────────────────────────
    public bool IsPositionOnMergeButton(Vector3 worldPos)
    {
        if (mergeButtonObj == null || !mergeButtonObj.activeSelf) return false;

        Vector3 btnPos = mergeButtonObj.transform.position;
        return Mathf.Abs(worldPos.x - btnPos.x) < buttonWidth * 0.5f &&
               Mathf.Abs(worldPos.y - btnPos.y) < buttonHeight * 0.5f;
    }

    // ─────────────────────────────────────────────────────────────────────────
    void CreateWorldMergeButton()
    {
        mergeButtonObj = new GameObject("MergeButton_World");

        GameObject bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
        bg.name = "MergeBG";
        bg.transform.SetParent(mergeButtonObj.transform);
        bg.transform.localPosition = Vector3.zero;
        bg.transform.localScale = new Vector3(buttonWidth, buttonHeight, 1f);
        Destroy(bg.GetComponent<Collider>());

        Renderer rend = bg.GetComponent<Renderer>();
        rend.material = new Material(Shader.Find("Sprites/Default"));
        rend.material.color = new Color(0.15f, 0.15f, 0.15f, 0.92f);
        rend.sortingOrder = 15;

        GameObject labelObj = new GameObject("MergeLabel");
        labelObj.transform.SetParent(mergeButtonObj.transform);
        labelObj.transform.localPosition = new Vector3(0f, 0f, -0.01f);

        mergeButtonLabel = labelObj.AddComponent<TextMeshPro>();
        mergeButtonLabel.text = "Merge";
        mergeButtonLabel.fontSize = 1.4f;
        mergeButtonLabel.alignment = TextAlignmentOptions.Center;
        mergeButtonLabel.color = Color.white;
        mergeButtonLabel.sortingOrder = 16;

        GameObject border = new GameObject("MergeBorder");
        border.transform.SetParent(mergeButtonObj.transform);
        border.transform.localPosition = new Vector3(0f, 0f, -0.02f);

        LineRenderer lr = border.AddComponent<LineRenderer>();
        lr.positionCount = 5;
        lr.useWorldSpace = false;
        lr.loop = false;
        lr.startWidth = 0.03f;
        lr.endWidth = 0.03f;
        lr.startColor = new Color(0.4f, 0.9f, 0.4f);
        lr.endColor = new Color(0.4f, 0.9f, 0.4f);
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.material.color = new Color(0.4f, 0.9f, 0.4f);
        lr.sortingOrder = 17;

        float hw = buttonWidth * 0.5f;
        float hh = buttonHeight * 0.5f;
        lr.SetPosition(0, new Vector3(-hw, -hh, 0f));
        lr.SetPosition(1, new Vector3(hw, -hh, 0f));
        lr.SetPosition(2, new Vector3(hw, hh, 0f));
        lr.SetPosition(3, new Vector3(-hw, hh, 0f));
        lr.SetPosition(4, new Vector3(-hw, -hh, 0f));

        mergeButtonObj.SetActive(false);
    }

    void ShowMergeButton(GameObject unit)
    {
        if (mergeButtonObj == null) return;

        Vector3 pos = unit.transform.position;
        pos.y += buttonOffsetY;
        pos.z = 0f;
        mergeButtonObj.transform.position = pos;

        if (mergeButtonLabel != null)
            mergeButtonLabel.text = "Merge";

        mergeButtonObj.SetActive(true);
    }

    void HideMergeButton()
    {
        if (mergeButtonObj != null)
            mergeButtonObj.SetActive(false);
    }

    // Called when unit closes its own circle — no need to hide it again
    public void DeselectWithoutHiding()
    {
        selectedUnit = null;
        selectedUnitData = null;
        mergeTargets.Clear();
        HideMergeButton();
    }

    public void Deselect()
    {
        if (selectedUnit != null)
        {
            UnitCombat combat = selectedUnit.GetComponent<UnitCombat>();
            if (combat != null) combat.HideRangeCircle();
        }

        selectedUnit = null;
        selectedUnitData = null;
        mergeTargets.Clear();
        HideMergeButton();
    }
}