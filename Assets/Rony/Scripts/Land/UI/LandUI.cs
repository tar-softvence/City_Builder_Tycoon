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
                () => GameActions.RequestPurchaseLand(thisLand)
            );
        }
        else
        {

            // Scenario A: Land is owned, but NO building exists
            if (currentBuilding == null)
            {
                double cost = GameMath.CalculateBuildingCost(Config, landData.Data.Grade);
                CreateDetailButtonPanel(
                    "Build A Building",
                    $"Price: <color={UIColors.MoneyGreen}>{cost}$</color>",
                    "Build Building",
                    () => GameActions.RequestConstructBuilding(thisLand)
                );
            }
            // Scenario B: Land is owned AND building exists

            else
            {
                BuildingData bData = BuildingService.Instance.GetBuildingData(landData.PlotID);

                if (bData != null)
                {
                    // --- STATISTICS PANEL ---
                    string statsInfo = $"Level: {bData.Level} | Tenants: {bData.CurrentTenants}\n" +
                                       $"Stored: <color={UIColors.MoneyGreen}>${bData.StoredIncome:N2}</color>";

                    // --- REVENUE BUTTON ---
                    CreateDetailButtonPanel(
                        "Revenue",
                        "Collect accumulated rent.",
                        "Collect Cash",
                        () => BuildingService.Instance.CollectRent(landData.PlotID)
                    );

                    // --- UPGRADE BUTTON (Check Max Level) ---
                    if (!IsMaxLevel(landData, bData.Level))
                    {
                        double upgradeCost = GameMath.CalculateUpgradeCost(Config, bData.Level);

                        CreateDetailButtonPanel(
                           "Building Info",
                           $"{statsInfo}\nUpgrade Cost: <color={UIColors.MoneyGreen}>{upgradeCost}$</color>",
                           "Upgrade Building",
                           () => GameActions.RequestUpgradeBuilding(thisLand.CurrentBuilding)
                       );
                    }
                    else
                    {
                        // Optional: Show "Max Level" text instead of a button
                        // CreateDetailButtonPanel("Building Info", statsInfo + "\n<color=orange>MAX LEVEL</color>", "", null);
                    }
                }

            }
        }
    }



    // Helper to check array bounds in the SO
    private bool IsMaxLevel(Land land, int currentLevel)
    {
        // Assuming Level 1 = Index 0, Level 2 = Index 1
        // If currentLevel is 1, next index is 1. If Count is 2, we are fine.
        // If currentLevel is 2 (Index 1), next index is 2. If Count is 2, we are MAX.
        return currentLevel >= land.Data.BuildingLevels.Count;
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

    private void OnEnable()
    {
        EventBus<LandEvent>.Subscribe(HandleLandEvent);
        EventBus<BuildingEvent>.Subscribe(HandleBuildingEvent);
    }

    private void OnDisable()
    {
        EventBus<LandEvent>.Unsubscribe(HandleLandEvent);
        EventBus<BuildingEvent>.Unsubscribe(HandleBuildingEvent);
    }

    private void HandleLandEvent(LandEvent data)
    {
        switch (data.Type)
        {
            case LandEventType.Selected:
                this.gameObject.SetActive(true); // Show the UI
                Setup(data.Subject);
                break;

            case LandEventType.Deselected:
                this.gameObject.SetActive(false); // Hide the UI
                break;

            case LandEventType.Purchased:
            case LandEventType.BuildingConstructed: // 
                if (thisLand != null && data.Subject == thisLand)
                {
                    // We re-run Setup. Since we updated view.ParentLand.SetBuilding 
                    // in the service, thisLand.CurrentBuilding is no longer null.
                    Setup(thisLand);
                }
                break;
        }
    }

    private void HandleBuildingEvent(BuildingEvent data)
    {
        switch (data.Type)
        {
            case BuildingEventType.Upgraded:
                if (thisLand != null && data.SubjectView.ParentLand == thisLand)
                {
                    // We re-run Setup. Since we updated view.ParentLand.SetBuilding 
                    // in the service, thisLand.CurrentBuilding is no longer null.
                    Setup(thisLand);
                }
                break;
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
        foreach (Transform child in detailButtonPanelParent)
        {
            Destroy(child.gameObject);
        }
    }
}
