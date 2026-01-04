using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float moveSensitivity = 0.5f;
    [SerializeField] float smoothTime = 10f; // Higher = smoother/slower

    [SerializeField] float zoomSensitivity = 0.5f;

    [Header("Clamping Limits")]
    [SerializeField] Vector2 minBound = new Vector2(-50, -50);
    [SerializeField] Vector2 maxBound = new Vector2(50, 50);

    [SerializeField] float maxZoomIn = 5f;
    [SerializeField] float maxZoomOut = 50f;


    private Vector3 targetPosition;
    private Transform cameraTransform;

    void Awake()
    {
        cameraTransform = Camera.main.transform;
        targetPosition = cameraTransform.position;
    }

    void Update()
    {
        // Smoothly slide the camera toward the target position
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, Time.deltaTime * smoothTime);
    }

    public void MoveCamera(Vector2 moveInput)
    {
        if (cameraTransform == null) return;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // Project vectors onto the XZ plane (ignore Y height)
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();


        // 2. Calculate movement based on camera orientation
        // We invert moveInput.x/y depending on your preference for 'Drag' vs 'Fly'
        Vector3 moveDirection = (forward * -moveInput.y + right * -moveInput.x) * moveSensitivity;

        targetPosition += moveDirection;

        // 3. Apply Clamping to the target
        targetPosition.x = Mathf.Clamp(targetPosition.x, minBound.x, maxBound.x);
        targetPosition.z = Mathf.Clamp(targetPosition.z, minBound.y, maxBound.y);

        //calculate movement relative to the x and z plane
        // Vector3 deltaMovement = new Vector3(-moveInput.y, 0, moveInput.x) * moveSensitivity;
        // Vector3 newPosition = cameraTransform.position + deltaMovement;

        // apply clamping 
        // newPosition.x = Mathf.Clamp(newPosition.x, minBound.x, maxBound.x);
        // newPosition.z = Mathf.Clamp(newPosition.z, minBound.y, maxBound.y);

        // cameraTransform.position = newPosition;
    }

    public void CameraZoom(float delta)
    {
        // delta is usually the scroll wheel value or pinch value
        float zoomAmount = -delta * zoomSensitivity;

        // 1. Apply the zoom change to the TARGET Y
        targetPosition.y += zoomAmount;

        // 2. Clamp the TARGET Y so it stays within zoom bounds
        targetPosition.y = Mathf.Clamp(targetPosition.y, maxZoomIn, maxZoomOut);
    }
}
