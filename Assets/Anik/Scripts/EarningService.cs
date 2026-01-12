using System;
using System.Collections;
using UnityEngine;

public class EarningService : MonoBehaviour
{
    public static EarningService Instance;

    private BuildingService buildingService;

    private GameBalanceConfig Config => GameManager.Instance.BalanceConfig;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Grab BuildingService singleton
        buildingService = BuildingService.Instance;

        // Safety check
        if (buildingService == null)
        {
            Debug.LogError("BuildingService instance not found in scene!");
            return;
        }

        // Start income loop
        StartCoroutine(EarningLoop());
    }

    private IEnumerator EarningLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Config.tickInterval);
            GenerateIncome();
        }
    }

    private void GenerateIncome()
    {
        if (buildingService == null) return;

        GameBalanceConfig config = GameManager.Instance?.BalanceConfig;
        if (config == null) return;

        foreach (var entry in buildingService.GetAllBuildings())
        {
            BuildingData data = entry.Value;

            // Rent and max tenants
            double rent = GameMath.CalculateRentPerTenant(config, data.Level);
            int maxTenants = GameMath.CalculateMaxTenants(config, data.Level);

            // Storage limit
            data.MaxIncomeStorage = GameMath.CalculateIncomeLimit(config, data.Level);

            // Income per tick
            double incomeThisTick = rent * data.CurrentTenants;

            // Add income with cap
            data.StoredIncome = Math.Min(data.StoredIncome + incomeThisTick, data.MaxIncomeStorage);

            // Debug when storage full
            if (Math.Abs(data.StoredIncome - data.MaxIncomeStorage) < 0.01) 
            {
                float timeToFull = GameMath.CalculateTimeToFull(config, data.Level, data.CurrentTenants, 10f);
                Debug.Log($"Income full on {data.ParentPlotID} at {data.StoredIncome}! Time to full was approx {timeToFull:F2} sec.");
            }

            // Refresh UI
            buildingService.RefreshBuildingView(data.ParentPlotID);
        }
    }



}
