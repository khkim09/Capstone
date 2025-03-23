using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 조준석의 레벨별 데이터를 저장하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "WeaponControlRoom", menuName = "RoomData/WeaponControlRoom Data")]
public class WeaponControlRoomData : RoomData<WeaponControlRoomData.WeaponControlRoomLevel>
{
    [System.Serializable]
    public class WeaponControlRoomLevel : RoomLevel
    {
        public float accuracy;
    }

    /// <summary>
    /// 기본 조준석 레벨 데이터 초기화
    /// </summary>
    protected override void InitializeDefaultLevels()
    {
        RoomLevels = new List<WeaponControlRoomLevel>
        {
            new()
            {
                roomName = "room.weaponcontrol.level1",
                level = 1,
                hitPoint = 100,
                size = new Vector2Int(2, 1),
                cost = 2000,
                powerRequirement = 30f,
                crewRequirement = 0,
                accuracy = 10f,
                damageHitPointRate =
                    new Dictionary<RoomDamageLevel, float>()
                    {
                        { RoomDamageLevel.DamageLevelOne, 60f }, { RoomDamageLevel.DamageLevelTwo, 30f }
                    }
            },
            new()
            {
                roomName = "room.weaponcontrol.level2",
                level = 2,
                hitPoint = 120,
                size = new Vector2Int(2, 1),
                cost = 3500,
                powerRequirement = 50f,
                crewRequirement = 0,
                accuracy = 20,
                damageHitPointRate =
                    new Dictionary<RoomDamageLevel, float>()
                    {
                        { RoomDamageLevel.DamageLevelOne, 60f }, { RoomDamageLevel.DamageLevelTwo, 30f }
                    }
            }
        };
    }
}
