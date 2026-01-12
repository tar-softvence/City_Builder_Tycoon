[System.Serializable]
public class BuildingData
{
    public string ParentPlotID;
    public int Level = 1;
    public int CurrentTenants;
    public double StoredIncome;
    public bool HasManager;
    public double MaxIncomeStorage;

    public GameBalanceConfig Config; // <--- Add this
}
