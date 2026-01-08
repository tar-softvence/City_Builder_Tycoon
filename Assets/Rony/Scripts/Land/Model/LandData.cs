[System.Serializable]
public class LandData
{
    public LandGrade Grade;
    public string PlotID;
    public bool IsOwned;
    public BuildingData CurrentBuilding; // Null if no building

}