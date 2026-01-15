using UnityEngine;
using UnityEngine.UI;

public class UIBuilding : MonoBehaviour
{
    [SerializeField] private Image fillBar;      // Fill Type = Horizontal
    [SerializeField] private GameObject fullSign;
    [SerializeField] private float smoothingSpeed = 5f;

    private BuildingData _data;
    private float _visualFillAmount;

    // Called once when Building is initialized or upgraded
    public void Initialize(BuildingData data)
    {
        _data = data;
        // Snap immediately on init
        if (_data != null && _data.MaxIncomeStorage > 0)
        {
            _visualFillAmount = (float)(_data.StoredIncome / _data.MaxIncomeStorage);
            fillBar.fillAmount = _visualFillAmount;
        }
    }

    private void Update()
    {
        if (_data == null || _data.MaxIncomeStorage <= 0) return;

        // 1. Calculate the "Real" target percentage based on data
        float targetFill = (float)(_data.StoredIncome / _data.MaxIncomeStorage);

        // 2. Client-Side Prediction (The "Anti-Chunky" Logic)
        // If we aren't full, visually add the earning rate * deltaTime
        if (_data.StoredIncome < _data.MaxIncomeStorage)
        {
            // Calculate how much % we gain per frame
            float percentPerSec = (float)(_data.CachedIncomeRatePerSec / _data.MaxIncomeStorage);
            _visualFillAmount += percentPerSec * Time.deltaTime;
        }

        // 3. Synchronization
        // Sometimes prediction drifts from reality. We Lerp the visual amount 
        // towards the actual backend data 'targetFill' to keep them in sync without snapping.
        // However, if we just collected rent (target is 0), snap down immediately or fast lerp.

        if (targetFill < _visualFillAmount && targetFill < 0.05f)
        {
            // Collection happened: Snap or Fast Drop
            _visualFillAmount = Mathf.Lerp(_visualFillAmount, targetFill, Time.deltaTime * 15f);
        }
        else
        {
            // Standard sync: blend prediction with reality
            // This corrects the visual if the tick update was slightly different than prediction
            _visualFillAmount = Mathf.Lerp(_visualFillAmount, targetFill, Time.deltaTime * smoothingSpeed);
        }

        // 4. Clamp and Apply
        _visualFillAmount = Mathf.Clamp01(_visualFillAmount);
        fillBar.fillAmount = _visualFillAmount;

        // 5. Full Sign Logic
        bool isFull = _data.StoredIncome >= _data.MaxIncomeStorage - 0.01;
        if (fullSign.activeSelf != isFull)
        {
            fullSign.SetActive(isFull);
        }
    }
}
