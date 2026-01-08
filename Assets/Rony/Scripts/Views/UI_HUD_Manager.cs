using UnityEngine;
using TMPro;

public class UI_HUD_Manager : MonoBehaviour
{
    [Header("Currency Text References")]
    [SerializeField] private TMP_Text cashText;
    [SerializeField] private TMP_Text diamondsText;
    [SerializeField] private TMP_Text tokensText;

    private IEconomyService _economyService;

    void Start()
    {
        _economyService = FindFirstObjectByType<EconomyManager>();

        if (_economyService != null)
        {
            // 2. Subscribe to the event
            _economyService.OnBalanceChanged += HandleBalanceChanged;

            // 3. Initial UI Setup (Set starting values from the wallet)
            UpdateUI(CurrencyType.Cash, _economyService.GetBalance(CurrencyType.Cash));
            UpdateUI(CurrencyType.Diamonds, _economyService.GetBalance(CurrencyType.Diamonds));
            UpdateUI(CurrencyType.LandTokens, _economyService.GetBalance(CurrencyType.LandTokens));
        }
    }

    private void OnDestroy()
    {
        // Always unsubscribe from events when the object is destroyed to prevent memory leaks
        if (_economyService != null)
        {
            _economyService.OnBalanceChanged -= HandleBalanceChanged;
        }
    }

    private void HandleBalanceChanged(CurrencyType type, double newAmount)
    {
        UpdateUI(type, newAmount);
    }

    private void UpdateUI(CurrencyType type, double amount)
    {
        // Formatting numbers: "N0" adds commas (e.g., 1,000,000)
        string formattedAmount = amount.ToString("N0");

        switch (type)
        {
            case CurrencyType.Cash:
                cashText.text = $"${formattedAmount}";
                break;
            case CurrencyType.Diamonds:
                diamondsText.text = formattedAmount;
                break;
            case CurrencyType.LandTokens:
                tokensText.text = formattedAmount;
                break;
        }
    }
}
