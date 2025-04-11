using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 복도(RoomType.Corridor) 정의
/// </summary>
public class CorridorRoom : Room<CorridorRoomData, CorridorRoomData.CorridorRoomLevel>
{
    protected override void Start()
    {
        base.Start();
        roomType = RoomType.Corridor;
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

    public override Dictionary<ShipStat, float> GetStatContributions()
    {
        return new Dictionary<ShipStat, float>(); // 복도는 기여 없음
    }
}
