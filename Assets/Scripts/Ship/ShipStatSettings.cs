using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ship 스탯의 최소값과 최대값을 관리하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "ShipStatSettings", menuName = "Ship/Stat Settings")]
public class ShipStatSettings : ScriptableObject
{
    [System.Serializable]
    public class StatLimit
    {
        public ShipStat Stat;
        public float MinValue; // 최소값 추가
        public float MaxValue;
        public bool HasMinLimit; // 최소값 제한 여부
        [TextArea(1, 3)] public string Description; // 스탯에 대한 설명이나 메모
    }

    [Tooltip("각 스탯에 대한 최소/최대값 설정")] public List<StatLimit> StatLimits = new();

    // 기본값 설정 (인스펙터에서 설정하지 않은 경우 사용됨)
    [SerializeField] private float defaultMaxValue = float.MaxValue;
    [SerializeField] private float defaultMinValue = 0f;

    // 캐싱을 위한 사전
    private Dictionary<ShipStat, StatLimit> _limitsCache;

    // Unity 이벤트: 스크립트가 로드될 때 호출됨
    private void OnEnable()
    {
        // 값이 비어있으면 기본값으로 초기화
        if (StatLimits == null || StatLimits.Count == 0) InitializeDefaultValues();

        InitializeCache();
    }

    // 캐시 초기화
    private void InitializeCache()
    {
        _limitsCache = new Dictionary<ShipStat, StatLimit>();
        foreach (StatLimit limit in StatLimits) _limitsCache[limit.Stat] = limit;
    }

    // 기본값으로 초기화
    private void InitializeDefaultValues()
    {
        StatLimits = new List<StatLimit>
        {
            new()
            {
                Stat = ShipStat.DodgeChance,
                MinValue = 0f,
                MaxValue = 70f,
                HasMinLimit = false,
                Description = "함선의 회피율 (%)"
            },
            new()
            {
                Stat = ShipStat.HitPointsMax,
                MinValue = 100f,
                MaxValue = 10000f,
                HasMinLimit = false,
                Description = "함선의 체력"
            },
            new()
            {
                Stat = ShipStat.FuelEfficiency,
                MinValue = 0f,
                MaxValue = 30f,
                HasMinLimit = false,
                Description = "연료 효율 (%)"
            },
            new()
            {
                Stat = ShipStat.FuelConsumption,
                MinValue = 5f,
                MaxValue = 10f,
                HasMinLimit = true,
                Description = "워프당 연료 소모량"
            },
            new()
            {
                Stat = ShipStat.ShieldMaxAmount,
                MinValue = 0f,
                MaxValue = 250,
                HasMinLimit = false,
                Description = "방어막 용량"
            },
            new()
            {
                Stat = ShipStat.OxygenGeneratePerSecond,
                MinValue = 0f,
                MaxValue = 100f,
                HasMinLimit = false,
                Description = "초당 산소 생산율 (%)"
            },
            new()
            {
                Stat = ShipStat.OxygenUsingPerSecond,
                MinValue = 0f,
                MaxValue = 100f,
                HasMinLimit = false,
                Description = "초당 산소 소비율 (%)"
            },
            new()
            {
                Stat = ShipStat.PowerUsing,
                MinValue = 0f,
                MaxValue = float.MaxValue,
                HasMinLimit = false,
                Description = "전력 사용량"
            },
            new()
            {
                Stat = ShipStat.PowerCapacity,
                MinValue = 0f,
                MaxValue = float.MaxValue,
                HasMinLimit = false,
                Description = "전력 생산량"
            },
            new()
            {
                Stat = ShipStat.ShieldRespawnTime,
                MinValue = 12f,
                MaxValue = float.MaxValue,
                HasMinLimit = true,
                Description = "방어막 재생성 시간 (초, 최소 5초)"
            },
            new()
            {
                Stat = ShipStat.ShieldRegeneratePerSecond,
                MinValue = 0f,
                MaxValue = 250f,
                HasMinLimit = false,
                Description = "초당 방어막 재생량"
            },
            new()
            {
                Stat = ShipStat.CrewCapacity,
                MinValue = 0f,
                MaxValue = float.MaxValue,
                HasMinLimit = false,
                Description = "선원 고용 수"
            },
            new()
            {
                Stat = ShipStat.DamageReduction,
                MinValue = 0f,
                MaxValue = 10f,
                HasMinLimit = false,
                Description = "데미지 감소율 (%)"
            },
            new()
            {
                Stat = ShipStat.Accuracy,
                MinValue = 0f,
                MaxValue = 150f,
                HasMinLimit = false,
                Description = "함선 무기 명중률 (%)"
            },
            new()
            {
                Stat = ShipStat.ReloadTimeBonus,
                MinValue = 0f,
                MaxValue = 150f,
                HasMinLimit = false,
                Description = "재장전 시간 보너스 (%)"
            },
            new()
            {
                Stat = ShipStat.DamageBonus,
                MinValue = 0f,
                MaxValue = 150f,
                HasMinLimit = false,
                Description = "데미지 보너스 (%)"
            },
            new()
            {
                Stat = ShipStat.CrewMoraleBonus,
                MinValue = 0f,
                MaxValue = float.MaxValue,
                HasMinLimit = false,
                Description = "선원 사기 보너스 (최소 -10)"
            }
        };
    }

    /// <summary>
    /// 특정 스탯의 최대값을 반환합니다.
    /// </summary>
    public float GetMaxValue(ShipStat stat)
    {
        if (_limitsCache == null) InitializeCache();

        if (_limitsCache.TryGetValue(stat, out StatLimit limit)) return limit.MaxValue;

        return defaultMaxValue;
    }

    /// <summary>
    /// 특정 스탯의 최소값을 반환합니다.
    /// </summary>
    public float GetMinValue(ShipStat stat)
    {
        if (_limitsCache == null) InitializeCache();

        if (_limitsCache.TryGetValue(stat, out StatLimit limit) && limit.HasMinLimit) return limit.MinValue;

        return defaultMinValue;
    }

    /// <summary>
    /// 특정 스탯이 최소값 제한이 있는지 확인합니다.
    /// </summary>
    public bool HasMinLimit(ShipStat stat)
    {
        if (_limitsCache == null) InitializeCache();

        if (_limitsCache.TryGetValue(stat, out StatLimit limit)) return limit.HasMinLimit;

        return false;
    }

    /// <summary>
    /// 주어진 스탯 값이 최소값과 최대값 사이에 있도록 제한합니다.
    /// </summary>
    public float ClampValue(ShipStat stat, float value)
    {
        float max = GetMaxValue(stat);

        if (HasMinLimit(stat))
        {
            float min = GetMinValue(stat);
            return Mathf.Clamp(value, min, max);
        }

        return Mathf.Min(value, max);
    }
}
