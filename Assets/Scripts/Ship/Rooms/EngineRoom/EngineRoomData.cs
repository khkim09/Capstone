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
                level = 1,
                hitPoint = 100,
                size = new Vector2Int(2, 2),
                cost = 2500,
                powerRequirement = 50f,
                crewRequirement = 1,
                fuelConsumption = 10f,
                fuelEfficiency = 0f,
                avoidEfficiency = 0f,
                damageHitPointRate =
                    new Dictionary<RoomDamageLevel, float>()
                    {
                        { RoomDamageLevel.DamageLevelOne, 60f }, { RoomDamageLevel.DamageLevelTwo, 20f }
                    }
            },
            new()
            {
                roomName = "room.engine.level2",
                level = 2,
                hitPoint = 200,
                size = new Vector2Int(2, 3),
                cost = 4000,
                powerRequirement = 75f,
                crewRequirement = 1,
                fuelConsumption = 8f,
                fuelEfficiency = 5f,
                avoidEfficiency = 2f,
                damageHitPointRate =
                    new Dictionary<RoomDamageLevel, float>()
                    {
                        { RoomDamageLevel.DamageLevelOne, 60f }, { RoomDamageLevel.DamageLevelTwo, 20f }
                    }
            },
            new()
            {
                roomName = "room.engine.level3",
                level = 3,
                hitPoint = 300,
                size = new Vector2Int(3, 3),
                cost = 7000,
                powerRequirement = 100f,
                crewRequirement = 1,
                fuelConsumption = 5f,
                fuelEfficiency = 10f,
                avoidEfficiency = 5f,
                damageHitPointRate =
                    new Dictionary<RoomDamageLevel, float>()
                    {
                        { RoomDamageLevel.DamageLevelOne, 60f }, { RoomDamageLevel.DamageLevelTwo, 20f }
                    }
            }
        };
    }
}
