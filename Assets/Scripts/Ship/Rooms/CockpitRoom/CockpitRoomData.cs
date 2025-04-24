using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 조종실의 레벨별 데이터를 저장하는 ScriptableObject.
/// 각 레벨마다 회피 효율, 연료 효율 등의 수치를 정의합니다.
/// </summary>
[CreateAssetMenu(fileName = "CockpitRoomData", menuName = "RoomData/CockpitRoom Data")]
public class CockpitRoomData : RoomData<CockpitRoomData.CockpitRoomLevel>
{
    /// <summary>
    /// 조종실의 레벨별 데이터 구조.
    /// </summary>
    [System.Serializable]
    public class CockpitRoomLevel : RoomLevel
    {
        /// <summary>연료 효율 보너스 (%).</summary>
        public float fuelEfficiency;

        /// <summary>회피 확률 보너스 (%).</summary>
        public float avoidEfficiency;
    }

    /// <summary>
    /// 기본 조종실 레벨 데이터를 초기화합니다.
    /// </summary>
    protected override void InitializeDefaultLevels()
    {
        RoomLevels = new List<CockpitRoomLevel>
        {
            new()
            {
                roomName = "room.cockpit.level1",
                roomType = RoomType.Cockpit,
                category = RoomCategory.Essential,
                level = 1,
                hitPoint = 100,
                size = new Vector2Int(2, 2),
                cost = 2500,
                powerRequirement = 25f,
                crewRequirement = 1,
                fuelEfficiency = 0f,
                avoidEfficiency = 0f,
                damageHitPointRate = RoomDamageRates.Create(50f, 10f),
                possibleDoorPositions = new List<DoorPosition>()
                {
                    new(new Vector2Int(0, 0), DoorDirection.South)
                },
                crewEntryGridPriority = new List<Vector2Int>()
                {
                    new(1, 1),
                    new(0, 1),
                    new(1, 0),
                    new(0, 0)
                }
            },
            new()
            {
                roomName = "room.cockpit.level2",
                roomType = RoomType.Cockpit,
                category = RoomCategory.Essential,
                level = 2,
                hitPoint = 150,
                size = new Vector2Int(3, 2),
                cost = 4000,
                powerRequirement = 35f,
                crewRequirement = 1,
                fuelEfficiency = 5f,
                avoidEfficiency = 2f,
                damageHitPointRate = RoomDamageRates.Create(75f, 15f),
                possibleDoorPositions = new List<DoorPosition>()
                {
                    new(new Vector2Int(1, 0), DoorDirection.South)
                },
                crewEntryGridPriority = new List<Vector2Int>()
                {
                    new(0, 1),
                    new(2, 1),
                    new(1, 1),
                    new(0, 0),
                    new(2, 0),
                    new(1, 0)
                }
            },
            new()
            {
                roomName = "room.cockpit.level3",
                roomType = RoomType.Cockpit,
                category = RoomCategory.Essential,
                level = 3,
                hitPoint = 200,
                size = new Vector2Int(3, 3),
                cost = 7000,
                powerRequirement = 45f,
                crewRequirement = 1,
                fuelEfficiency = 10f,
                avoidEfficiency = 5f,
                damageHitPointRate = RoomDamageRates.Create(50f, 10f),
                crewEntryGridPriority = new List<Vector2Int>()
                {
                    new(0, 2),
                    new(1, 2),
                    new(2, 2),
                    new(0, 1),
                    new(1, 1),
                    new(2, 1),
                    new(1, 0),
                    new(0, 0),
                    new(2, 0)
                }
            }
        };
    }
}
