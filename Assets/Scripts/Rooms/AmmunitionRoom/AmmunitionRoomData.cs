using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 탄약고의 레벨별 데이터를 저장하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "AmmunitionRoomData", menuName = "RoomData/AmmunitionRoom Data")]
public class AmmunitionRoomData : RoomData<AmmunitionRoomData.AmmunitionRoomLevel>
{
    [System.Serializable]
    public class AmmunitionRoomLevel : RoomLevel
    {
        public float reloadTimeBonus;
        public float damageBonus;
    }

    /// <summary>
    /// 기본 탄약고 레벨 데이터 초기화
    /// </summary>
    protected override void InitializeDefaultLevels()
    {
        RoomLevels = new List<AmmunitionRoomLevel>
        {
            new()
            {
                roomName = "room.ammunition.level1",
                level = 1,
                hitPoint = 100,
                size = new Vector2Int(2, 1),
                cost = 1800,
                powerRequirement = 40f,
                crewRequirement = 0,
                reloadTimeBonus = 10f,
                damageBonus = 0f,
                damageHitPointRate =
                    new Dictionary<RoomDamageLevel, float>()
                    {
                        { RoomDamageLevel.DamageLevelOne, 60f }, { RoomDamageLevel.DamageLevelTwo, 30f }
                    }
            },
            new()
            {
                roomName = "room.ammunition.level2",
                level = 2,
                hitPoint = 200,
                size = new Vector2Int(2, 1),
                cost = 3000,
                powerRequirement = 60f,
                crewRequirement = 0,
                reloadTimeBonus = 20f,
                damageBonus = 10f,
                damageHitPointRate =
                    new Dictionary<RoomDamageLevel, float>()
                    {
                        { RoomDamageLevel.DamageLevelOne, 60f }, { RoomDamageLevel.DamageLevelTwo, 30f }
                    }
            }
        };
    }
}
