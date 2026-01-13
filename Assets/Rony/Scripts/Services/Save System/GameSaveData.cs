using System.Collections.Generic;

[System.Serializable]
public class GameSaveData
{
    // Key = Scene Build Index (0, 1, 2...), Value = That city's data
    public Dictionary<int, CitySaveData> Cities = new Dictionary<int, CitySaveData>();
}

[System.Serializable]
public class CitySaveData
{
    // Key = PlotID, Value = Land Data
    public Dictionary<string, LandSaveData> Lands = new Dictionary<string, LandSaveData>();
}

[System.Serializable]
public class LandSaveData
{
    public string PlotID;
    public bool IsOwned;
    public BuildingSaveData Building; // Null if no building
}

[System.Serializable]
public class BuildingSaveData
{
    public int Level;
    public int CurrentTenants;
    public double StoredIncome;
    public double LocalMultiplier;
    public float BoostTimeRemaining;
}