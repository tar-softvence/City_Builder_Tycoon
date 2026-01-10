using UnityEngine;
using System.Collections.Generic;

public class BuildingService : MonoBehaviour
{
    public static BuildingService Instance { get; private set; }

    // Master list: Key is PlotID, Value is the data
    private Dictionary<string, BuildingData> _buildingDatabase = new Dictionary<string, BuildingData>();
    // Reference to active MonoBehaviours in the scene to update views
    private Dictionary<string, Building> _activeViews = new Dictionary<string, Building>();

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

        // 2. Initialize the View
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




}
