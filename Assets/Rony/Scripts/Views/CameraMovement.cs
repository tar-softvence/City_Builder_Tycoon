using UnityEngine;

/// <summary>
/// Manages camera translation and zooming logic. 
/// Receives processed input vectors from the <see cref="InputManager"/> and applies smooth movement.
/// </summary>
public class CameraMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSensitivity = 0.5f;
    [SerializeField] private float smoothTime = 10f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSensitivity = 2f;
    [SerializeField] private float minHeight = 5f;
    [SerializeField] private float maxHeight = 50f;

    [Header("Map Boundaries")]
    [SerializeField] private Vector2 minBound = new Vector2(-50, -50);
    [SerializeField] private Vector2 maxBound = new Vector2(50, 50);

    private Vector3 _targetPosition;
    private Transform _cameraTransform;

    private void Awake()
    {
        _cameraTransform = Camera.main.transform;
        _targetPosition = _cameraTransform.position;
    }

    private void Update()
    {
        _cameraTransform.position = Vector3.Lerp(_cameraTransform.position, _targetPosition, Time.deltaTime * smoothTime);
    }

    /// <summary>
    /// Translates the camera target based on directional input.
    /// Values are typically passed from <see cref="InputManager.HandleCameraMovement"/>.
    /// </summary>
    /// <param name="moveInput">The X/Y delta from touch or mouse.</param>
    public void MoveCamera(Vector2 moveInput)
    {
        if (_cameraTransform == null) return;

        Vector3 forward = _cameraTransform.forward;
        Vector3 right = _cameraTransform.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * -moveInput.y + right * -moveInput.x) * moveSensitivity;
        _targetPosition += moveDirection;

        ClampTargetPosition();
    }

    /// <summary>
    /// Adjusts the camera height (zoom) based on a delta value.
    /// Values are typically passed from <see cref="InputManager.HandleCameraZoom"/>.
    /// </summary>
    /// <param name="delta">The scroll or pinch delta.</param>
    public void CameraZoom(float delta)
    {
        float zoomAmount = -delta * zoomSensitivity;
        Vector3 zoomMovement = _cameraTransform.forward * zoomAmount;

        _targetPosition += zoomMovement;
        ClampTargetPosition();
    }

    private void ClampTargetPosition()
    {
        _targetPosition.x = Mathf.Clamp(_targetPosition.x, minBound.x, maxBound.x);
        _targetPosition.z = Mathf.Clamp(_targetPosition.z, minBound.y, maxBound.y);
        _targetPosition.y = Mathf.Clamp(_targetPosition.y, minHeight, maxHeight);
    }
}