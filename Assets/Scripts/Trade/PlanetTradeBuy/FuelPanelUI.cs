using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 연료 거래 UI를 관리하는 스크립트입니다.
/// </summary>
public class FuelPanelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fuelPriceText;     // 연료 가격 표시 텍스트
    [SerializeField] private TextMeshProUGUI fuelValueText;     // 현재 연료량 표시 텍스트
    [SerializeField] private Button buyButton;           // 구매 버튼

    private int basePricePer100 = 100;                   // 기준 연료 가격 (100당)

    private int fluctuatedPrice;                         // 변동된 연료 가격

    private float fluctuationRange = 0.5f;               // 변동폭 ±50%

    public float MaxFuelAmount = 1000f;                  // 최대 연료량 (임시 로컬 변수)

    /// <summary>마지막 가격 업데이트 된 연도입니다.</summary>
    private int lastPriceUpdateYear = -1;

    /// <summary>가격 업데이트 주기입니다.</summary>
    private int updateCycle = 5;

    /// <summary>
    /// 시작할때 실행되는 함수입니다.
    /// </summary>
    private void Start()
    {
        lastPriceUpdateYear = GameManager.Instance.CurrentYear;
        ApplyPriceFluctuation();
        UpdateFuelDisplay();

        buyButton.onClick.AddListener(BuyFuel);
    }

    /// <summary>
    /// 업데이트 함수입니다. 가격 변동을 업데이트 합니다.
    /// </summary>
    private void Update()
    {
        int currentYear = GameManager.Instance.CurrentYear;

        if (currentYear >= lastPriceUpdateYear + updateCycle)
        {
            ApplyPriceFluctuation();
            lastPriceUpdateYear = currentYear;
        }
    }


    /// <summary>
    /// 연료 가격에 변동폭을 적용합니다.
    /// </summary>
    private void ApplyPriceFluctuation()
    {
        float minMultiplier = 1f - fluctuationRange; // 0.5
        float maxMultiplier = 1f + fluctuationRange; // 1.5

        float multiplier = Random.Range(minMultiplier, maxMultiplier);
        fluctuatedPrice = Mathf.RoundToInt(basePricePer100 * multiplier);

        fuelPriceText.text = $"<size=80%>{fluctuatedPrice}</size>";
    }

    /// <summary>
    /// 현재 연료량을 텍스트로 표시합니다.
    /// </summary>
    private void UpdateFuelDisplay()
    {
        float currentFuel = ResourceManager.Instance.Fuel;
        fuelValueText.text = $"{currentFuel} / {MaxFuelAmount}";
    }

    /// <summary>
    /// 연료를 구매하는 로직입니다.
    /// </summary>
    private void BuyFuel()
    {
        float currentFuel = ResourceManager.Instance.Fuel;

        if (currentFuel >= MaxFuelAmount)
        {
            Debug.Log("이미 최대 연료입니다.");
            return;
        }

        int currentCOMA = ResourceManager.Instance.COMA;

        if (currentCOMA < fluctuatedPrice)
        {
            Debug.Log("COMA가 부족합니다.");
            return;
        }

        float newFuel = Mathf.Min(currentFuel + 100, MaxFuelAmount);
        float amountToAdd = newFuel - currentFuel;

        ResourceManager.Instance.ChangeResource(ResourceType.Fuel, amountToAdd);
        ResourceManager.Instance.ChangeResource(ResourceType.COMA, -fluctuatedPrice);

        UpdateFuelDisplay();
    }
}
