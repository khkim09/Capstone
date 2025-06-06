﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 전력실의 레벨별 데이터를 저장하는 ScriptableObject.
/// 각 레벨은 전력 생산량(PowerCapacity)에 대응되며, 별도 파라미터 없이 base 속성만 활용됩니다.
/// </summary>
[CreateAssetMenu(fileName = "PowerRoomData", menuName = "RoomData/PowerRoom Data")]
public class PowerRoomData : RoomData<PowerRoomData.PowerRoomLevel>
{
    /// <summary>
    /// 전력실의 레벨별 데이터 구조.
    /// 특별한 속성 없이 기본 RoomLevel 속성만 사용됩니다.
    /// </summary>
    [System.Serializable]
    public class PowerRoomLevel : RoomLevel
    {
    }

    /// <summary>
    /// 기본 전력실 레벨 데이터를 초기화합니다.
    /// </summary>
    protected override void InitializeDefaultLevels()
    {
        RoomLevels = new List<PowerRoomLevel>
        {
            new()
            {
                roomName = "room.power.level1",
                roomType = RoomType.Power,
                category = RoomCategory.Essential,
                level = 1,
                hitPoint = 200,
                size = new Vector2Int(2, 2),
                cost = 4000,
                powerRequirement = 3000f,
                crewRequirement = 0,
                damageHitPointRate = RoomDamageRates.Create(100f, 20f),
                possibleDoorPositions = new List<DoorPosition>()
                {
                    new(new Vector2Int(1, 1), DoorDirection.East)
                },
                crewEntryGridPriority = new List<Vector2Int>()
                {
                    new(0, 0),
                    new(0, 1),
                    new(1, 0),
                    new(1, 1)
                }
            },
            new()
            {
                roomName = "room.power.level2",
                roomType = RoomType.Power,
                category = RoomCategory.Essential,
                level = 2,
                hitPoint = 300,
                size = new Vector2Int(3, 3),
                cost = 8000,
                powerRequirement = 7000f,
                crewRequirement = 0,
                damageHitPointRate = RoomDamageRates.Create(150f, 30f),
                possibleDoorPositions = new List<DoorPosition>()
                {
                    new(new Vector2Int(0, 1), DoorDirection.West),
                    new(new Vector2Int(2, 1), DoorDirection.East)
                },
                crewEntryGridPriority = new List<Vector2Int>()
                {
                    new (0, 2),
                    new (1, 2),
                    new (2, 2),
                    new (0, 0),
                    new (1, 0),
                    new (2, 0),
                    new (1, 1),
                    new (0, 1),
                    new (2, 1)
                }
            },
            new()
            {
                roomName = "room.power.level3",
                roomType = RoomType.Power,
                category = RoomCategory.Essential,
                level = 3,
                hitPoint = 400,
                size = new Vector2Int(4, 4),
                cost = 12000,
                powerRequirement = 12000f,
                crewRequirement = 0,
                damageHitPointRate = RoomDamageRates.Create(200f, 40f),
                possibleDoorPositions = new List<DoorPosition>()
                {
                    new(new Vector2Int(0, 2), DoorDirection.West),
                    new(new Vector2Int(3, 1), DoorDirection.East)
                },
                crewEntryGridPriority = new List<Vector2Int>()
                {
                    new (0, 3),
                    new (1, 3),
                    new (2, 3),
                    new (3, 3),
                    new (0, 0),
                    new (1, 0),
                    new (2, 0),
                    new (3, 0),
                    new (3, 2),
                    new (2, 2),
                    new (1, 2),
                    new (0, 1),
                    new (1, 1),
                    new (2, 1),
                    new (3, 1),
                    new (0, 2)
                }
            }
        };
    }
}
