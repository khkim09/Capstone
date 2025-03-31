/// <summary>
/// 방 내부의 산소 농도 상태를 나타내는 값입니다.
/// </summary>
public enum OxygenLevel
{
    /// <summary>산소가 전혀 없는 진공 상태. 즉시 생존 불가. (0%)</summary>
    None = 0,

    /// <summary>산소가 매우 부족하여 치명적인 상태. (20% 이상)</summary>
    Critical = 1,

    /// <summary>산소가 부족하지만 즉각적인 위험은 없는 상태. (40% 이상)</summary>
    Low = 2,

    /// <summary>산소가 절반 이상 있는 중간 상태. (60% 이상)</summary>
    Medium = 3,

    /// <summary>정상적인 산소 상태. (80% 이상)</summary>
    Normal = 4
}
