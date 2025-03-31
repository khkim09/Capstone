using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ship 스탯의 최소값과 최대값을 관리하는 ScriptableObject.
/// 밸런스 조정 및 수치 안정성을 위한 범위 제한 기능을 제공합니다.
/// </summary>
[CreateAssetMenu(fileName = "ShipStatSettings", menuName = "Ship/Stat Settings")]
public class ShipStatSettings : ScriptableObject
{
    /// <summary>
    /// 특정 ShipStat에 대한 제한 설정입니다.
    /// </summary>
    [System.Serializable]
    public class StatLimit
    {
        /// <summary>대상 스탯.</summary>
        public ShipStat Stat;

        /// <summary>허용되는 최소값.</summary>
        public float MinValue;

        /// <summary>허용되는 최대값.</summary>
        public float MaxValue;

        /// <summary>최소값 제한 여부.</summary>
        public bool HasMinLimit;

        /// <summary>해당 스탯에 대한 설명 또는 메모.</summary>
        [TextArea(1, 3)] public string Description;
    }

    /// <summary>해당 스탯에 대한 설명 또는 메모.</summary>
    [Tooltip("각 스탯에 대한 최소/최대값 설정")] public List<StatLimit> StatLimits = new();


    /// <summary>
    /// 설정되지 않은 스탯에 대한 기본 최대값.
    /// </summary>
    [SerializeField] private float defaultMaxValue = float.MaxValue;

    /// <summary>
    /// 설정되지 않은 스탯에 대한 기본 최소값.
    /// </summary>
    [SerializeField] private float defaultMinValue = 0f;

    /// <summary>
    /// 빠른 조회를 위한 내부 캐시 딕셔너리.
    /// </summary>
    private Dictionary<ShipStat, StatLimit> _limitsCache;

    /// <summary>
    /// ScriptableObject가 활성화될 때 자동으로 호출됩니다.
    /// 기본값과 캐시 초기화를 수행합니다.
    /// </summary>
    private void OnEnable()
    {
        // 값이 비어있으면 기본값으로 초기화
        if (StatLimits == null || StatLimits.Count == 0) InitializeDefaultValues();

        InitializeCache();
    }

    /// <summary>
    /// 내부 캐시를 초기화합니다.
    /// </summary>
    private void InitializeCache()
    {
        _limitsCache = new Dictionary<ShipStat, StatLimit>();
        foreach (StatLimit limit in StatLimits) _limitsCache[limit.Stat] = limit;
    }

    /// <summary>
    /// 기본 스탯 제한 값들을 설정합니다.
    /// StatLimits가 비어 있는 경우에만 호출됩니다.
    /// </summary>
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
    /// <param name="stat">조회할 스탯.</param>
    /// <returns>최대값.</returns>
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
