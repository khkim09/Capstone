using System;
using UnityEngine;

[Serializable]
public class TradableItem
{
    // JSON 데이터 셋을 불러와서 저장
    [Tooltip("행성 코드 (예: SIS, CCK 등)")] public string planet;

    [Tooltip("티어 (예: T1, T2, T3 등)")] public ItemTierLevel tier;

    [Tooltip("아이템 이름")] public string itemName;

    [Tooltip("아이템 상태")] public ItemState itemState;

    [Tooltip("분류 (예: 향신료, 소재, 보석 등)")] public ItemCategory category;

    [Tooltip("최소 보관 온도 (℃)")] public float minStorageTemperature;

    [Tooltip("최대 보관 온도 (℃)")] public float maxStorageTemperature;

    [Tooltip("기본 가격 (COMA/kg)")] public float basePrice;

    [Tooltip("최대 적층량 (kg)")] public int maxStackAmount;

    [Tooltip("가격 변동폭")] public float fluctuation;

    [Tooltip("아이템 설명")] [TextArea] public string description;

    // 한 번 계산된 최종 가격을 캐싱할 변수
    private float? cachedPrice = null;
    // 처음 호출되었는지 여부 (한 번만 초기화할지 판단)
    private bool priceInitialized = false;

    /// <summary>
    /// 현재 가격을 반환합니다.
    /// - 아직 초기화되지 않았다면 RecalculatePrice()를 한 번 호출하여
    ///   basePrice ± fluctuation 범위에서 랜덤으로 결정하고,
    ///   이후에는 동일한 값을 반환합니다.
    /// </summary>
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
    /// 10년 주기 등 외부에서 가격을 다시 갱신해야 할 때 호출합니다.
    /// basePrice ± fluctuation 범위에서 새로 랜덤 계산합니다.
    /// </summary>
    public void RecalculatePrice()
    {
        cachedPrice = UnityEngine.Random.Range(basePrice - fluctuation, basePrice + fluctuation);

        // 음수가 되지 않도록 보정
        if (cachedPrice < 0)
            cachedPrice = 0;

        Debug.Log($"[RecalculatePrice] {itemName} new price = {cachedPrice.Value}");
    }
}
