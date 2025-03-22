using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 배리어실의 레벨별 데이터를 저장하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "ShieldRoom", menuName = "RoomData/ShieldRoom Data")]
public class ShieldRoomData : RoomData<ShieldRoomData.ShieldRoomLevel>
{
    [System.Serializable]
    public class ShieldRoomLevel : RoomLevel
    {
        public float shieldRespawnTime;
        public float shieldMaxAmount;
        public float shieldReneratePerSecond;
    }

    /// <summary>
    /// 기본 배리어실 레벨 데이터 초기화
    /// </summary>
    protected override void InitializeDefaultLevels()
    {
        RoomLevels = new List<ShieldRoomLevel>
        {
            new()
            {
                roomName = "room.shield.level1",
                level = 1,
                hitPoint = 200,
                size = new Vector2Int(2, 3),
                cost = 2000,
                powerRequirement = 30f,
                crewRequirement = 0,
                shieldRespawnTime = 30f,
                shieldMaxAmount = 100f,
                shieldReneratePerSecond = 5,
                damageHitPointRate =
                    new Dictionary<RoomDamageLevel, float>()
                    {
                        { RoomDamageLevel.DamageLevelOne, 50f }, { RoomDamageLevel.DamageLevelTwo, 25f }
                    }
            },
            new()
            {
                roomName = "room.shield.level2",
                level = 2,
                hitPoint = 300,
                size = new Vector2Int(2, 3),
                cost = 4000,
                powerRequirement = 50f,
                crewRequirement = 0,
                shieldRespawnTime = 25f,
                shieldMaxAmount = 200f,
                shieldReneratePerSecond = 8,
                damageHitPointRate =
                    new Dictionary<RoomDamageLevel, float>()
                    {
                        { RoomDamageLevel.DamageLevelOne, 50f }, { RoomDamageLevel.DamageLevelTwo, 25f }
                    }
            }
        };
    }
}
