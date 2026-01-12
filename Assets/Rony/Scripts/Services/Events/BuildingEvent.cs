using UnityEngine;

public enum BuildingEventType
{
    TryUpgrade,       // UI -> Service
    Upgraded,         // Service -> UI
    TryCollectRent,   // UI -> Service (Optional, if you want to move rent here too)
    RentCollected     // Service -> UI
}

public struct BuildingEvent
{
    public BuildingEventType Type;
    public Building SubjectView; // The visual component
    public string PlotID;        // Helper ID

    public BuildingEvent(Building view, BuildingEventType type)
    {
        SubjectView = view;
        PlotID = view != null ? view.ParentLand.PlotID : ""; // Auto-fetch ID
        Type = type;
    }
}
