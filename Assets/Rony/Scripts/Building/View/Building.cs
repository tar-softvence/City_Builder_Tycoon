using UnityEngine;

/// <summary>
/// Represents a revenue-generating structure placed on a Land plot.
/// Handles income generation, storage limits, and interaction via <see cref="ISelectable"/>.
/// </summary>
public class Building : MonoBehaviour, ISelectable
{

    public Land ParentLand { get; private set; }
    public BuildingDataSO Data { get; private set; } // Reference to the SO

    // The View only stores what it needs to display
    [Header("Display Stats (Read Only)")]
    public int CurrentLevel;
    public double VisualIncome;
    [SerializeField] private UIBuilding ui;

    public string PlotID;

    public void ApplyData(BuildingData data)
    {
        ui.UpdateUI(data);
    }

    /// <summary>
    /// Links the building to its specific plot of land.
    /// </summary>
    public void Initialize(Land land, BuildingDataSO buildingData)
    {
        ParentLand = land;
        Data = buildingData;


    }


    // The Service calls this to update the visual/UI state
    public void UpdateView(BuildingData state)
    {
        CurrentLevel = state.Level;
        VisualIncome = state.StoredIncome;
        // Update UI bars, 3D models, or text labels here
    }






    public void OnSelect()
    {
        // Logic: Trigger UI Manager to show the Building Management Panel

        if (ParentLand != null)
        {
            ParentLand.OnSelect();
        }
    }

    public void OnDeSelect()
    {
        // Logic: Close the Management Panel
        if (ParentLand != null)
        {
            ParentLand.OnDeSelect();
        }
    }
}