using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 산소실의 레벨별 데이터를 저장하는 ScriptableObject.
/// 레벨에 따라 산소 공급량과 전력 소비량 등이 달라집니다.
/// </summary>
[CreateAssetMenu(fileName = "OxygenRoomData", menuName = "RoomData/OxygenRoom Data")]
public class OxygenRoomData : RoomData<OxygenRoomData.OxygenLevel>
{
    /// <summary>
    /// 산소실의 레벨별 데이터 구조.
    /// </summary>
    [System.Serializable]
    public class OxygenLevel : RoomLevel
    {
        /// <summary>초당 산소 공급량.</summary>
        public float oxygenSupplyPerSecond; // 초당 산소 공급량
    }


    /// <summary>
    /// 기본 산소실 레벨 데이터를 초기화합니다.
    /// </summary>
    protected override void InitializeDefaultLevels()
    {
        RoomLevels = new List<OxygenLevel>
        {
            new()
            {
                roomName = "room.oxygen.level1",
                roomType = RoomType.Oxygen,
                category = RoomCategory.Auxiliary,
                level = 1,
                hitPoint = 80,
                size = new Vector2Int(2, 1),
                cost = 500,
                powerRequirement = 10f,
                crewRequirement = 0,
                damageHitPointRate = RoomDamageRates.Create(80f, 30f),
                possibleDoorPositions = new List<DoorPosition>() { new(new Vector2Int(1, 0), DoorDirection.East) }
            },
            new()
            {
                roomName = "room.oxygen.level2",
                roomType = RoomType.Oxygen,
                category = RoomCategory.Auxiliary,
                level = 2,
                hitPoint = 160,
                size = new Vector2Int(2, 1),
                cost = 1500,
                powerRequirement = 15f,
                crewRequirement = 0,
                oxygenSupplyPerSecond = 8f,
                damageHitPointRate = RoomDamageRates.Create(80f, 30f),
                possibleDoorPositions = new List<DoorPosition>() { new(new Vector2Int(1, 0), DoorDirection.East) }
            }
        };
    }
}
