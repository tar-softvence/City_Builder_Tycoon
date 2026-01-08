// 1. Define the types of things that can happen to Land
public enum LandEventType
{
    Selected,
    Deselected,
    TryPurchase,
    Purchased,
    BuildingConstructed
}

// 2. The single struct that handles ALL Land events
public struct LandEvent
{
    public Land Subject;      // Which land?
    public LandEventType Type; // What happened to it?

    public LandEvent(Land subject, LandEventType type)
    {
        Subject = subject;
        Type = type;
    }
}