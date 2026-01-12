using UnityEngine;
using System.Collections.Generic;

public class BuildingService : MonoBehaviour
{
    public static BuildingService Instance { get; private set; }

    // Master list: Key is PlotID, Value is the data
    private Dictionary<string, BuildingData> _buildingDatabase = new Dictionary<string, BuildingData>();
    // Reference to active MonoBehaviours in the scene to update views
    private Dictionary<string, Building> _activeViews = new Dictionary<string, Building>();

    private IEconomyService _economyService => EconomyManager.Instance;
    private GameBalanceConfig Config => GameManager.Instance.BalanceConfig;

    void Awake() => Instance = this;

    /// <summary>
    /// Creates the data entry and registers the visual view.
    /// </summary>
    public void RegisterNewBuilding(string plotID, Building view, BuildingDataSO initialSO)
    {
        // 1. Create the Data (The Truth)
        BuildingData newData = new BuildingData
        {
            ParentPlotID = plotID,
            Level = initialSO.Level,
            CurrentTenants = initialSO.InitialTenants,
            StoredIncome = 0
        };

        _buildingDatabase[plotID] = newData;
        _activeViews[plotID] = view;

        // --- ADD THESE TWO STEPS ---

        // 2. Tell the Land about the Building so land.CurrentBuilding isn't null
        if (view.ParentLand != null)
        {
            view.ParentLand.SetBuilding(view);
        }

        // 3. Notify the UI (and others) that construction is finished
        EventBus<LandEvent>.Raise(new LandEvent(view.ParentLand, LandEventType.BuildingConstructed));

        // ---------------------------

        // 4. Initialize the View
        view.UpdateView(newData);
    }

    void Update()
    {
        // // Centralized Income Generation
        // foreach (var entry in _buildingDatabase)
        // {
        //     ProcessIncome(entry.Value, Time.deltaTime);
        // }

        //SHOULD BE DONE BY EARNING SERVICE CLASS
    }

    public Dictionary<string, BuildingData> GetAllBuildings()
    {
        return _buildingDatabase;
    }

    public void RefreshBuildingView(string plotID)
    {
        if (_activeViews.TryGetValue(plotID, out Building view) &&
            _buildingDatabase.TryGetValue(plotID, out BuildingData data))
        {
            view.ApplyData(data);
        }
    }



    private void ProcessIncome(BuildingData data, float deltaTime)
    {
        double limit = GameMath.CalculateIncomeLimit(Config, data.Level);
        if (data.StoredIncome >= limit) return;

        double rate = GameMath.CalculateRentPerTenant(Config, data.Level) * data.CurrentTenants;
        double gain = rate * (deltaTime / 3600f);

        data.StoredIncome = System.Math.Min(data.StoredIncome + gain, limit);

        // Push update to the View if it exists in the scene
        if (_activeViews.TryGetValue(data.ParentPlotID, out var view))
        {
            view.UpdateView(data);
        }
    }

    public void UpgradeBuilding(string plotID)
    {
        BuildingData data = GetBuildingData(plotID);
        if (data == null) return;

        double cost = GameMath.CalculateUpgradeCost(Config, data.Level);
        if (EconomyManager.Instance.TrySpend(CurrencyType.Cash, cost))
        {
            data.Level++;
            RefreshView(plotID);
        }
    }

    /// <summary>
    /// Updates the data model and updates the Active View reference
    /// </summary>
    public void ProcessUpgrade(string plotID, Building newView, BuildingDataSO newSO)
    {
        if (!_buildingDatabase.ContainsKey(plotID)) return;

        // 1. Update Data
        BuildingData data = _buildingDatabase[plotID];
        data.Level = newSO.Level;
        // Optionally increase tenant capacity immediately or handled elsewhere
        // data.MaxTenants = newSO.MaxTenants; 

        // 2. Update Active View Reference (Since we destroyed the old GameObject)
        _activeViews[plotID] = newView;

        // 3. Link Land to new Building View
        if (newView.ParentLand != null)
        {
            newView.ParentLand.SetBuilding(newView);
        }

        // 4. Refresh View
        newView.UpdateView(data);
    }

    public void CollectRent(string plotID)
    {
        BuildingData data = GetBuildingData(plotID);
        if (data == null || data.StoredIncome <= 0) return;

        EconomyManager.Instance.AddCurrency(CurrencyType.Cash, data.StoredIncome);
        data.StoredIncome = 0;

        RefreshView(plotID);
    }

    public BuildingData GetBuildingData(string plotID)
    {
        return _buildingDatabase.TryGetValue(plotID, out var data) ? data : null;
    }

    private void RefreshView(string plotID)
    {
        if (_activeViews.TryGetValue(plotID, out Building view))
        {
            view.UpdateView(_buildingDatabase[plotID]);
        }
    }


    private void OnEnable() => EventBus<BuildingEvent>.Subscribe(OnBuildingEvent);
    private void OnDisable() => EventBus<BuildingEvent>.Unsubscribe(OnBuildingEvent);

    private void OnBuildingEvent(BuildingEvent data)
    {
        switch (data.Type)
        {
            case BuildingEventType.TryUpgrade:
                AttemptUpgradeBuilding(data.SubjectView);
                break;
            case BuildingEventType.TryCollectRent:
                CollectRent(data.PlotID);
                break;
        }
    }

    // Moved from LandService and adapted
    private void AttemptUpgradeBuilding(Building oldView)
    {
        if (oldView == null) return;
        string plotID = oldView.ParentLand.PlotID;
        BuildingData data = GetBuildingData(plotID);

        // 1. Validation
        LandDataSO landConfig = oldView.ParentLand.Data;
        int nextLevelIndex = data.Level; // Logic: Level 1 is index 0, so next is index 1 (Level 2)

        if (nextLevelIndex >= landConfig.BuildingLevels.Count)
        {
            Debug.LogWarning("Max Level Reached");
            return;
        }

        // 2. Economy Check
        double cost = GameMath.CalculateUpgradeCost(Config, data.Level);
        if (!_economyService.TrySpend(CurrencyType.Cash, cost)) return;

        // 3. Execution (Swap Prefabs)
        Land parentLand = oldView.ParentLand;
        BuildingDataSO nextSO = landConfig.BuildingLevels[nextLevelIndex];

        // Destroy Old
        Destroy(oldView.gameObject);

        // Create New
        GameObject prefab = Instantiate(nextSO.BuildingPrefab, parentLand.transform);
        Building newView = prefab.GetComponent<Building>();
        newView.Initialize(parentLand, nextSO);

        // 4. Update Data & Register
        data.Level = nextSO.Level;
        RegisterNewBuildingView(plotID, newView); // Helper to update dictionary

        // 5. Notify System with NEW View
        Debug.Log($"Building Upgraded to Level {data.Level}");
        EventBus<BuildingEvent>.Raise(new BuildingEvent(newView, BuildingEventType.Upgraded));
    }

    private void RegisterNewBuildingView(string plotID, Building view)
    {
        // Update local cache
        // _activeViews[plotID] = view; // If you have this dict

        // Ensure Land knows about the new script
        view.ParentLand.SetBuilding(view);
        view.UpdateView(GetBuildingData(plotID));
    }








}
