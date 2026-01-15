using UnityEngine;

public class TenantManager : MonoBehaviour, IBuildingManager
{
    private Building _building;
    private BuildingData _data;

    [Header("SO Settings")]
    public TenantManagerSO managerSO; // assign this in inspector

    private float _timer;

    public void Initialize(Building building, BuildingData data)
    {
        if (building == null)
        {
            Debug.LogError("TenantManager Initialize failed: building is null");
            return;
        }

        if (data == null)
        {
            Debug.LogError("TenantManager Initialize failed: data is null");
            return;
        }

        if (managerSO == null)
        {
            Debug.LogError($"TenantManager SO is not assigned for building {data.ParentPlotID}");
            return;
        }

        _building = building;
        _data = data;
        _timer = 0f;

        Debug.Log($"TenantManager initialized for {_data.ParentPlotID} with Level {managerSO.Level}");
    }


    private void Update()
    {
        if (_data == null || managerSO == null) return;

        _timer += Time.deltaTime;
        if (_timer >= managerSO.TickInterval)
        {
            Tick(_timer);
            _timer = 0f;
        }
    }

    public void Tick(float deltaTime)
    {
        if (_data.CurrentTenants >= GetMaxTenants()) return;

        _data.CurrentTenants += managerSO.TenantsPerTick;

        // Clamp to max tenants
        _data.CurrentTenants = Mathf.Min(_data.CurrentTenants, GetMaxTenants());

        // Refresh the building view
        BuildingService.Instance.RefreshBuildingView(_data.ParentPlotID);

        Debug.Log($"[{_data.ParentPlotID}] Current Tenants: {_data.CurrentTenants}/{GetMaxTenants()}");
    }

    private int GetMaxTenants()
    {
        return _data.Level * managerSO.MaxTenantsPerLevel;
    }
}