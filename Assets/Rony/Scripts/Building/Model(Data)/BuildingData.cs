[System.Serializable]
public class BuildingData
{
    public string ParentPlotID;
    public int Level = 1;
    public int CurrentTenants;
    public double StoredIncome;
    public bool HasManager;
    public double MaxIncomeStorage;

    // --- NEW: Per-Building Multiplier Data ---
    public double LocalMultiplier = 1.0;
    public float BoostTimeRemaining = 0f; // In seconds
    // -----------------------------------------

    public double CachedIncomeRatePerSec;
    public int MaxTenants => 10 * Level;
     



}
