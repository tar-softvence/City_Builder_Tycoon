using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player input for camera navigation, zooming, and object selection.
/// Bridges the Unity Input System with game-specific logic (CameraMovement and SelectionManager).
/// </summary>
public class InputManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private SelectionManager selectionManager;

    // Input System variables
    private MainInput _mainInput;
    private MainInput.NavigateActions _navigate;

    // State tracking
    private float _prevDistance;

    #region Lifecycle

    private void Awake()
    {
        InitializeInput();
        ResolveDependencies();
    }

    private void OnEnable()
    {
        _navigate.Enable();
        // Subscribe to the Press action for selection logic
        _navigate.Press.performed += OnPressPerformed;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks and unexpected behavior
        _navigate.Press.performed -= OnPressPerformed;
        _navigate.Disable();
    }

    private void LateUpdate()
    {
        HandleCameraMovement();
        HandleCameraZoom();
    }

    #endregion

    #region Setup

    private void InitializeInput()
    {
        _mainInput = new MainInput();
        _navigate = _mainInput.Navigate;
    }

    private void ResolveDependencies()
    {
        if (cameraMovement == null)
            cameraMovement = FindFirstObjectByType<CameraMovement>();

        if (selectionManager == null)
            selectionManager = FindFirstObjectByType<SelectionManager>();
    }

    #endregion

    #region Input Handlers

    /// <summary>
    /// Triggered when the 'Press' action (Click/Touch) is performed.
    /// Manages object selection while ensuring pinch-to-zoom doesn't trigger a selection.
    /// </summary>
    private void OnPressPerformed(InputAction.CallbackContext context)
    {
        // Don't select if we are performing a pinch zoom (multi-touch)
        bool isPinching = _navigate.Touch0Contact.IsPressed() && _navigate.Touch1Contact.IsPressed();
        if (isPinching) return;

        // Get the current pointer position (Mouse or Touch)
        Vector2 pointerPos = _navigate.Position.ReadValue<Vector2>();

        if (selectionManager != null)
        {
            selectionManager.ProcessSelection(pointerPos);
        }
    }

    /// <summary>
    /// Reads delta movement to pan the camera. 
    /// Movement is only allowed during single-press dragging.
    /// </summary>
    private void HandleCameraMovement()
    {
        if (cameraMovement == null) return;

        // Read the delta position of mouse or touch
        Vector2 inputDelta = _navigate.Delta.ReadValue<Vector2>();

        bool isPressed = _navigate.Press.IsPressed();
        bool allowMove = false;

        if (isPressed)
        {
            // Check if we are pinching; if so, block movement to keep zoom stable
            bool isPinching = _navigate.Touch0Contact.IsPressed() && _navigate.Touch1Contact.IsPressed();
            if (!isPinching) allowMove = true;
        }

        if (allowMove && inputDelta.sqrMagnitude > 0.01f)
        {
            cameraMovement.MoveCamera(inputDelta);
        }
    }

    /// <summary>
    /// Handles zooming logic via Mouse Scroll Wheel or Touch Pinch gestures.
    /// </summary>
    private void HandleCameraZoom()
    {
        if (cameraMovement == null) return;

        float zoomDelta = 0f;

        // 1. Mouse Scroll handling
        float scroll = _navigate.MouseScroll.ReadValue<float>();
        if (Mathf.Abs(scroll) > 0.1f)
        {
            // Mouse scroll is usually large (e.g. 120), so we invert/scale it for consistency
            zoomDelta = -scroll;
        }

        // 2. Touch Pinch handling
        bool finger0Down = _navigate.Touch0Contact.IsPressed();
        bool finger1Down = _navigate.Touch1Contact.IsPressed();

        if (finger0Down && finger1Down)
        {
            Vector2 pos0 = _navigate.Touch0Position.ReadValue<Vector2>();
            Vector2 pos1 = _navigate.Touch1Position.ReadValue<Vector2>();

            float currentDistance = Vector2.Distance(pos0, pos1);

            // Initial pinch frame setup
            if (_prevDistance == 0)
            {
                _prevDistance = currentDistance;
            }
            else
            {
                // Calculate difference in distance between frames
                zoomDelta = currentDistance - _prevDistance;
                _prevDistance = currentDistance;
            }
        }
        else
        {
            // Reset distance tracker when fingers are lifted
            _prevDistance = 0;
        }

        // 3. Apply the calculated zoom delta to the camera
        if (Mathf.Abs(zoomDelta) > 0.001f)
        {
            cameraMovement.CameraZoom(zoomDelta);
        }
    }

    #endregion
}