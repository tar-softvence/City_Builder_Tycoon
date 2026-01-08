using Unity.Collections;
using UnityEngine;

/// <summary>
/// Represents a specific plot in the game world. 
/// Manages ownership state and building construction.
/// </summary>
public class Land : MonoBehaviour, ISelectable
{

    [Header("Plot Details")]
    [ReadOnly] public string PlotID; // must be unique 
    public LandGrade Grade;
    public bool IsOwned;

    [SerializeField] private Building _currentBuilding;

    [SerializeField] private GameObject nonOwnedLandPrefab;
    [SerializeField] private GameObject ownedLandPrefab;
    [SerializeField] SpriteRenderer selectionSprite;

    public Building CurrentBuilding => _currentBuilding;
    public bool CanBuild => IsOwned && _currentBuilding == null;

    // We assume you have a GameManager singleton holding the Config
    private GameBalanceConfig Config => GameManager.Instance.BalanceConfig;

    void Start()
    {
        selectionSprite.enabled = false;
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
            nonOwnedLandPrefab.SetActive(false);
            ownedLandPrefab.SetActive(true);
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

        // Option B: Use a Random Unique GUID (If you prefer absolute randomness)
        // PlotID = System.Guid.NewGuid().ToString();

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }






}