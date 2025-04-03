/// <summary>
/// 무기가 시설에 붙어있는 방향 열거형
/// </summary>
public enum ShipWeaponAttachedDirection
{
    /// <summary>
    /// 시설의 위쪽에 붙어있습니다.
    /// </summary>
    North,
    /// <summary>
    /// 시설의 오른쪽에 붙어있습니다.
    /// </summary>
    East,
    /// <summary>
    /// 시설의 아래쪽에 붙어있습니다.
    /// </summary>
    South
}

/*
 * 무기의 발사 방향은 무조건 그리드 절대 좌표 기준 오른쪽(x가 증가하는 방향)이다.
 * 즉, 무기가 회전한다는 것은 무기와 시설을 연결하는 힌지가 회전하는 것이다.
 */
