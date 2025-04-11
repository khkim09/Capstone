using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 의무실의 레벨별 데이터를 저장하는 ScriptableObject.
/// 회복량(초당 힐량)을 포함한 의무실 스탯을 정의합니다.
/// </summary>
[CreateAssetMenu(fileName = "MedBayRoomData", menuName = "RoomData/MedBayRoom Data")]
public class MedBayRoomData : RoomData<MedBayRoomData.MedBayRoomLevel>
{
    /// <summary>
    /// 의무실의 레벨별 데이터 구조.
    /// </summary>
    [System.Serializable]
    public class MedBayRoomLevel : RoomLevel
    {
        /// <summary>초당 체력 회복량.</summary>
        public float healPerSecond;
    }

    /// <summary>
    /// 기본 의무실 레벨 데이터를 초기화합니다.
    /// </summary>
    protected override void InitializeDefaultLevels()
    {
        RoomLevels = new List<MedBayRoomLevel>
        {
            new()
            {
                roomName = "room.medbay.level1",
                roomType = RoomType.MedBay,
                category = RoomCategory.Auxiliary,
                level = 1,
                hitPoint = 100,
                size = new Vector2Int(2, 2),
                cost = 2000,
                powerRequirement = 10f,
                crewRequirement = 0,
                healPerSecond = 5f,
                damageHitPointRate = RoomDamageRates.Create(60f, 30f),
                possibleDoorPositions = new List<DoorPosition>() { new(new Vector2Int(1, 0), DoorDirection.East) }
            },
            new()
            {
                roomName = "room.medbay.level2",
                roomType = RoomType.MedBay,
                category = RoomCategory.Auxiliary,
                level = 2,
                hitPoint = 120,
                size = new Vector2Int(3, 2),
                cost = 4000,
                powerRequirement = 20f,
                crewRequirement = 0,
                healPerSecond = 8f,
                damageHitPointRate = RoomDamageRates.Create(60f, 30f),
                possibleDoorPositions = new List<DoorPosition>() { new(new Vector2Int(0, 0), DoorDirection.West) }
            }
        };
    }
}
