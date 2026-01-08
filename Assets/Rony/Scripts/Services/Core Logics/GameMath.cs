using UnityEngine;

public static class GameMath
{
    // --- Land & Upgrade ---
    public static double CalculateLandCost(GameBalanceConfig config, LandGrade grade)
    {
        return config.BaseLandPrice * config.GetLandMultiplier(grade);
    }

    public static double CalculateUpgradeCost(GameBalanceConfig config, int currentLevel)
    {
        return config.BaseBuildingCost * Mathf.Pow(config.UpgradeCostExponent, currentLevel - 1);
    }

    // --- Rent ---
    public static double CalculateRentPerTenant(GameBalanceConfig config, int level)
    {
        return config.BaseRent * (1 + config.RentLevelGrowth * (level - 1));
    }

    // --- Dynamic Tenant Count ---
    // Formula: Base + (Level - 1) * Growth
    public static int CalculateMaxTenants(GameBalanceConfig config, int level)
    {
        return config.BaseTenantCount + (config.TenantsPerLevel * (level - 1));
    }

    // --- Dynamic Income Limit ---
    // Formula: Base * (Growth ^ (Level - 1))
    // We use exponential growth here so high-level buildings don't fill up instantly.
    public static double CalculateIncomeLimit(GameBalanceConfig config, int level)
    {
        return config.BaseIncomeLimit * Mathf.Pow(config.IncomeLimitExponent, level - 1);
    }
}