using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 엔진실의 레벨별 데이터를 저장하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "EngineRoomData", menuName = "RoomData/EngineRoom Data")]
public class EngineRoomData : RoomData<EngineRoomData.EngineRoomLevel>
{
    [System.Serializable]
    public class EngineRoomLevel : RoomLevel
    {
        public float fuelConsumption; // 워프 시 연료 소모량
        public float fuelEfficiency; // 에너지 효율 (%)
        public float avoidEfficiency; // 회피 효율 (%)
    }


    /// <summary>
    /// 기본 엔진 레벨 데이터 초기화
    /// </summary>
    protected override void InitializeDefaultLevels()
    {
        RoomLevels = new List<EngineRoomLevel>
        {
            new()
            {
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
