using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 조준석(WeaponControlRoom)의 레벨별 데이터를 저장하는 ScriptableObject.
/// 명중률(Accuracy)과 전력 소비량을 정의합니다.
/// </summary>
[CreateAssetMenu(fileName = "WeaponControlRoom", menuName = "RoomData/WeaponControlRoom Data")]
public class WeaponControlRoomData : RoomData<WeaponControlRoomData.WeaponControlRoomLevel>
{
    /// <summary>
    /// 조준석의 레벨별 데이터 구조.
    /// </summary>
    [System.Serializable]
    public class WeaponControlRoomLevel : RoomLevel
    {
        public float accuracy;
    }

    /// <summary>
    /// 기본 조준석 레벨 데이터를 초기화합니다.
    /// </summary>
    protected override void InitializeDefaultLevels()
    {
        RoomLevels = new List<WeaponControlRoomLevel>
        {
            new()
            {
                roomName = "room.weaponcontrol.level1",
                roomType = RoomType.WeaponControl,
                level = 1,
                hitPoint = 100,
                size = new Vector2Int(2, 1),
                cost = 2000,
                powerRequirement = 30f,
                crewRequirement = 0,
                accuracy = 10f,
                damageHitPointRate = RoomDamageRates.Create(60f, 30f),
                possibleDoorPositions =
                    new List<DoorPosition>() { new(new Vector2Int(1, 0), DoorDirection.East) }
            },
            new()
            {
                roomName = "room.weaponcontrol.level2",
                roomType = RoomType.WeaponControl,
                level = 2,
                hitPoint = 120,
                size = new Vector2Int(2, 1),
                cost = 3500,
                powerRequirement = 50f,
                crewRequirement = 0,
                accuracy = 20,
                damageHitPointRate = RoomDamageRates.Create(60f, 30f),
                possibleDoorPositions =
                    new List<DoorPosition>() { new(new Vector2Int(1, 0), DoorDirection.East) }
            }
        };
    }
}
