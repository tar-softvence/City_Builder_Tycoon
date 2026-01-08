using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
/// <summary>
/// Handles the identification and state management of selectable world objects.
/// Acts as the bridge between the <see cref="InputManager"/> pointer data and the <see cref="ISelectable"/> interface.
/// </summary>
public class SelectionManager : MonoBehaviour
{
    private ISelectable _currentSelection;

    /// <summary>
    /// Performs a Raycast from the screen position to find objects implementing <see cref="ISelectable"/>.
    /// Triggered by <see cref="InputManager.OnPressPerformed"/>.
    /// </summary>
    /// <param name="screenPosition">The screen coordinates (mouse or touch) to cast from.</param>
    public void ProcessSelection(Vector2 screenPosition)
    {

        if (IsPointerOverUI(screenPosition)) return;

        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            ISelectable clickedObject = hit.collider.GetComponentInParent<ISelectable>();

            if (clickedObject != null)
            {
                // Toggle Logic: Deselect if clicking the same object
                if (_currentSelection == clickedObject)
                {
                    DeselectCurrent();
                    return;
                }

                // New Selection Logic
                DeselectCurrent();
                _currentSelection = clickedObject;
                _currentSelection.OnSelect();
            }
            // Optional: Add 'else { DeselectCurrent(); }' here if you WANT 
            // clicking a non-selectable tree/rock to deselect the land.
        }
        // Note: We removed the 'else' block for "hitting nothing" 
        // so clicking empty sky/void does not deselect.
    }

    /// <summary>
    /// Clears the current selection and notifies the object.
    /// </summary>
    public void DeselectCurrent()
    {
        if (_currentSelection != null)
        {
            _currentSelection.OnDeSelect();
            _currentSelection = null;
        }
    }

    /// <summary>
    /// Checks if the given screen position is on top of any UI element.
    /// Works for Mouse, Touch, and New Input System.
    /// </summary>
    private bool IsPointerOverUI(Vector2 screenPos)
    {
        // 1. Create a pointer event at the screen position
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPos;

        // 2. Raycast into the UI Canvas
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // 3. If we hit any UI element, return true
        return results.Count > 0;
    }
}