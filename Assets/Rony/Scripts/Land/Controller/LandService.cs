using System.Collections.Generic;
using UnityEngine;

public class LandService : MonoBehaviour
{
    public static LandService Instance { get; private set; }

    // The Master List of State
    private Dictionary<string, LandData> _landDatabase = new Dictionary<string, LandData>();

    // Dependencies
    private IEconomyService _economyService => EconomyManager.Instance;
    private GameBalanceConfig Config => GameManager.Instance.BalanceConfig;


    void Start()
    {
        InitializeDatabase();
    }



    void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        // Use the new Subscribe method
        EventBus<LandEvent>.Subscribe(OnLandEvent);
    }

    private void OnDisable()
    {
        // Use the new Unsubscribe method
        EventBus<LandEvent>.Unsubscribe(OnLandEvent);
    }

    // One method handles all Land-related updates
    private void OnLandEvent(LandEvent data)
    {
        switch (data.Type)
        {
            case LandEventType.TryPurchase:
                AttemptPurchaseLand(data.Subject);
                break;
        }
    }

    private void InitializeDatabase()
    {
        _landDatabase.Clear();
        Land[] sceneLands = FindObjectsByType<Land>(FindObjectsSortMode.None);

        foreach (Land land in sceneLands)
        {
            // Safety check: if for some reason ID is still empty, force generate it
            if (string.IsNullOrEmpty(land.PlotID))
            {
                land.GenerateID();
            }

            if (!_landDatabase.ContainsKey(land.PlotID))
            {
                _landDatabase.Add(land.PlotID, new LandData
                {
                    PlotID = land.PlotID,
                    Grade = land.Grade,
                    IsOwned = land.IsOwned
                });
            }
            else
            {
                Debug.LogWarning($"Duplicate PlotID found: {land.PlotID} on {land.name}");
            }
        }
    }


    public void AttemptPurchaseLand(Land land)
    {
        if (land == null) return;


        LandData data = GetData(land.PlotID);
        if (data == null || data.IsOwned) return;

        double cost = GameMath.CalculateLandCost(Config, data.Grade);

        if (_economyService.TrySpend(CurrencyType.Cash, cost))
        {
            // 1. Update the Data (Master State)
            data.IsOwned = true;

            // 2. Broadcast the success with the Land object as the Subject
            Debug.Log($"Purchase successful for {land.PlotID}");
            EventBus<LandEvent>.Raise(new LandEvent(land, LandEventType.Purchased));
        }
        else
        {
            Debug.Log("Insufficient balance");
        }
    }

    public LandData GetData(string id) => _landDatabase.TryGetValue(id, out var data) ? data : null;







}
