using System;
using UnityEngine;

/// <summary>
/// TradableItem 클래스는 거래 가능한 아이템의 데이터를 담고 있으며,
/// 아이템의 속성(행성 코드, 티어, 이름, 상태, 분류 등)과 가격 관련 기능을 제공합니다.
/// </summary>
[Serializable]
public class TradableItem
{
    /// <summary>
    /// 행성 코드 (예: SIS, CCK 등)
    /// </summary>
    [Tooltip("행성 코드 (예: SIS, CCK 등)")] public string planet;

    /// <summary>
    /// 티어 (예: T1, T2, T3 등)
    /// </summary>
    [Tooltip("티어 (예: T1, T2, T3 등)")] public ItemTierLevel tier;

    /// <summary>
    /// 아이템 이름
    /// </summary>
    [Tooltip("아이템 이름")] public string itemName;

    /// <summary>
    /// 아이템 상태
    /// </summary>
    [Tooltip("아이템 상태")] public ItemState itemState;

    /// <summary>
    /// 분류 (예: 향신료, 소재, 보석 등)
    /// </summary>
    [Tooltip("분류 (예: 향신료, 소재, 보석 등)")] public ItemCategory category;

    /// <summary>
    /// 최소 보관 온도 (℃)
    /// </summary>
    [Tooltip("최소 보관 온도 (℃)")] public float minStorageTemperature;

    /// <summary>
    /// 최대 보관 온도 (℃)
    /// </summary>
    [Tooltip("최대 보관 온도 (℃)")] public float maxStorageTemperature;

    /// <summary>
    /// 기본 가격 (COMA/kg)
    /// </summary>
    [Tooltip("기본 가격 (COMA/kg)")] public float basePrice;

    /// <summary>
    /// 최대 적층량 (kg)
    /// </summary>
    [Tooltip("최대 적층량 (kg)")] public int maxStackAmount;

    /// <summary>
    /// 가격 변동폭
    /// </summary>
    [Tooltip("가격 변동폭")] public float fluctuation;

    /// <summary>
    /// 아이템 설명
    /// </summary>
    [Tooltip("아이템 설명")] [TextArea] public string description;

    // 한 번 계산된 최종 가격을 캐싱할 변수
    private float? cachedPrice = null;
    // 처음 호출되었는지 여부 (한 번만 초기화할지 판단)
    private bool priceInitialized = false;

    /// <summary>
    /// 현재 가격을 반환합니다.
    /// 아직 초기화되지 않았다면 RecalculatePrice()를 호출하여 basePrice ± fluctuation 범위에서
    /// 랜덤으로 결정한 후, 동일한 값을 반환합니다.
    /// </summary>
    /// <returns>현재 계산된 가격</returns>
    public float GetCurrentPrice()
    {
        // 최초 한 번만 가격 결정
        if (!priceInitialized)
        {
            RecalculatePrice();
            priceInitialized = true;
        }
        return cachedPrice.Value;
    }

    /// <summary>
    /// 외부에서 가격을 갱신할 필요가 있을 때 호출합니다.
    /// basePrice ± fluctuation 범위 내에서 새로 랜덤으로 가격을 계산합니다.
    /// 음수가 되지 않도록 보정합니다.
    /// </summary>
    public void RecalculatePrice()
    {
        cachedPrice = UnityEngine.Random.Range(basePrice - fluctuation, basePrice + fluctuation);

        // 음수가 되지 않도록 보정
        if (cachedPrice < 0)
            cachedPrice = 0;

        Debug.Log($"[RecalculatePrice] {itemName} new price = {cachedPrice.Value}");
    }

    /// <summary>
    /// 원본 가격(BasePrice)을 반환합니다.
    /// 변동폭이나 기타 가격 계산 로직과 무관하게, JSON에 정의된 기본 가격을 그대로 반환합니다.
    /// 인벤토리 아이템의 가격을 나타낼 때 사용됩니다.
    /// </summary>
    /// <returns>아이템의 기본 가격(basePrice)</returns>
    public float GetBasePrice()
    {
        return basePrice;
    }
}
