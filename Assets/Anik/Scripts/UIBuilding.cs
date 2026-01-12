using UnityEngine;
using UnityEngine.UI;

public class UIBuilding : MonoBehaviour
{
    [SerializeField] private Image fillBar;      // Fill Type = Horizontal
    [SerializeField] private GameObject fullSign;

    public void UpdateUI(BuildingData data)
    {
        if (data.MaxIncomeStorage <= 0)
        {
            fillBar.fillAmount = 0;
            fullSign.SetActive(false);
            return;
        }

        float percent = (float)(data.StoredIncome / data.MaxIncomeStorage);
        fillBar.fillAmount = percent;

        // Show FULL sign when storage is full
        bool isFull = data.StoredIncome >= data.MaxIncomeStorage - 0.01;
        fullSign.SetActive(isFull);
    }
}
