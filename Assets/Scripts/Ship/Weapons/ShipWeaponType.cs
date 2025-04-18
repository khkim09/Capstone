/// <summary>
/// 무기의 타입을 정의하는 열거형.
/// 무기 종류에 따라 피해 방식이나 특성이 달라질 수 있습니다.
/// </summary>
public enum ShipWeaponType
{
    /// <summary>
    /// 레이저 무기.
    /// </summary>
    Laser,

    /// <summary>
    /// 미사일 무기.
    /// </summary>
    Missile,

    /// <summary>
    /// 레일건 무기.
    /// </summary>
    Railgun,

    /// <summary>
    /// 기본값
    /// </summary>
    Default
}
