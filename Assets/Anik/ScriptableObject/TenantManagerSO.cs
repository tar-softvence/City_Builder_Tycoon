using UnityEngine;

[CreateAssetMenu(fileName = "NewTenantManager", menuName = "Managers/TenantManagerSO")]
public class TenantManagerSO : ScriptableObject
{
    [Header("Tenant Manager Level Settings")]
    public int Level = 1;

    [Tooltip("Tenants added per tick interval")]
    public int TenantsPerTick = 1;

    [Tooltip("Time in seconds between tenant increases")]
    public float TickInterval = 5f;

}