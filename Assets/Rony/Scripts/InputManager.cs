using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{

    MainInput mainInput;
    MainInput.NavigateActions navigate;

    [SerializeField] CameraMovement cameraMovement;


    float prevDistance;

    void Awake()
    {
        mainInput = new MainInput();
        navigate = mainInput.Navigate;

        if (cameraMovement == null)
        {
            cameraMovement = FindFirstObjectByType<CameraMovement>();
        }


    }

    void LateUpdate()
    {
        HandleCameraMovement();
        HandleCameraZoom();
    }

    void HandleCameraMovement()
    {
        if (cameraMovement == null) return;
        // will read the position value of mous or touch
        Vector2 input = navigate.Delta.ReadValue<Vector2>();

        //1. check if we should allow movement
        bool isPressed = navigate.Press.IsPressed();
        bool allowMove = false;

        if (isPressed)
        {
            bool isPinching = navigate.Touch0Contact.IsPressed() && navigate.Touch1Contact.IsPressed();
            if (!isPinching) allowMove = true;
        }


        if (allowMove && input.sqrMagnitude > 0.01f)
        {
            cameraMovement.MoveCamera(input);
        }
    }


    void HandleCameraZoom()
    {
        float zoomDelta = 0f;

        //1. mouseScroll 
        float scroll = navigate.MouseScroll.ReadValue<float>();
        if (Mathf.Abs(scroll) > 0.1f)
        {
            //mouse scroll is usally very large( e.g. 120) so we scale it down
            zoomDelta = scroll;

        }

        //2. touch pinch
        //we strictly check if both specific finger are touching
        bool finger0Down = navigate.Touch0Contact.IsPressed();
        bool finger1Down = navigate.Touch1Contact.IsPressed();

        //if both finger is down
        if (finger0Down && finger1Down)
        {
            Vector2 pos0 = navigate.Touch0Position.ReadValue<Vector2>();
            Vector2 pos1 = navigate.Touch1Position.ReadValue<Vector2>();

            float currentDistance = Vector2.Distance(pos0, pos1);

            //if we just start pinching this frame , reset prev distance
            if (prevDistance == 0)
            {
                prevDistance = currentDistance;
            }
            else
            {
                // calculate the distance
                zoomDelta = currentDistance - prevDistance;
                prevDistance = currentDistance;
            }
        }
        else
        {
            //if finger are lifted , reset the distance tracker
            prevDistance = 0;

        }


        //3. apply zoom if there is input
        if (Mathf.Abs(zoomDelta) > 0.001f)
        {
            cameraMovement.CameraZoom(zoomDelta);
        }
    }

    void OnEnable()
    {
        navigate.Enable();
    }

    void OnDisable()
    {
        navigate.Disable();
    }




}
