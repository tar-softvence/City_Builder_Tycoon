using UnityEngine;

[CreateAssetMenu(fileName = "NewBuilding", menuName = "Land/Building Data")]
public class BuildingDataSO : ScriptableObject
{
    [Header("Visuals")]
    public int Level;
    public GameObject BuildingPrefab;
    public Sprite BuildingUISprite;

    [Header("Tenant Stats")]
    public int MaxTenants;
    public int InitialTenants;
}