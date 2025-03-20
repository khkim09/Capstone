using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 산소실의 레벨별 데이터를 저장하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "OxygenRoomData", menuName = "RoomData/OxygenRoom Data")]
public class OxygenRoomData : RoomData<OxygenRoomData.OxygenLevel>
{
    [System.Serializable]
    public class OxygenLevel : RoomLevel
    {
        public float oxygenSupplyPerSecond; // 초당 산소 공급량
    }

    /// <summary>
    /// 기본 산소실 레벨 데이터 초기화
    /// </summary>
    protected override void InitializeDefaultLevels()
    {
        RoomLevels = new List<OxygenLevel>
        {
            new()
            {
                level = 1,
                hitPoint = 80,
                size = new Vector2Int(2, 1),
                cost = 500,
                powerRequirement = 10f,
                crewRequirement = 0,
                oxygenSupplyPerSecond = 5f,
                damageHitPointRate =
                    new Dictionary<RoomDamageLevel, float>()
                    {
                        { RoomDamageLevel.DamageLevelOne, 80f }, { RoomDamageLevel.DamageLevelTwo, 30f }
                    }
            },
            new()
            {
                level = 2,
                hitPoint = 160,
                size = new Vector2Int(2, 1),
                cost = 1500,
                powerRequirement = 15f,
                crewRequirement = 0,
                oxygenSupplyPerSecond = 8f,
                damageHitPointRate =
                    new Dictionary<RoomDamageLevel, float>()
                    {
                        { RoomDamageLevel.DamageLevelOne, 80f }, { RoomDamageLevel.DamageLevelTwo, 30f }
                    }
            }
        };
    }
}
