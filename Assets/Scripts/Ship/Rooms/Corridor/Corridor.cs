using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 복도(RoomType.Corridor) 정의
/// </summary>
public class CorridorRoom : Room<CorridorRoomData, CorridorRoomData.CorridorRoomLevel>
{
    /// <summary>
    /// 초기화 시 방 타입을 복도로 설정정
    /// </summary>
    protected override void Start()
    {
        base.Start();
        roomType = RoomType.Corridor;
        workDirection=Vector2Int.zero;
    }

    /// <summary>
    /// 속도 10% 증가
    /// </summary>
    public void OnCrewEnter()
    {
        foreach (CrewMember crew in crewInRoom)
            crew.moveSpeed *= 1.1f;
    }

    /// <summary>
    /// 속도 원 상태로 복구
    /// </summary>
    public void OnCrewExit()
    {
        foreach (CrewMember crew in crewInRoom)
            crew.moveSpeed /= 1.1f;
    }

    /// <summary>
    /// 복도가 함선 스탯에 기여하는 값을 계산
    /// </summary>
    /// <returns>스탯 기여도 딕셔너리</returns>
    public override Dictionary<ShipStat, float> GetStatContributions()
    {
        return new Dictionary<ShipStat, float>(); // 복도는 기여 없음
    }
}
