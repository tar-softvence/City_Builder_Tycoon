using UnityEngine;

[CreateAssetMenu(fileName = "GameBalanceConfig", menuName = "Game/Balance Config")]
public class GameBalanceConfig : ScriptableObject
{
    [Header("Global Base Prices")]
    public float BaseLandPrice = 500f; // Was hardcoded in Land.cs
    public float BaseBuildingCost = 1000f;

    [Header("Land Pricing Multipliers")]
    public float GradeAMultiplier = 4.0f;
    public float GradeBMultiplier = 3.0f;
    public float GradeCMultiplier = 2.0f;
    public float GradeDMultiplier = 1.0f;

    [Header("Rent & Income Settings")]
    public float BaseRent = 50f;
    public float RentLevelGrowth = 0.1f; // 10% increase per level

    [Header("Tenant Settings")]
    public int BaseTenantCount = 5;
    [Tooltip("How many extra tenants per level? (e.g. 2 means Level 2 has 7 tenants)")]
    public int TenantsPerLevel = 100;

    [Header("Storage Limit Settings")]
    public float BaseIncomeLimit = 1000f;
    [Tooltip("How much the storage cap grows per level. 1.2 = 20% growth.")]
    public float IncomeLimitExponent = 1.25f;

    [Header("Maintenance Settings")]
    public float MaintenanceLevelGrowth = 0.05f;

    [Header("Upgrade Settings")]
    public float UpgradeCostExponent = 1.2f;

    // Helper to get Land Grade quickly
    public float GetLandMultiplier(LandGrade grade)
    {
        return grade switch
        {
            LandGrade.A => GradeAMultiplier,
            LandGrade.B => GradeBMultiplier,
            LandGrade.C => GradeCMultiplier,
            _ => GradeDMultiplier
        };
    }
}