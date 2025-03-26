using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 전력실의 레벨별 데이터를 저장하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "PowerRoomData", menuName = "RoomData/PowerRoom Data")]
public class PowerRoomData : RoomData<PowerRoomData.PowerRoomLevel>
{
    [System.Serializable]
    public class PowerRoomLevel : RoomLevel
    {
    }

    /// <summary>
    /// 기본 전력실 레벨 데이터 초기화
    /// </summary>
    protected override void InitializeDefaultLevels()
    {
        RoomLevels = new List<PowerRoomLevel>
        {
            new()
            {
                roomName = "room.power.level1",
                level = 1,
                hitPoint = 200,
                size = new Vector2Int(2, 2),
                cost = 2000,
                powerRequirement = 3000f,
                crewRequirement = 0,
                damageHitPointRate =
                    new Dictionary<RoomDamageLevel, float>()
                    {
                        { RoomDamageLevel.DamageLevelOne, 50f }, { RoomDamageLevel.DamageLevelTwo, 10f }
                    }
            },
            new()
            {
                roomName = "room.power.level2",
                level = 2,
                hitPoint = 300,
                size = new Vector2Int(3, 3),
                cost = 8000,
                powerRequirement = 7000f,
                crewRequirement = 0,
                damageHitPointRate =
                    new Dictionary<RoomDamageLevel, float>()
                    {
                        { RoomDamageLevel.DamageLevelOne, 50f }, { RoomDamageLevel.DamageLevelTwo, 10f }
                    }
            },
            new()
            {
                roomName = "room.power.level3",
                level = 3,
                hitPoint = 400,
                size = new Vector2Int(4, 4),
                cost = 12000,
                powerRequirement = 12000f,
                crewRequirement = 0,
                damageHitPointRate =
                    new Dictionary<RoomDamageLevel, float>()
                    {
                        { RoomDamageLevel.DamageLevelOne, 50f }, { RoomDamageLevel.DamageLevelTwo, 10f }
                    }
            }
        };
    }
}
