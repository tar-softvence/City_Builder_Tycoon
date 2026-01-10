using UnityEngine;

/// <summary>
/// Represents a revenue-generating structure placed on a Land plot.
/// Handles income generation, storage limits, and interaction via <see cref="ISelectable"/>.
/// </summary>
public class Building : MonoBehaviour, ISelectable
{

    public Land ParentLand { get; private set; }
    public BuildingDataSO Data { get; private set; } // Reference to the SO

    [Header("Level & Growth")]
    public int Level = 1;
    public int currentTenants = 0;
    // Dynamic Stats (Calculated, not hardcoded)
    public int MaxTenants { get; private set; }
    public double IncomeLimit { get; private set; }
    public double CurrentStoredIncome { get; private set; }

    private GameBalanceConfig Config => GameManager.Instance.BalanceConfig;

    /// <summary>
    /// Links the building to its specific plot of land.
    /// </summary>
    public void Initialize(Land land, BuildingDataSO buildingData)
    {
        ParentLand = land;
        Data = buildingData;

        // Apply initial stats from the SO
        if (Data != null)
        {
            this.Level = Data.Level;
            this.currentTenants = Data.InitialTenants;
        }

        // Recalculate dynamic limits based on the new Level/Data
        RecalculateStats();
    }



    // Call this whenever Level changes
    public void RecalculateStats()
    {
        MaxTenants = GameMath.CalculateMaxTenants(Config, Level);
        IncomeLimit = GameMath.CalculateIncomeLimit(Config, Level);
    }

    public void Upgrade()
    {
        double cost = GameMath.CalculateUpgradeCost(Config, Level);

        if (EconomyManager.Instance.TrySpend(CurrencyType.Cash, cost))
        {
            Level++;
            RecalculateStats(); // Important: Update limits immediately
            Debug.Log($"Upgraded to Level {Level}. Max Tenants: {MaxTenants}, Limit: ${IncomeLimit:N0}");
        }
    }



    /// <summary>
    /// Calculates and adds income based on time elapsed. 
    /// Should be called by a centralized TimeService or Update loop.
    /// </summary>
    /// <param name="deltaTime">Time passed since last tick.</param>
    public void GenerateIncome(float deltaTime)
    {
        if (CurrentStoredIncome >= IncomeLimit) return;

        double rate = GameMath.CalculateRentPerTenant(Config, Level) * MaxTenants;
        double gain = rate * (deltaTime / 3600f); // Per hour to per second

        CurrentStoredIncome = System.Math.Min(CurrentStoredIncome + gain, IncomeLimit);
    }

    /// <summary>
    /// Transfers stored income to the global EconomyManager and resets internal storage.
    /// </summary>
    public void CollectIncome()
    {
        if (CurrentStoredIncome <= 0) return;

        EconomyManager.Instance.AddCurrency(CurrencyType.Cash, CurrentStoredIncome);
        CurrentStoredIncome = 0;
    }

    public void OnSelect()
    {
        // Logic: Trigger UI Manager to show the Building Management Panel
    }

    public void OnDeSelect()
    {
        // Logic: Close the Management Panel
    }
}