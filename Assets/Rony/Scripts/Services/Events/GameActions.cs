using UnityEngine;

/// <summary>
/// A centralized "Remote Control" for the game. 
/// Any UI or Input script calls these methods to request changes.
/// </summary>
public static class GameActions
{
    // --- Land Actions ---

    public static void RequestPurchaseLand(Land land)
    {
        if (land == null) return;
        Debug.Log($"[Action] Requesting purchase of {land.PlotID}");
        EventBus<LandEvent>.Raise(new LandEvent(land, LandEventType.TryPurchase));
    }

    public static void RequestConstructBuilding(Land land)
    {
        if (land == null) return;
        Debug.Log($"[Action] Requesting construction on {land.PlotID}");
        EventBus<LandEvent>.Raise(new LandEvent(land, LandEventType.TryConstructBuilding));
    }

    // --- Building Actions ---

    public static void RequestUpgradeBuilding(Building building)
    {
        if (building == null) return;
        Debug.Log($"[Action] Requesting upgrade for {building.name}");
        EventBus<BuildingEvent>.Raise(new BuildingEvent(building, BuildingEventType.TryUpgrade));
    }

    public static void RequestCollectRent(Building building)
    {
        if (building == null) return;
        // Logic check: Can't collect if null, etc.
        EventBus<BuildingEvent>.Raise(new BuildingEvent(building, BuildingEventType.TryCollectRent));
    }

    // Overload for convenience if you only have the ID (optional, requires Service lookup if needed, 
    // but usually UI has the Building object. If not, stick to passing the View)
    public static void RequestCollectRent(string plotID)
    {
        // Note: Your current BuildingEvent requires a 'SubjectView' (Building). 
        // If you only have an ID, you might need to refactor Event to accept ID, 
        // or look it up here. For now, we assume the UI has the building reference.
    }
}