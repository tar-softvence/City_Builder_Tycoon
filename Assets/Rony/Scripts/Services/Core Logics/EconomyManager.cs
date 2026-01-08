using System;
using UnityEngine;

public class EconomyManager : MonoBehaviour, IEconomyService
{

    // Singleton Instance
    public static EconomyManager Instance { get; private set; }
    public event Action<CurrencyType, double> OnBalanceChanged;

    [SerializeField] private WalletData _walletData = new WalletData();

    private void Awake()
    {
        // Singleton Logic
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: keeps money across scenes
    }

    public double GetBalance(CurrencyType type) => _walletData.GetValue(type);

    public bool HasEnough(CurrencyType type, double amount) => GetBalance(type) >= amount;

    public void AddCurrency(CurrencyType type, double amount)
    {
        double newAmount = GetBalance(type) + amount;
        _walletData.SetValue(type, newAmount);

        OnBalanceChanged?.Invoke(type, newAmount);
    }



    public bool TrySpend(CurrencyType type, double amount)
    {
        if (HasEnough(type, amount))
        {
            double newAmount = GetBalance(type) - amount;
            _walletData.SetValue(type, newAmount);

            OnBalanceChanged?.Invoke(type, newAmount);
            return true;
        }
        return false;
    }


    public void ExchangeDiamondsForCash(int diamondAmount)
    {
        if (TrySpend(CurrencyType.Diamonds, diamondAmount))
        {
            double cashGained = diamondAmount * GetDiamondToCashExchangeRate();
            AddCurrency(CurrencyType.Cash, cashGained);
        }
    }

    public double GetDiamondToCashExchangeRate() => 1000.0; // 1 Diamond = $1000
}
