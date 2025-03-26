using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선의 승무원 선실을 나타내는 클래스
/// </summary>
public abstract class CrewQuartersRoom : Room<CrewQuartersRoomData, CrewQuartersRoomData.CrewQuartersRoomLevel>
{
    public bool isPersonalRoom;

    protected override void Start()
    {
        base.Start();

        // 방 타입 설정
        roomType = RoomType.CrewQuarters;
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
        contributions[ShipStat.CrewCapacity] = currentRoomLevelData.maxCrewCapacity;

        return contributions;
    }

    /// <summary>
    /// 이 방이 개인 전용 방인지 여부를 반환
    /// </summary>
    /// <returns>개인 전용 방이라면 true, 그렇지 않다면 false</returns>
    public abstract bool GetIsPersonalRoom();
}
