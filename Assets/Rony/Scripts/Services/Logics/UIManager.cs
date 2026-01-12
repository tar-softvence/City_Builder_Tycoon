using UnityEngine;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject landUIPrefab;
    [SerializeField] Transform uiCanvasParent;

    [Header("Animation Settings")]
    public float animationTime = 0.5f;
    public Ease animationEase = Ease.OutExpo;

    private GameObject currentActiveUI;
    private bool panelOpened = false;

    void OnEnable() => EventBus<LandEvent>.Subscribe(OnLandEvent);
    void OnDisable() => EventBus<LandEvent>.Unsubscribe(OnLandEvent);

    private void OnLandEvent(LandEvent data)
    {
        switch (data.Type)
        {
            case LandEventType.Selected: OpenLandUI(data.Subject); break;
            case LandEventType.Deselected: CloseLandUI(); break;
            case LandEventType.Purchased:
                if (currentActiveUI != null)
                    currentActiveUI.GetComponent<LandUI>().Setup(data.Subject);
                break;
        }
    }

    // --- Generic Animation Methods ---

    /// <summary>
    /// Toggles a panel's position between a hidden state and center.
    /// </summary>
    public void TogglePanelSlide(RectTransform rect)
    {
        if (panelOpened)
            MoveTo(rect, new Vector2(0f, -2000f));
        else
            MoveTo(rect, Vector2.zero);

        panelOpened = !panelOpened;
    }

    /// <summary>
    /// Toggles a panel's visibility using alpha.
    /// </summary>
    public void TogglePanelFade(CanvasGroup canvasGroup)
    {
        float targetAlpha = panelOpened ? 0f : 1f;
        FadeTo(canvasGroup, targetAlpha);

        panelOpened = !panelOpened;
    }

    public void MoveTo(RectTransform rect, Vector2 targetPosition)
    {
        rect.DOAnchorPos(targetPosition, animationTime).SetEase(animationEase);
    }

    public void FadeTo(CanvasGroup canvasGroup, float targetAlpha)
    {
        canvasGroup.DOFade(targetAlpha, animationTime).SetEase(animationEase);
    }

    // --- UI Logic ---

    private void OpenLandUI(Land land)
    {
        CloseLandUI();
        currentActiveUI = Instantiate(landUIPrefab, uiCanvasParent);
        if (currentActiveUI.TryGetComponent<LandUI>(out var uiScript))
            uiScript.Setup(land);
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