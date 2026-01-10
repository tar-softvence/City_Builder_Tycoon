using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LandUI : MonoBehaviour
{
    [SerializeField] TMP_Text landName;
    [SerializeField] TMP_Text landTier;

    //Details 
    [SerializeField] TMP_Text level;
    [SerializeField] TMP_Text currentTanents;
    [SerializeField] TMP_Text earnings;
    [SerializeField] TMP_Text life;

    [Header("Buttons")]
    [SerializeField] Button BuyButton;
    [SerializeField] GameObject buyButtonParentPanel;

    [Header("Detail Panel Settings")]
    [SerializeField] DetailButtonPanel detailButtonPanelPrefab;
    [SerializeField] Transform detailButtonPanelParent; // The container
    private DetailButtonPanel activeDetailButtonPanel;

    private GameBalanceConfig Config => GameManager.Instance.BalanceConfig;


    Land thisLand;

    bool isLandOwned;
    Building currentBuilding = null;




    public void Setup(Land landData)
    {
        thisLand = landData;
        landName.text = landData.gameObject.name;
        landTier.text = LandGradeToText(landData.Data.Grade);
        isLandOwned = landData.IsOwned;
        currentBuilding = landData.CurrentBuilding;

        // Clear old panels before redrawing
        cleanupDetailsPanel();

        if (!landData.IsOwned)
        {
            double cost = GameMath.CalculateLandCost(Config, landData.Data.Grade);

            // We use the <color="green"> or <color=#HexCode> tag here
            string priceText = $"Price: <color={UIColors.MoneyGreen}>{cost}$</color>";

            CreateDetailButtonPanel(
                "Property Available",
                $"{priceText} - Tier: {landData.Data.Grade}",
                "Purchase Land",
                RaiseLandTryPurchaseEvent
            );
        }
        else
        {

            //Scenario: Land is owned but no building yet

            if (landData.CurrentBuilding == null)
            {
                double cost = GameMath.CalculateBuildingCost(Config, landData.Data.Grade);
                string priceText = $"Price: <color={UIColors.MoneyGreen}>{cost}$</color>";

                CreateDetailButtonPanel(
                "Build A Building",
                $"{priceText}",
                "Build Building",
                RaiseTryConstructBuilding
            );
            }
            // Scenario: Land is owned and has building
            if (landData.CurrentBuilding != null)
            {
                // Fetch the live data from the Service using the PlotID
                BuildingData bData = BuildingService.Instance.GetBuildingData(landData.PlotID);

                if (bData != null)
                {
                    string statsInfo = $"Level: {bData.Level} | Tenants: {bData.CurrentTenants}\n" +
                                       $"Stored: <color={UIColors.MoneyGreen}>${bData.StoredIncome:N2}</color>";

                    CreateDetailButtonPanel(
                        "Building Info",
                        statsInfo,
                        "Upgrade Building",
                        () => BuildingService.Instance.UpgradeBuilding(landData.PlotID)
                    );

                    // You could also add a Collect Rent button here
                    CreateDetailButtonPanel(
                        "Revenue",
                        "Collect accumulated rent.",
                        "Collect Cash",
                        () => BuildingService.Instance.CollectRent(landData.PlotID)
                    );
                }
            }
        }
    }

    public void RaiseLandTryPurchaseEvent()
    {
        Debug.Log("Buy Button pressed");
        EventBus<LandEvent>.Raise(new LandEvent(thisLand, LandEventType.TryPurchase));
    }

    public void RaiseTryConstructBuilding()
    {
        Debug.Log("Build Building Button Pressed");
        EventBus<LandEvent>.Raise(new LandEvent(thisLand, LandEventType.TryConstructBuilding));
    }

    public string LandGradeToText(LandGrade grade)
    {
        return grade switch
        {
            LandGrade.A => "A",
            LandGrade.B => "B",
            LandGrade.C => "C",
            _ => "D"
        };
    }

    private void OnEnable() => EventBus<LandEvent>.Subscribe(RefreshUI);
    private void OnDisable() => EventBus<LandEvent>.Unsubscribe(RefreshUI);

    private void RefreshUI(LandEvent data)
    {
        // If the land we are currently looking at was just purchased...
        if (data.Type == LandEventType.Purchased && data.Subject == thisLand)
        {
            // Re-run setup with the updated land data
            Setup(thisLand);
        }
    }



    //Detail Button Creation Helper
    void CreateDetailButtonPanel(string title, string info, string btnLabel, UnityAction action)
    {
        activeDetailButtonPanel = Instantiate(detailButtonPanelPrefab, detailButtonPanelParent);
        activeDetailButtonPanel.Setup(title, info, btnLabel, action);
    }

    void cleanupDetailsPanel()
    {
        if (activeDetailButtonPanel != null)
        {
            Destroy(activeDetailButtonPanel.gameObject);
        }
    }
}
