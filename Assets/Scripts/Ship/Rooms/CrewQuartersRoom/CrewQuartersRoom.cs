using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선의 승무원 선실(CrewQuarters)을 나타내는 클래스.
/// 선원 수용량에 기여하며, 전력을 소비합니다.
/// </summary>
public class CrewQuartersRoom : Room<CrewQuartersRoomData, CrewQuartersRoomData.CrewQuartersRoomLevel>
{
    /// <summary>
    /// 이 방이 개인 전용 방인지 여부.
    /// </summary>
    public bool isPersonalRoom;

    /// <summary>
    /// 초기화 시 방 타입을 CrewQuarters로 설정합니다.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // 방 타입 설정
        roomType = RoomType.CrewQuarters;
        workDirection=Vector2Int.zero;
    }

    /// <summary>
    /// 초기화 시 방 타입을 CrewQuarters로 설정합니다.
    /// </summary>
    public override Dictionary<ShipStat, float> GetStatContributions()
    {
        // 기본 기여도 가져오기 (작동 상태 체크 등)
        Dictionary<ShipStat, float> contributions = base.GetStatContributions();
        contributions[ShipStat.CrewCapacity] = 0;
        // 작동 상태가 아니면 기여도 없음
        if (!IsOperational() || currentRoomLevelData == null)
            return contributions;

        //contributions[ShipStat.PowerUsing] = currentRoomLevelData.powerRequirement;
        contributions[ShipStat.CrewCapacity] = currentRoomLevelData.maxCrewCapacity;

        return contributions;
    }

    /// <summary>
    /// 이 방이 개인 전용 방인지 여부를 반환합니다.
    /// </summary>
    /// <returns>개인 전용 방일 경우 true.</returns>
    public bool GetIsPersonalRoom()
    {
        return isPersonalRoom;
    }
}
