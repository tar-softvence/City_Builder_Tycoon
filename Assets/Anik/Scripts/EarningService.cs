using System.Collections;
using UnityEngine;

public class EarningService : MonoBehaviour
{
    public static EarningService Instance;

    private BuildingService buildingService;

    [SerializeField] private float tickInterval = 10f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Now all singletons exist
        buildingService = BuildingService.Instance;

        StartCoroutine(EarningLoop());
    }

    private IEnumerator EarningLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(tickInterval);
            GenerateIncome();
        }
    }

    private void GenerateIncome()
    {
        // Safety check
        if (buildingService == null) return;

        foreach (var entry in buildingService.GetAllBuildings())
        {
            BuildingData data = entry.Value;
            data.StoredIncome += 1000;
            buildingService.RefreshBuildingView(data.ParentPlotID);
        }
    }
}
