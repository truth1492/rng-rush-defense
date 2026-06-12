using UnityEngine;

public class UnitDragHandler : MonoBehaviour
{
    private bool isDragging = false;
    public bool IsDragging => isDragging;
    private bool isPressedOnMe = false;  // press happened on this unit
    private int originalSlotIndex = -1;

    private const float PickupRadius = 0.45f;
    private const float DragThreshold = 0.15f; // must move this far before drag starts

    void Start()
    {
        SnapToNearestSlot();
    }

    void SnapToNearestSlot()
    {
        if (UnitSpawner.GridPositions == null || UnitSpawner.GridPositions.Count == 0) return;
        if (UnitSpawner.SlotOccupants == null) return;

        int nearestIndex = -1;
        float nearestDist = Mathf.Infinity;

        for (int i = 0; i < UnitSpawner.GridPositions.Count; i++)
        {
            float d = Vector3.Distance(transform.position, UnitSpawner.GridPositions[i]);
            if (d < nearestDist) { nearestDist = d; nearestIndex = i; }
        }

        if (nearestIndex < 0) return;

        transform.position = UnitSpawner.GridPositions[nearestIndex];
        UnitSpawner.SlotOccupants[nearestIndex] = gameObject;
        originalSlotIndex = nearestIndex;
    }

    void Update()
    {
        if (InputManager.Instance == null) return;

        // Disable all interaction when game is over
        if (GameOverScreen.IsGameOver) return;

        // Ignore if UI consumed this click
        if (InputManager.Instance.ConsumedByUI)
        {
            isPressedOnMe = false;
            isDragging = false;
            return;
        }

        Vector3 wp = InputManager.Instance.WorldPosition;

        // ── 1. Press — check if it landed on this unit ────────────────────────
        if (InputManager.Instance.PressedThisFrame)
        {
            if (Vector3.Distance(transform.position, wp) < PickupRadius)
            {
                isPressedOnMe = true;
                originalSlotIndex = FindMySlotIndex();
            }
            else
            {
                isPressedOnMe = false;
            }
        }

        // ── 2. Hold — only start dragging once pointer moves enough ───────────
        if (isPressedOnMe && !isDragging && InputManager.Instance.IsHeld)
        {
            float movedDist = Vector3.Distance(
                UnitSpawner.GridPositions[originalSlotIndex >= 0 ? originalSlotIndex : 0],
                wp);

            if (movedDist > DragThreshold && originalSlotIndex >= 0)
            {
                // Threshold crossed — start actual drag now
                isDragging = true;
                UnitSpawner.SlotOccupants[originalSlotIndex] = null;

                // Close any open range circle
                if (MergeManager.Instance != null)
                    MergeManager.Instance.Deselect();
            }
        }

        // ── 3. While dragging — follow pointer ────────────────────────────────
        if (isDragging && InputManager.Instance.IsHeld)
            transform.position = wp;

        // ── 4. Release ────────────────────────────────────────────────────────
        if (InputManager.Instance.ReleasedThisFrame)
        {
            isPressedOnMe = false;

            if (isDragging)
            {
                isDragging = false;
                ResolveDrop();
            }
        }
    }

    int FindMySlotIndex()
    {
        int nearestIndex = -1;
        float nearestDist = Mathf.Infinity;

        for (int i = 0; i < UnitSpawner.GridPositions.Count; i++)
        {
            float d = Vector3.Distance(transform.position, UnitSpawner.GridPositions[i]);
            if (d < nearestDist) { nearestDist = d; nearestIndex = i; }
        }

        if (UnitSpawner.Instance == null) return -1;
        return (nearestDist < (UnitSpawner.Instance?.cellSnapRadius ?? 0.4f)) ? nearestIndex : -1;
    }

    void ResolveDrop()
    {
        int nearestIndex = -1;
        float nearestDist = Mathf.Infinity;

        for (int i = 0; i < UnitSpawner.GridPositions.Count; i++)
        {
            float d = Vector3.Distance(transform.position, UnitSpawner.GridPositions[i]);
            if (d < nearestDist) { nearestDist = d; nearestIndex = i; }
        }

        if (nearestIndex < 0 || nearestDist > GetSnapThreshold())
        {
            PlaceInSlot(originalSlotIndex);
            return;
        }

        GameObject occupant = UnitSpawner.SlotOccupants[nearestIndex];

        if (occupant != null)
        {
            UnitDragHandler occupantDrag = occupant.GetComponent<UnitDragHandler>();
            if (occupantDrag != null)
                occupantDrag.ForceSnapToSlot(originalSlotIndex);
            else
                PlaceUnitInSlot(occupant, originalSlotIndex);

            PlaceInSlot(nearestIndex);
        }
        else
        {
            PlaceInSlot(nearestIndex);
        }
    }

    public void ForceSnapToSlot(int slotIndex)
    {
        PlaceInSlot(slotIndex);
    }

    void PlaceInSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= UnitSpawner.GridPositions.Count)
        {
            if (originalSlotIndex >= 0 && originalSlotIndex < UnitSpawner.GridPositions.Count)
            {
                transform.position = UnitSpawner.GridPositions[originalSlotIndex];
                UnitSpawner.SlotOccupants[originalSlotIndex] = gameObject;
            }
            return;
        }

        transform.position = UnitSpawner.GridPositions[slotIndex];
        UnitSpawner.SlotOccupants[slotIndex] = gameObject;
    }

    void PlaceUnitInSlot(GameObject unit, int slotIndex)
    {
        unit.transform.position = UnitSpawner.GridPositions[slotIndex];
        UnitSpawner.SlotOccupants[slotIndex] = unit;
    }

    float GetSnapThreshold()
    {
        if (UnitSpawner.GridPositions.Count >= 2)
            return Vector3.Distance(
                UnitSpawner.GridPositions[0],
                UnitSpawner.GridPositions[1]) * 0.6f;
        return 0.5f;
    }
}