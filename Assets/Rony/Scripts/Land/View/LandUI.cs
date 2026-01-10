using TMPro;
using UnityEngine;
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

    Land thisLand;

    bool isLandOwned;
    Building currentBuilding = null;


    public void Setup(Land landData)
    {
        landName.text = landData.gameObject.name;
        landTier.text = LandGradeToText(landData.Data.Grade);
        isLandOwned = landData.IsOwned;
        thisLand = landData;
        currentBuilding = landData.CurrentBuilding;

        if (!isLandOwned)
        {
            buyButtonParentPanel.gameObject.SetActive(true);
            BuyButton.onClick.AddListener(RaiseLandTryPurchaseEvent);
        }

        if (isLandOwned)
        {
            buyButtonParentPanel.gameObject.SetActive(false);
            if (currentBuilding != null)
            {
                level.text = landData.CurrentBuilding.Level.ToString();
                currentTanents.text = landData.CurrentBuilding.currentTenants.ToString();
            }

        }
    }

    public void RaiseLandTryPurchaseEvent()
    {
        Debug.Log("Buy Button pressed");
        EventBus<LandEvent>.Raise(new LandEvent(thisLand, LandEventType.TryPurchase));
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
}
