using UnityEngine;
using UnityEngine.InputSystem;

// ─────────────────────────────────────────────────────────────────────────────
//  InputManager  —  Single source of truth for click/tap input
//
//  Attach to GameManager.
//  All other scripts read from this instead of polling Input directly.
//
//  Each frame:
//  - Detects press and release
//  - Converts screen pos to world pos once
//  - Sets WasConsumedByUI = true if the click hit the merge button
//  - Other scripts check WasConsumedByUI before acting on the click
// ─────────────────────────────────────────────────────────────────────────────
public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    // Read these from UnitCombat and UnitDragHandler
    public bool PressedThisFrame { get; private set; }
    public bool ReleasedThisFrame { get; private set; }
    public bool IsHeld { get; private set; }
    public Vector3 WorldPosition { get; private set; }

    // True if this frame's click was already handled by UI (merge button etc.)
    // When true, units must NOT react to the click
    public bool ConsumedByUI { get; private set; }

    private Camera mainCamera;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Reset every frame
        PressedThisFrame = false;
        ReleasedThisFrame = false;
        IsHeld = false;
        ConsumedByUI = false;

        Vector2 screenPos = Vector2.zero;
        bool gotPos = false;

        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                PressedThisFrame = true;
                screenPos = Mouse.current.position.ReadValue();
                gotPos = true;
            }
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                ReleasedThisFrame = true;
                screenPos = Mouse.current.position.ReadValue();
                gotPos = true;
            }
            if (Mouse.current.leftButton.isPressed)
            {
                IsHeld = true;
                screenPos = Mouse.current.position.ReadValue();
                gotPos = true;
            }
        }

        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            if (touch.press.wasPressedThisFrame)
            {
                PressedThisFrame = true;
                screenPos = touch.position.ReadValue();
                gotPos = true;
            }
            if (touch.press.wasReleasedThisFrame)
            {
                ReleasedThisFrame = true;
                screenPos = touch.position.ReadValue();
                gotPos = true;
            }
            if (touch.press.isPressed)
            {
                IsHeld = true;
                screenPos = touch.position.ReadValue();
                gotPos = true;
            }
        }

        // Convert to world position once
        if (gotPos && mainCamera != null)
        {
            Vector3 wp = mainCamera.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, -mainCamera.transform.position.z));
            wp.z = 0f;
            WorldPosition = wp;
        }

        // Check if this click should be consumed by the merge button
        // MergeManager.Update() runs after this because of script order,
        // so we check here first and set the flag
        if ((PressedThisFrame || ReleasedThisFrame) &&
            MergeManager.Instance != null &&
            MergeManager.Instance.IsPositionOnMergeButton(WorldPosition))
        {
            ConsumedByUI = true;

            // Directly tell MergeManager to execute if it was a press
            if (PressedThisFrame)
                MergeManager.Instance.TryExecuteMerge(WorldPosition);
        }
    }
}