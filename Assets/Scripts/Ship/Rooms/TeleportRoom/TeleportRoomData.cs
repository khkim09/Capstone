using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 텔레포트실의 레벨별 데이터를 저장하는 ScriptableObject.
/// 전력 소비와 텔레포트 딜레이 관련 스탯을 정의합니다.
/// </summary>
[CreateAssetMenu(fileName = "TeleportRoom", menuName = "RoomData/TeleportRoom Data")]
public class TeleportRoomData : RoomData<TeleportRoomData.TeleportRoomLevel>
{
    /// <summary>
    /// 텔레포트실의 레벨별 데이터 구조.
    /// </summary>
    [System.Serializable]
    public class TeleportRoomLevel : RoomLevel
    {
        /// <summary>텔레포트 시전 후 발동까지의 대기 시간 (초).</summary>
        public float teleportWaitingTime = 0.5f; // 이동 시전 후 딜레이

        /// <summary>텔레포트 재사용 대기 시간 (초).</summary>
        public float teleportRespawnTime = 1.0f; // 재소환 딜레이
    }
    // TODO: 텔레포터 관련 스탯 더 세부적으로 정해야할 것

    /// <summary>
    /// 기본 텔레포트실 레벨 데이터를 초기화합니다.
    /// </summary>
    protected override void InitializeDefaultLevels()
    {
        RoomLevels = new List<TeleportRoomLevel>
        {
            new()
            {
                roomName = "room.teleport.level1",
                roomType = RoomType.Teleporter,
                category = RoomCategory.Essential,
                level = 1,
                hitPoint = 100,
                size = new Vector2Int(1, 1),
                cost = 700,
                powerRequirement = 20f,
                crewRequirement = 0,
                damageHitPointRate = RoomDamageRates.Create(40f, 0f),
                possibleDoorPositions =
                    new List<DoorPosition>() { new(new Vector2Int(0, 0), DoorDirection.East) },
                crewEntryGridPriority = new List<Vector2Int>() { new(0, 0) }
            }
        };
    }
}
