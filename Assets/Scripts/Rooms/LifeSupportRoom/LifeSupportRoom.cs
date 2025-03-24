using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선의 생활 편의 시설을 나타내는 클래스
/// </summary>
public class LifeSupportRoom : Room<LifeSupportRoomData, LifeSupportRoomData.LifeSupportRoomLevel>
{
    protected override void Start()
    {
        base.Start();

        // 방 타입 설정
        roomType = RoomType.LifeSupport;
    }

    /// <summary>
    /// 이 방의 스탯 기여도 계산
    /// </summary>
    public override Dictionary<ShipStat, float> GetStatContributions()
    {
        // 기본 기여도 가져오기 (작동 상태 체크 등)
        Dictionary<ShipStat, float> contributions = base.GetStatContributions();

        // 작동 상태가 아니면 기여도 없음
        if (!IsOperational() || currentRoomLevelData == null)
            return contributions;

        contributions[ShipStat.PowerUsing] = currentRoomLevelData.powerRequirement;
        contributions[ShipStat.CrewCapacity] = currentRoomLevelData.crewMoraleBonus;

        return contributions;
    }
}
