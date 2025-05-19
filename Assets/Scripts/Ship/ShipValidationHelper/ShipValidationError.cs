/// <summary>
/// 함선 유효성 검사시 발생할 수 있는 오류 목록 열거형
/// </summary>
public enum ShipValidationError
{
    /// <summary>
    /// 유효한 배치
    /// </summary>
    None,

    /// <summary>
    /// 필수 방이 없음
    /// </summary>
    MissingRequiredRoom,

    /// <summary>
    /// 고립된 방이 있음
    /// </summary>
    DisconnectedRooms,

    /// <summary>
    /// 문 연결 오류 : 문은 복도 또는 다른 방의 문과 연결되어 있어야 함
    /// </summary>
    InvalidDoorConnection,

    /// <summary>
    /// 무기 배치 오류: 무기 앞쪽에는 방이 있으면 안 됨
    /// </summary>
    WeaponPlacementError,

    /// <summary>
    /// 무기 배치  오류 : 무기는 방과 연결되어있어야 됨
    /// </summary>
    DisconnectedWeapons,

    /// <summary>
    /// 무기 배치 오류 : 무기 앞쪽에는 아무것도 있으면 안됨
    /// </summary>
    WeaponFrontObject,

    /// <summary>
    /// 방이 없음
    /// </summary>
    NoRoom,

    /// <summary>
    /// 기타 오류
    /// </summary>
    Other
}
