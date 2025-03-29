using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 조종실의 레벨별 데이터를 저장하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "CockpitRoomData", menuName = "RoomData/CockpitRoom Data")]
public class CockpitRoomData : RoomData<CockpitRoomData.CockpitRoomLevel>
{
    [System.Serializable]
    public class CockpitRoomLevel : RoomLevel
    {
        public float fuelEfficiency; // 에너지 효율 (%)
        public float avoidEfficiency; // 회피 효율 (%)
    }

    /// <summary>
    /// 기본 조종실 레벨 데이터 초기화
    /// </summary>
    protected override void InitializeDefaultLevels()
    {
        RoomLevels = new List<CockpitRoomLevel>
        {
            new()
            {
                roomName = "room.cockpit.level1",
                level = 1,
                hitPoint = 100,
                size = new Vector2Int(2, 2),
                cost = 2500,
                powerRequirement = 25f,
                crewRequirement = 1,
                fuelEfficiency = 0f,
                avoidEfficiency = 0f,
                damageHitPointRate =
                    new Dictionary<RoomDamageLevel, float>()
                    {
                        { RoomDamageLevel.DamageLevelOne, 50f }, { RoomDamageLevel.DamageLevelTwo, 10f }
                    }
            },
            new()
            {
                roomName = "room.cockpit.level3",
                level = 2,
                hitPoint = 150,
                size = new Vector2Int(2, 3),
                cost = 4000,
                powerRequirement = 35f,
                crewRequirement = 1,
                fuelEfficiency = 5f,
                avoidEfficiency = 2f,
                damageHitPointRate =
                    new Dictionary<RoomDamageLevel, float>()
                    {
                        { RoomDamageLevel.DamageLevelOne, 50f }, { RoomDamageLevel.DamageLevelTwo, 10f }
                    }
            },
            new()
            {
                roomName = "room.cockpit.level3",
                level = 3,
                hitPoint = 200,
                size = new Vector2Int(3, 3),
                cost = 7000,
                powerRequirement = 45f,
                crewRequirement = 1,
                fuelEfficiency = 10f,
                avoidEfficiency = 5f,
                damageHitPointRate =
                    new Dictionary<RoomDamageLevel, float>()
                    {
                        { RoomDamageLevel.DamageLevelOne, 50f }, { RoomDamageLevel.DamageLevelTwo, 10f }
                    }
            }
        };
    }
}
