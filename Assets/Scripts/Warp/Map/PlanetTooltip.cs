using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlanetTooltip : MonoBehaviour
{
    [Header("UI References")] [SerializeField]
    private TextMeshProUGUI planetNameText;

    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI eventInfoText;
    [SerializeField] private TextMeshProUGUI questInfoText;
    [SerializeField] private TextMeshProUGUI fuelPriceText;

    [Header("Optional Elements")] [SerializeField]
    private GameObject eventInfoContainer;

    [SerializeField] private GameObject questInfoContainer;

    // Setup the tooltip with planet data
    public void SetupTooltip(Planet planet)
    {
        // Set basic info
        if (planetNameText) planetNameText.text = planet.planetName;
        if (distanceText) distanceText.text = $"Distance: {planet.distanceInLightYears:F1} light years";
        if (fuelPriceText) fuelPriceText.text = $"Fuel Price: {planet.fuelPrice:F1} credits";

        // Set optional info and show/hide containers
        if (eventInfoContainer && eventInfoText)
        {
            eventInfoContainer.SetActive(planet.hasEvent);
            if (planet.hasEvent) eventInfoText.text = "Special event available!";
        }

        if (questInfoContainer && questInfoText)
        {
            questInfoContainer.SetActive(planet.hasQuest);
            if (planet.hasQuest) questInfoText.text = "Quest available";
        }

        // Force layout refresh
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
}
