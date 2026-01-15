public interface IBuildingManager
{
    void Initialize(Building building, BuildingData data);
    void Tick(float deltaTime); // Called every frame or interval
}