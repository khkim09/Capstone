using System;
using UnityEngine;

[Serializable]
public class TradableItem
{
    // JSON 데이터 셋을 불러와서 저장
    [Tooltip("행성 코드 (예: SIS, CCK 등)")] public string planet;

    [Tooltip("티어 (예: T1, T2, T3 등)")] public ItemTier tier;

    [Tooltip("아이템 이름")] public string itemName;

    [Tooltip("아이템 상태")] public ItemState itemState;

    [Tooltip("분류 (예: 향신료, 소재, 보석 등)")] public ItemType category;

    [Tooltip("최소 보관 온도 (℃)")] public float minStorageTemperature;

    [Tooltip("최대 보관 온도 (℃)")] public float maxStorageTemperature;

    [Tooltip("기본 가격 (COMA/kg)")] public float basePrice;

    [Tooltip("최대 적층량 (kg)")] public int maxStackAmount;

    [Tooltip("가격 변동폭")] public float fluctuation;

    [Tooltip("아이템 설명")] [TextArea] public string description;

    /// <summary>
    /// /// 현재 가격을 계산합니다.
    /// 기본 가격에서 변동폭 만큼 랜덤하게 + 또는 - 된 값을 반환합니다.
    /// </summary>
    /// <returns>변동 적용된 가격</returns>
    public float GetCurrentPrice()
    {
        return UnityEngine.Random.Range(basePrice - fluctuation, basePrice + fluctuation);
    }
}
