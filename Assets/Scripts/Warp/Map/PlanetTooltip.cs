using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 행성의 정보를 UI 툴팁 형태로 표시하는 컴포넌트입니다.
/// </summary>
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

    /// <summary>
    /// 전달받은 행성 데이터를 바탕으로 툴팁을 설정합니다.
    /// </summary>
    /// <param name="planet">표시할 정보를 가진 Planet 객체입니다.</param>
    public void SetupTooltip(Planet planet)
    {
        // Set basic info
        if (planetNameText) planetNameText.text = planet.planetData.planetName;
        if (distanceText) distanceText.text = $"Distance: {planet.planetData.distance:F1} light years";
        if (fuelPriceText) fuelPriceText.text = $"Fuel Price: {planet.planetData.currentFuelPrice:F1} credits";

        // Set optional info and show/hide containers
        if (eventInfoContainer && eventInfoText)
        {
            eventInfoContainer.SetActive(planet.HasEvent);
            if (planet.HasEvent) eventInfoText.text = "Special event available!";
        }

        if (questInfoContainer && questInfoText)
        {
            questInfoContainer.SetActive(planet.HasQuest);
            if (planet.HasQuest) questInfoText.text = "Quest available";
        }

        // Force layout refresh
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
}
