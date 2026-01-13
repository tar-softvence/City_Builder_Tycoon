using System;
using System.Collections;
using UnityEngine;

public class EarningService : MonoBehaviour
{
    public static EarningService Instance;

    private BuildingService buildingService;

    private GameBalanceConfig Config => GameManager.Instance.BalanceConfig;

    // Multiplier State
    private float _globalMultiplier = 1.0f;
    private Coroutine _globalBoostCoroutine;

    // Enum for Time Periods
    public enum TimePeriod { Second, Minute, Hour }


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
            GenerateIncome(Config.tickInterval);
        }
    }

    private void GenerateIncome(float tickDuration)
    {
        if (buildingService == null) return;

        foreach (var entry in buildingService.GetAllBuildings())
        {
            BuildingData data = entry.Value;

            // 1. Handle Local Boost Timer
            if (data.BoostTimeRemaining > 0)
            {
                data.BoostTimeRemaining -= tickDuration;
                if (data.BoostTimeRemaining <= 0)
                {
                    data.BoostTimeRemaining = 0;
                    data.LocalMultiplier = 1.0; // Reset when time runs out
                    Debug.Log($"Boost ended for {data.ParentPlotID}");
                    // Optional: Refresh UI to remove boost icon
                    buildingService.RefreshBuildingView(data.ParentPlotID);
                }
            }

            // 2. Calculate Combined Multiplier
            // Example: Global 2x * Local 3x = 6x Total Boost
            double totalMultiplier = _globalMultiplier * data.LocalMultiplier;

            // 3. Base Math
            double rent = GameMath.CalculateRentPerTenant(Config, data.Level);
            data.MaxIncomeStorage = GameMath.CalculateIncomeLimit(Config, data.Level);

            // 4. Calculate Income
            double baseIncome = rent * data.CurrentTenants;
            double finalIncome = baseIncome * totalMultiplier;

            // 5. Cache Rate for UI (Per Second)
            // (If tick is 1s, this equals finalIncome. If tick is 0.5s, we double it for the "Per Sec" rate)
            data.CachedIncomeRatePerSec = finalIncome / tickDuration;

            // 6. Apply to Storage
            data.StoredIncome = Math.Min(data.StoredIncome + finalIncome, data.MaxIncomeStorage);
        }
    }

    // --- BOOST METHODS ---

    /// <summary>
    /// Boosts specific building (e.g., Item usage)
    /// </summary>
    public void ApplyBuildingBoost(string plotID, double multiplier, float duration)
    {
        var data = buildingService.GetBuildingData(plotID);
        if (data != null)
        {
            data.LocalMultiplier = multiplier;
            data.BoostTimeRemaining = duration;
            Debug.Log($"Applied {multiplier}x boost to {plotID} for {duration}s");

            // Optional: Force a view refresh immediately if you have visual effects (like particles)
            buildingService.RefreshBuildingView(plotID);
        }
    }

    /// <summary>
    /// Boosts ALL buildings (e.g., Global Event / Ad Watch)
    /// </summary>
    public void ApplyGlobalBoost(float multiplier, float duration)
    {
        if (_globalBoostCoroutine != null) StopCoroutine(_globalBoostCoroutine);
        _globalBoostCoroutine = StartCoroutine(GlobalBoostRoutine(multiplier, duration));
    }

    private IEnumerator GlobalBoostRoutine(float multiplier, float duration)
    {
        _globalMultiplier = multiplier;
        Debug.Log($"GLOBAL BOOST: x{multiplier} started.");
        yield return new WaitForSeconds(duration);
        _globalMultiplier = 1.0f;
        Debug.Log("GLOBAL BOOST: Ended.");
    }

    private IEnumerator BoostRoutine(float multiplier, float duration)
    {
        _globalMultiplier = multiplier;
        Debug.Log($"Economy Boost Activated: x{multiplier} for {duration}s");

        // Optional: Trigger UI event here to show a boost icon

        yield return new WaitForSeconds(duration);

        _globalMultiplier = 1.0f;
        Debug.Log("Economy Boost Ended");
    }

    public float GetCurrentMultiplier() => _globalMultiplier;


    // --- 3. PREDICTION & STATS ---

    public double CalculateCurrentRate(BuildingData data)
    {
        if (data == null) return 0;

        // Perform the math manually for the UI
        double rent = GameMath.CalculateRentPerTenant(Config, data.Level);
        double totalMultiplier = _globalMultiplier * data.LocalMultiplier;
        return rent * data.CurrentTenants * totalMultiplier;
    }


    /// <summary>
    /// Returns the current earning rate for a specific building/plot.
    /// Includes all active Global and Local multipliers.
    /// </summary>
    public double GetBuildingIncomeRate(string plotID, TimePeriod period)
    {
        // 1. Get Data
        var data = buildingService.GetBuildingData(plotID);
        if (data == null) return 0;

        // 2. Get Base Rate (Per Second)
        // We use the cached value because it already includes:
        // - Tenant Count
        // - Global Multiplier
        // - Local Multiplier
        double ratePerSec = data.CachedIncomeRatePerSec;

        // Safety: If result is invalid or infinite, return 0
        if (double.IsNaN(ratePerSec) || double.IsInfinity(ratePerSec))
            return 0;

        // FIX: If the loop hasn't run yet, calculate it on the fly
        if (ratePerSec <= 0 && data.CurrentTenants > 0)
        {
            ratePerSec = CalculateCurrentRate(data);
        }

        // 3. Scale by Period
        switch (period)
        {
            case TimePeriod.Minute: return ratePerSec * 60;
            case TimePeriod.Hour: return ratePerSec * 3600;
            default: return ratePerSec; // Second
        }
    }



    public double GetTotalIncome(TimePeriod period)
    {
        if (buildingService == null) return 0;

        double totalRatePerSec = 0;

        foreach (var entry in buildingService.GetAllBuildings())
        {
            // BuildingData data = entry.Value;
            // double rent = GameMath.CalculateRentPerTenant(Config, data.Level);
            // // Base rate per second * Multiplier
            // double bldRate = (rent * data.CurrentTenants * _globalMultiplier);
            // totalRatePerSec += bldRate;



            // Simply sum up the cached rates we calculated in the loop
            // This is much more performant than recalculating everything
            totalRatePerSec += entry.Value.CachedIncomeRatePerSec;
        }

        switch (period)
        {
            case TimePeriod.Minute: return totalRatePerSec * 60;
            case TimePeriod.Hour: return totalRatePerSec * 3600;
            default: return totalRatePerSec;
        }
    }



}
