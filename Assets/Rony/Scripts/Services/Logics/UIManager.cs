using UnityEngine;

public class UIManager : MonoBehaviour
{

    [SerializeField] GameObject landUIPrefab;
    [SerializeField] Transform uiCanvasParent;

    private GameObject currentActiveUI;

    void OnEnable()
    {
        EventBus<LandEvent>.Subscribe(OnLandEvent);
    }

    private void OnDisable()
    {
        // UNSUBSCRIBE: Always clean up!
        EventBus<LandEvent>.Unsubscribe(OnLandEvent);

    }

    // One method handles all Land-related updates
    private void OnLandEvent(LandEvent data)
    {
        switch (data.Type)
        {
            case LandEventType.Selected:
                OpenLandUI(data.Subject);
                break;

            case LandEventType.Deselected:
                CloseLandUI();
                break;

            case LandEventType.Purchased:
                // Maybe refresh the UI if it's currently open?
                if (currentActiveUI != null)
                    currentActiveUI.GetComponent<LandUI>().Setup(data.Subject);
                break;
        }
    }

    private void OpenLandUI(Land land)
    {
        CloseLandUI(); // Cleanup old one first

        currentActiveUI = Instantiate(landUIPrefab, uiCanvasParent);
        var uiScript = currentActiveUI.GetComponent<LandUI>();
        if (uiScript != null) uiScript.Setup(land);
    }

    private void CloseLandUI()
    {
        if (currentActiveUI != null)
        {
            Destroy(currentActiveUI);
            currentActiveUI = null;
        }
    }


}