using UnityEngine;
using System;


public interface IEconomyService
{
    // Events for UI to subscribe to
    event Action<CurrencyType, double> OnBalanceChanged;

    /// <summary>
    /// Attempts to deduct currency. Returns true if successful.
    /// </summary>
    bool TrySpend(CurrencyType type, double amount);

    double GetBalance(CurrencyType type);
    void AddCurrency(CurrencyType type, double amount);
    double GetDiamondToCashExchangeRate();
    void ExchangeDiamondsForCash(int diamondAmount);
}
