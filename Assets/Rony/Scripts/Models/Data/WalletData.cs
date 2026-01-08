[System.Serializable]
public class WalletData
{
    public double Cash;
    public double Diamonds;
    public double LandTokens;

    // Helper to get value by enum
    public double GetValue(CurrencyType type) => type switch
    {
        CurrencyType.Cash => Cash,
        CurrencyType.Diamonds => Diamonds,
        CurrencyType.LandTokens => LandTokens,
        _ => 0
    };

    // Helper to set value by enum
    public void SetValue(CurrencyType type, double amount)
    {
        switch (type)
        {
            case CurrencyType.Cash: Cash = amount; break;
            case CurrencyType.Diamonds: Diamonds = amount; break;
            case CurrencyType.LandTokens: LandTokens = amount; break;
        }
    }
}

