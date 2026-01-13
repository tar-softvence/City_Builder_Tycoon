using System.Collections.Generic;
using UnityEngine;

public class LandService : MonoBehaviour
{
    public static LandService Instance { get; private set; }

    // The Master List of State
    private Dictionary<string, LandData> _landDatabase = new Dictionary<string, LandData>();

    // Dependencies
    private IEconomyService _economyService => EconomyManager.Instance;
    private GameBalanceConfig Config => GameManager.Instance.BalanceConfig;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializeDatabase();
    }

    private void OnEnable()
    {
        EventBus<LandEvent>.Subscribe(OnLandEvent);
    }

    private void OnDisable()
    {
        EventBus<LandEvent>.Unsubscribe(OnLandEvent);
    }

    // One method handles all Land-related updates
    private void OnLandEvent(LandEvent data)
    {
        switch (data.Type)
        {
            case LandEventType.TryPurchase:
                AttemptPurchaseLand(data.Subject);
                break;
            case LandEventType.TryConstructBuilding:
                AttemptConstructBuilding(data.Subject);
                break;
        }
    }

    private void InitializeDatabase()
    {
        _landDatabase.Clear();
        Land[] sceneLands = FindObjectsByType<Land>(FindObjectsSortMode.None);

        // TODO: HERE load your save file (JSON/Binary) into a temp dictionary first
        // var savedData = SaveSystem.Load(); 
        // 1. Get data SPECIFIC to this active scene
        CitySaveData citySaveData = SaveManager.Instance?.GetCurrentCityData();

        foreach (Land land in sceneLands)
        {
            // Safety check
            if (land.Data == null) continue;
            if (string.IsNullOrEmpty(land.PlotID)) land.GenerateID();

            // Default State (Inspector)
            bool isOwned = land.IsOwned;
            BuildingSaveData buildingToLoad = null;

            // 2. Try to match PlotID within THIS City's save data
            if (citySaveData != null && citySaveData.Lands.TryGetValue(land.PlotID, out LandSaveData savedLand))
            {
                // Found match in save file!
                isOwned = savedLand.IsOwned;
                buildingToLoad = savedLand.Building;
            }

            // 3. Create Runtime Data
            if (!_landDatabase.ContainsKey(land.PlotID))
            {
                var newData = new LandData
                {
                    PlotID = land.PlotID,
                    Grade = land.Data.Grade,
                    IsOwned = isOwned
                };

                _landDatabase.Add(land.PlotID, newData);

                // 4. Update Visuals
                land.ForceUpdateState(isOwned);

                // 5. Load Building (if owned and building data exists)
                if (isOwned && buildingToLoad != null)
                {
                    // Call the helper we made in BuildingService earlier
                    BuildingService.Instance.LoadSavedBuilding(land, buildingToLoad);
                    newData.CurrentBuilding = BuildingService.Instance.GetBuildingData(land.PlotID);
                }
            }
        }
    }

    public void AttemptPurchaseLand(Land land)
    {
        if (land == null) return;

        LandData data = GetData(land.PlotID);
        if (data == null || data.IsOwned) return;

        double cost = GameMath.CalculateLandCost(Config, data.Grade);

        if (_economyService.TrySpend(CurrencyType.Cash, cost))
        {
            // 1. Update the Data (Master State)
            data.IsOwned = true;

            // 2. Broadcast the success with the Land object as the Subject
            Debug.Log($"Purchase successful for {land.PlotID}");
            EventBus<LandEvent>.Raise(new LandEvent(land, LandEventType.Purchased));
        }
        else
        {
            Debug.Log("Insufficient balance");
        }
    }

    private void AttemptConstructBuilding(Land land)
    {
        if (land == null) return;

        // 1. Validate State
        LandData landData = GetData(land.PlotID);

        // Must be owned and NOT already have a building
        if (landData == null || !landData.IsOwned || landData.CurrentBuilding != null)
        {
            Debug.LogWarning($"Cannot build on {land.PlotID}. Owned: {landData.IsOwned}, HasBuilding: {landData.CurrentBuilding != null}");
            return;
        }

        // Get the building data from the BuildingLevels list in LandDataSO (Land's associated scriptable object)
        BuildingDataSO buildingData = land.Data.BuildingLevels[0]; // Assuming level 0 is the first building level

        // Check if we have a building prefab
        if (buildingData?.BuildingPrefab == null)
        {
            Debug.LogWarning($"No building prefab found for {land.PlotID}");
            return;
        }

        // 2. Calculate Cost (Level 1 Cost)
        double cost = GameMath.CalculateBuildingCost(Config, landData.Grade);

        // Handle transaction: check if the player has enough money
        if (_economyService.TrySpend(CurrencyType.Cash, cost))
        {
            // Instantiate the building prefab as a child of the land
            GameObject prefab = Instantiate(buildingData.BuildingPrefab, land.transform);
            Building building = prefab.GetComponent<Building>();
            // Pass the land and data SO for visual setup
            building.Initialize(land, buildingData);

            // Register in the Service!
            BuildingService.Instance.RegisterNewBuilding(land.PlotID, building, buildingData);

            EventBus<LandEvent>.Raise(new LandEvent(land, LandEventType.BuildingConstructed));

            // Raise the event that the building has been constructed
            EventBus<LandEvent>.Raise(new LandEvent(land, LandEventType.BuildingConstructed));
        }
        else
        {
            Debug.Log("Insufficient funds to construct building.");
        }
    }



    public LandData GetData(string id) => _landDatabase.TryGetValue(id, out var data) ? data : null;
    public Dictionary<string, LandData> GetAllLandData() => _landDatabase;
}