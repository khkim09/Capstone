using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선의 생활 편의 시설(RoomType.LifeSupport)을 나타내는 클래스.
/// 선원 사기 보너스를 제공하며, 전력을 소비합니다.
/// </summary>
public class LifeSupportRoom : Room<LifeSupportRoomData, LifeSupportRoomData.LifeSupportRoomLevel>
{
    /// <summary>
    /// 초기화 시 방 타입을 LifeSupport로 설정합니다.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // 방 타입 설정
        roomType = RoomType.LifeSupport;
        workDirection = Vector2Int.zero;
    }

    /// <summary>
    /// 이 방이 함선 스탯에 기여하는 값을 계산합니다.
    /// 전력 사용량과 선원 사기 보너스를 포함합니다.
    /// </summary>
    /// <returns>스탯 기여도 딕셔너리.</returns>
    public override Dictionary<ShipStat, float> GetStatContributions()
    {
        // 기본 기여도 가져오기 (작동 상태 체크 등)
        Dictionary<ShipStat, float> contributions = base.GetStatContributions();

        // 작동 상태가 아니면 기여도 없음
        if (!IsOperational() || currentRoomLevelData == null)
            return contributions;

        contributions[ShipStat.PowerUsing] = currentRoomLevelData.powerRequirement;
        contributions[ShipStat.CrewMoraleBonus] = currentRoomLevelData.crewMoraleBonus;

        return contributions;
    }
}
