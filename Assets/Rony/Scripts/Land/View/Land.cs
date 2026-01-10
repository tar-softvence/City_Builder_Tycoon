using Unity.Collections;
using UnityEngine;

/// <summary>
/// Represents a specific plot in the game world. 
/// Manages ownership state and building construction.
/// </summary>
public class Land : MonoBehaviour, ISelectable
{
    [Header("Configuration")]
    [SerializeField] private LandDataSO landData; // Reference to the SO

    [Header("Plot Details")]
    [HideInInspector][ReadOnly] public string PlotID; // must be unique 
    [HideInInspector] public bool IsOwned; // Acts as a local cache of the state

    private Building _currentBuilding;
    [SerializeField] SpriteRenderer selectionSprite;

    // Prefab instances managed locally
    private GameObject _nonOwnedInstance;
    private GameObject _ownedInstance;

    public Building CurrentBuilding => _currentBuilding;
    public LandDataSO Data => landData;
    public bool CanBuild => IsOwned && _currentBuilding == null;

    // We assume you have a GameManager singleton holding the Config
    private GameBalanceConfig Config => GameManager.Instance.BalanceConfig;

    // Changed from Start to Awake so prefabs are ready when LandService initializes
    void Awake()
    {
        if (landData == null)
        {
            Debug.LogError($"The Land script on {name} is missing a LandDataSO reference!", this);
            return;
        }

        selectionSprite.enabled = false;

        // Instantiate visuals immediately so they are ready for the Service
        CreateVisualInstances();
    }

    private void CreateVisualInstances()
    {
        if (landData == null) return;

        // Instantiate the visuals from the SO
        _nonOwnedInstance = Instantiate(landData.NotOwnedLandPrefab, transform);
        _ownedInstance = Instantiate(landData.OwnedLandPrefab, transform);

        // We do not set Active/Inactive here anymore. 
        // We wait for ForceUpdateState to tell us what to show.
        // But we default them to off to prevent visual glitching before init.
        _nonOwnedInstance.SetActive(false);
        _ownedInstance.SetActive(false);
    }

    /// <summary>
    /// Called by LandService to strictly enforce the "Source of Truth".
    /// This ensures the visual matches the Database/SaveFile, not the Inspector checkbox.
    /// </summary>
    public void ForceUpdateState(bool ownershipStatus)
    {
        this.IsOwned = ownershipStatus;
        RefreshVisuals();
    }

    private void RefreshVisuals()
    {
        if (_nonOwnedInstance == null || _ownedInstance == null) return;

        _nonOwnedInstance.SetActive(!IsOwned);
        _ownedInstance.SetActive(IsOwned);
    }

    public void OnSelect()
    {
        selectionSprite.enabled = true;
        // RAISE: Selected
        EventBus<LandEvent>.Raise(new LandEvent(this, LandEventType.Selected));
    }

    public void OnDeSelect()
    {
        selectionSprite.enabled = false;
        // RAISE: Deselected
        EventBus<LandEvent>.Raise(new LandEvent(this, LandEventType.Deselected));
    }

    private void OnEnable() => EventBus<LandEvent>.Subscribe(UpdateOwnership);
    private void OnDisable() => EventBus<LandEvent>.Unsubscribe(UpdateOwnership);

    private void UpdateOwnership(LandEvent data)
    {
        // Check if THIS specific land was the one purchased
        if (data.Type == LandEventType.Purchased && data.Subject == this)
        {
            this.IsOwned = true;
            RefreshVisuals(); // Reuse the logic
            Debug.Log($"{PlotID} has updated its local state to Owned.");
        }
    }

    // Use OnValidate to generate the ID automatically in the Editor
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(PlotID))
        {
            GenerateID();
        }
    }

    [ContextMenu("Generate New ID")]
    public void GenerateID()
    {
        // Option A: Use Position (Great for Grids)
        PlotID = $"Plot_{Mathf.RoundToInt(transform.position.x)}_{Mathf.RoundToInt(transform.position.z)}";

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}