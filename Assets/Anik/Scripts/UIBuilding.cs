using UnityEngine;
using UnityEngine.UI;

public class UIBuilding : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillBar;
    [SerializeField] private GameObject fullSign;

    [Header("Settings")]
    [SerializeField] private float updateInterval = 0.1f;

    private BuildingData buildingData;
    private float _timer;

    public void Initialize(BuildingData data)
    {
        buildingData = data;
        UpdateUI(); // Initial update
    }

    private void Update()
    {
        if (buildingData == null || updateInterval <= 0f) return;

        _timer += Time.deltaTime;
        if (_timer >= updateInterval)
        {
            _timer = 0f;
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        if (buildingData == null || fillBar == null) return;

        // Fill bar calculation
        float fillAmount = buildingData.MaxIncomeStorage > 0
            ? (float)(buildingData.StoredIncome / buildingData.MaxIncomeStorage)
            : 0f;

        fillBar.fillAmount = Mathf.Clamp01(fillAmount);

        // Full sign visibility
        if (fullSign != null)
            fullSign.SetActive(buildingData.StoredIncome >= buildingData.MaxIncomeStorage);
    }

    public void CollectIncome()
    {
        if (buildingData == null || buildingData.StoredIncome <= 0) return;

        // Add to player currency
        EconomyManager.Instance.AddCurrency(CurrencyType.Cash, buildingData.StoredIncome);

        // Reset stored income
        buildingData.StoredIncome = 0;

        // Update UI immediately
        UpdateUI();
    }
}
