using UnityEngine;
using System.Collections.Generic;

public class UnitSpawner : MonoBehaviour
{
    [Header("Summon Effect")]
    public RectTransform summonButtonTransform;
    public Canvas mainCanvas;
    [Header("Unit Database")]
    public UnitDatabase unitDatabase;

    [Header("Board Layout")]
    public float cellSnapRadius = 0.4f; // how close to snap to a slot

    public static UnitSpawner Instance;
    public static List<Vector3> GridPositions = new List<Vector3>();
    public static GameObject[] SlotOccupants;

    void Awake()
    {
        Instance = this;
        BuildGrid();
    }

    // Hardcoded 42 slot positions matching background grid layout
    // Layout: 3x(2x3) + 6x(2x2) = 42 slots named G1-G42
    void BuildGrid()
    {
        GridPositions.Clear();
        GridPositions.AddRange(new Vector3[]
        {
            // Spawn order: G21-24 → G17-20 → G25-28 → G5,6,3,4 → G11,12,9,10 → G15,16,13,14
            //            → G29-32 → G33-36 → G39-42 → G1,2 → G7,8 → G37,38
            new Vector3(-0.45f,  0.95f, 0f), // G21 (slot 1)
            new Vector3( 0.45f,  0.95f, 0f), // G22 (slot 2)
            new Vector3(-0.45f, -0.05f, 0f), // G23 (slot 3)
            new Vector3( 0.45f, -0.05f, 0f), // G24 (slot 4)
            new Vector3(-3.05f,  0.95f, 0f), // G17 (slot 5)
            new Vector3(-2.15f,  0.95f, 0f), // G18 (slot 6)
            new Vector3(-3.05f, -0.05f, 0f), // G19 (slot 7)
            new Vector3(-2.15f, -0.05f, 0f), // G20 (slot 8)
            new Vector3( 2.1f,   0.95f, 0f), // G25 (slot 9)
            new Vector3( 3.0f,   0.95f, 0f), // G26 (slot 10)
            new Vector3( 2.1f,  -0.05f, 0f), // G27 (slot 11)
            new Vector3( 3.0f,  -0.05f, 0f), // G28 (slot 12)
            new Vector3(-3.05f,  2.8f,  0f), // G5  (slot 13)
            new Vector3(-2.15f,  2.8f,  0f), // G6  (slot 14)
            new Vector3(-3.05f,  3.8f,  0f), // G3  (slot 15)
            new Vector3(-2.15f,  3.8f,  0f), // G4  (slot 16)
            new Vector3(-0.45f,  2.8f,  0f), // G11 (slot 17)
            new Vector3( 0.45f,  2.8f,  0f), // G12 (slot 18)
            new Vector3(-0.45f,  3.8f,  0f), // G9  (slot 19)
            new Vector3( 0.45f,  3.8f,  0f), // G10 (slot 20)
            new Vector3( 2.1f,   2.86f, 0f), // G15 (slot 21)
            new Vector3( 3.0f,   2.86f, 0f), // G16 (slot 22)
            new Vector3( 2.1f,   3.86f, 0f), // G13 (slot 23)
            new Vector3( 3.0f,   3.86f, 0f), // G14 (slot 24)
            new Vector3(-3.05f, -1.95f, 0f), // G29 (slot 25)
            new Vector3(-2.15f, -1.95f, 0f), // G30 (slot 26)
            new Vector3(-3.05f, -2.95f, 0f), // G31 (slot 27)
            new Vector3(-2.15f, -2.95f, 0f), // G32 (slot 28)
            new Vector3(-0.45f, -1.95f, 0f), // G33 (slot 29)
            new Vector3( 0.45f, -1.95f, 0f), // G34 (slot 30)
            new Vector3(-0.45f, -2.95f, 0f), // G35 (slot 31)
            new Vector3( 0.45f, -2.95f, 0f), // G36 (slot 32)
            new Vector3( 2.1f,  -1.95f, 0f), // G39 (slot 33)
            new Vector3( 3.0f,  -1.95f, 0f), // G40 (slot 34)
            new Vector3( 2.1f,  -2.95f, 0f), // G41 (slot 35)
            new Vector3( 3.0f,  -2.95f, 0f), // G42 (slot 36)
            new Vector3(-3.05f,  4.8f,  0f), // G1  (slot 37)
            new Vector3(-2.15f,  4.8f,  0f), // G2  (slot 38)
            new Vector3(-0.45f,  4.8f,  0f), // G7  (slot 39)
            new Vector3( 0.45f,  4.8f,  0f), // G8  (slot 40)
            new Vector3(-0.45f, -3.95f, 0f), // G37 (slot 41)
            new Vector3( 0.45f, -3.95f, 0f), // G38 (slot 42)
        });

        SlotOccupants = new GameObject[GridPositions.Count];
    }

    // Called by UnitDragHandler — finds which slot a world position belongs to
    public static int GetSlotIndex(Vector3 worldPos)
    {
        int closestIndex = -1;
        float closestDist = Mathf.Infinity;

        for (int i = 0; i < GridPositions.Count; i++)
        {
            float d = Vector3.Distance(worldPos, GridPositions[i]);
            if (d < closestDist)
            {
                closestDist = d;
                closestIndex = i;
            }
        }

        if (Instance == null) return -1;
        return (closestDist < (Instance?.cellSnapRadius ?? 0.4f)) ? closestIndex : -1;
    }

    // Returns index of lowest-numbered empty slot, or -1 if full
    public static int GetNextEmptySlot()
    {
        if (SlotOccupants == null) return -1;
        for (int i = 0; i < SlotOccupants.Length; i++)
            if (SlotOccupants[i] == null) return i;
        return -1;
    }

    Vector3 GetButtonWorldPos()
    {
        if (summonButtonTransform == null) return Vector3.zero;
        Camera cam = mainCanvas?.worldCamera ?? Camera.main;
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, summonButtonTransform.position);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, Mathf.Abs(Camera.main.transform.position.z)));
        worldPos.z = 0f;
        return worldPos;
    }

    public bool CanSpawnUnit()
    {
        return GetNextEmptySlot() >= 0;
    }

    public void SpawnRandomUnit()
    {
        if (unitDatabase == null)
        {
            Debug.LogError("[UnitSpawner] No UnitDatabase assigned in Inspector!");
            return;
        }

        int slotIndex = GetNextEmptySlot();
        if (slotIndex < 0) return;

        UnitData data = unitDatabase.GetRandomUnit();
        if (data == null || data.prefab == null)
        {
            Debug.LogWarning("[UnitSpawner] UnitData or prefab is missing! Check UnitDatabase.");
            return;
        }

        Vector3 exactPos = GridPositions[slotIndex];
        GameObject unit = Instantiate(data.prefab, exactPos, Quaternion.identity);
        unit.transform.position = exactPos;

        // Give this unit its stats
        UnitCombat combat = unit.GetComponent<UnitCombat>()
                         ?? unit.AddComponent<UnitCombat>();
        combat.Initialize(data);

        SlotOccupants[slotIndex] = unit;

        // Trigger beam effect
        if (SummonBeamEffect.Instance != null && summonButtonTransform != null && mainCanvas != null)
        {
            Vector3 buttonWorldPos = GetButtonWorldPos();
            SummonBeamEffect.Instance.PlayBeam(buttonWorldPos, exactPos, data.rarity);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (GridPositions == null || GridPositions.Count == 0) return;

        for (int i = 0; i < GridPositions.Count; i++)
        {
            UnityEditor.Handles.color = new Color(0.4f, 0.8f, 1f, 0.9f);
            UnityEditor.Handles.DrawWireDisc(GridPositions[i], Vector3.forward, 0.18f);
            UnityEditor.Handles.Label(
                GridPositions[i] + Vector3.up * 0.22f,
                $"G{i + 1}");
        }
    }
#endif
}