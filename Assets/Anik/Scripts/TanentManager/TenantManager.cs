using UnityEngine;

public class TenantManager : MonoBehaviour, IBuildingManager
{
    private Building _building;
    private BuildingData _buildingData;

    [Header("Config")]
    public TenantManagerSO managerSO;

    public TenantManagerData Data { get; private set; }

    // ----------------------------------------------------
    // Initialize
    // ----------------------------------------------------
    public void Initialize(Building building, BuildingData data)
    {
        if (building == null || data == null)
        {
            Debug.LogError("[TenantManager] Initialize failed: null reference");
            return;
        }

        if (managerSO == null)
        {
            Debug.LogError("[TenantManager] TenantManagerSO not assigned");
            return;
        }

        _building = building;
        _buildingData = data;

        if (_buildingData.TenantManagerData == null)
        {
            _buildingData.TenantManagerData = new TenantManagerData
            {
                IsAssigned = true,
                ManagerLevel = managerSO.Level,
                Timer = 0f
            };

            Debug.Log($"[TenantManager] New manager data created for {_buildingData.ParentPlotID}");
        }

        Data = _buildingData.TenantManagerData;

        Debug.Log(
            $"[TenantManager] Initialized on {_buildingData.ParentPlotID} | " +
            $"Tenants: {_buildingData.CurrentTenants}/{_buildingData.MaxTenants}"
        );
    }

    // ----------------------------------------------------
    // Timer loop
    // ----------------------------------------------------
    private void Update()
    {
        if (Data == null || !Data.IsAssigned) return;

        Data.Timer += Time.deltaTime;

        if (Data.Timer >= managerSO.TickInterval)
        {
            Tick(Data.Timer);
            Data.Timer = 0f;
        }
    }

    // ----------------------------------------------------
    // Core logic
    // ----------------------------------------------------
    public void Tick(float deltaTime)
    {
        // Already full
        if (_buildingData.CurrentTenants >= _buildingData.MaxTenants)
        {
            Debug.Log(
                $"[TenantManager] {_buildingData.ParentPlotID} is FULL " +
                $"({_buildingData.CurrentTenants}/{_buildingData.MaxTenants})"
            );
            return;
        }

        int before = _buildingData.CurrentTenants;

        _buildingData.CurrentTenants += managerSO.TenantsPerTick;

        if (_buildingData.CurrentTenants > _buildingData.MaxTenants)
            _buildingData.CurrentTenants = _buildingData.MaxTenants;

        int after = _buildingData.CurrentTenants;

        Debug.Log(
            $"[TenantManager] {_buildingData.ParentPlotID} collected tenants: " +
            $"{before} → {after} / {_buildingData.MaxTenants}"
        );

        BuildingService.Instance.RefreshBuildingView(_buildingData.ParentPlotID);
    }
}
