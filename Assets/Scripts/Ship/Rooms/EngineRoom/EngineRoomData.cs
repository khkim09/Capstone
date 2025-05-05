using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 엔진실의 레벨별 데이터를 저장하는 ScriptableObject.
/// 연료 소모, 회피율, 연료 효율 등의 값을 레벨별로 정의합니다.
/// </summary>
[CreateAssetMenu(fileName = "EngineRoomData", menuName = "RoomData/EngineRoom Data")]
public class EngineRoomData : RoomData<EngineRoomData.EngineRoomLevel>
{
    /// <summary>
    /// 엔진실의 레벨별 데이터 구조.
    /// </summary>
    [System.Serializable]
    public class EngineRoomLevel : RoomLevel
    {
        /// <summary>연료 보관 용량</summary>
        public float fuelStoreLiter;

        /// <summary>워프 시 연료 소모량.</summary>
        public float fuelConsumption;

        /// <summary>연료 효율 보너스 (%).</summary>
        public float fuelEfficiency;

        /// <summary>회피 확률 보너스 (%).</summary>
        public float avoidEfficiency;
    }


    /// <summary>
    /// 기본 엔진 레벨 데이터를 초기화합니다.
    /// </summary>
    protected override void InitializeDefaultLevels()
    {
        RoomLevels = new List<EngineRoomLevel>
        {
            new()
            {
                roomName = "room.engine.level1",
                roomType = RoomType.Engine,
                category = RoomCategory.Essential,
                level = 1,
                hitPoint = 100,
                size = new Vector2Int(2, 2),
                cost = 2500,
                powerRequirement = 50f,
                crewRequirement = 1,
                fuelStoreLiter = 150f,
                fuelConsumption = 10f,
                fuelEfficiency = 0f,
                avoidEfficiency = 0f,
                damageHitPointRate = RoomDamageRates.Create(60f, 20f),
                possibleDoorPositions = new List<DoorPosition>()
                {
                    new(new Vector2Int(1, 0), DoorDirection.East)
                },
                crewEntryGridPriority = new List<Vector2Int>()
                {
                    new(0, 1),
                    new(0, 0),
                    new(1, 1),
                    new(1, 0)
                }
            },
            new()
            {
                roomName = "room.engine.level2",
                roomType = RoomType.Engine,
                category = RoomCategory.Essential,
                level = 2,
                hitPoint = 200,
                size = new Vector2Int(3, 2),
                cost = 4000,
                powerRequirement = 75f,
                crewRequirement = 1,
                fuelStoreLiter = 200f,
                fuelConsumption = 8f,
                fuelEfficiency = 5f,
                avoidEfficiency = 2f,
                damageHitPointRate = RoomDamageRates.Create(120f, 40f),
                possibleDoorPositions = new List<DoorPosition>()
                {
                    new(new Vector2Int(2, 1), DoorDirection.East)
                },
                crewEntryGridPriority = new List<Vector2Int>()
                {
                    new (0, 0),
                    new (0, 1),
                    new (1, 0),
                    new (1, 1),
                    new (2, 0),
                    new (2, 1)
                }
            },
            new()
            {
                roomName = "room.engine.level3",
                roomType = RoomType.Engine,
                category = RoomCategory.Essential,
                level = 3,
                hitPoint = 300,
                size = new Vector2Int(3, 3),
                cost = 7000,
                powerRequirement = 100f,
                crewRequirement = 1,
                fuelStoreLiter = 200f,
                fuelConsumption = 5f,
                fuelEfficiency = 10f,
                avoidEfficiency = 5f,
                damageHitPointRate = RoomDamageRates.Create(180f, 60f),
                possibleDoorPositions = new List<DoorPosition>()
                {
                    new(new Vector2Int(1, 0), DoorDirection.South)
                },
                crewEntryGridPriority = new List<Vector2Int>()
                {
                    new (2, 2),
                    new (1, 2),
                    new (0, 2),
                    new (2, 1),
                    new (1, 1),
                    new (0, 1),
                    new (2, 0),
                    new (1, 0),
                    new (0, 0)
                }
            }
        };
    }
}
