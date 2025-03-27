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

    /// <summary>
    /// 최초 호출 시, basePrice ± fluctuation 범위 내에서 랜덤으로 가격을 결정하고,
    /// 이후에는 그 값을 재사용합니다.
    /// </summary>
    /// <returns>고정된 최종 가격</returns>
    public float GetCurrentPrice()
    {
        if (cachedPrice.HasValue)
            return cachedPrice.Value;

        // 한 번만 계산해서 캐싱:
        cachedPrice = UnityEngine.Random.Range(basePrice - fluctuation, basePrice + fluctuation);
        return cachedPrice.Value;
    }
}
