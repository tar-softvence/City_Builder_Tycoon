using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class DetailButtonPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text headingText;
    [SerializeField] private TMP_Text detailsText;
    [SerializeField] private TMP_Text buttonLabel;
    [SerializeField] private Button actionButton;

    public void Setup(string heading, string details, string btnText, UnityAction onButtonClick)
    {
        headingText.text = heading;
        detailsText.text = details;
        buttonLabel.text = btnText;

        //clear previous litsener to prevent double firing if reused
        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(onButtonClick);
    }
}
