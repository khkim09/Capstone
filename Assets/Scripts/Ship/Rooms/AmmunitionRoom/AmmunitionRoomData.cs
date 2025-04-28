using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 탄약고의 레벨별 데이터를 저장하는 ScriptableObject.
/// 각 레벨에서 재장전 보너스와 데미지 보너스를 정의합니다.
/// </summary>
[CreateAssetMenu(fileName = "AmmunitionRoomData", menuName = "RoomData/AmmunitionRoom Data")]
public class AmmunitionRoomData : RoomData<AmmunitionRoomData.AmmunitionRoomLevel>
{
    /// <summary>
    /// 탄약고의 각 레벨별 데이터 구조.
    /// </summary>
    [System.Serializable]
    public class AmmunitionRoomLevel : RoomLevel
    {
        /// <summary>재장전 속도 보너스 (%).</summary>
        public float reloadTimeBonus;

        /// <summary>데미지 보너스 (%).</summary>
        public float damageBonus;
    }

    /// <summary>
    /// 기본 탄약고 레벨 데이터를 초기화합니다.
    /// </summary>
    protected override void InitializeDefaultLevels()
    {
        RoomLevels = new List<AmmunitionRoomLevel>
        {
            new()
            {
                roomName = "room.ammunition.level1",
                roomType = RoomType.Ammunition,
                category = RoomCategory.Auxiliary,
                level = 1,
                hitPoint = 100,
                size = new Vector2Int(2, 1),
                cost = 1800,
                powerRequirement = 40f,
                crewRequirement = 0,
                reloadTimeBonus = 10f,
                damageBonus = 0f,
                damageHitPointRate = RoomDamageRates.Create(60f, 30f),
                possibleDoorPositions = new List<DoorPosition>()
                {
                    new(new Vector2Int(1, 0), DoorDirection.East)
                },
                crewEntryGridPriority = new List<Vector2Int>()
                {
                    new (0, 0),
                    new (1, 0)
                }
            },
            new()
            {
                roomName = "room.ammunition.level2",
                roomType = RoomType.Ammunition,
                category = RoomCategory.Auxiliary,
                level = 2,
                hitPoint = 200,
                size = new Vector2Int(2, 1),
                cost = 3000,
                powerRequirement = 60f,
                crewRequirement = 0,
                reloadTimeBonus = 20f,
                damageBonus = 10f,
                damageHitPointRate = RoomDamageRates.Create(120f, 60f),
                possibleDoorPositions = new List<DoorPosition>()
                {
                    new(new Vector2Int(1, 0), DoorDirection.East)
                },
                crewEntryGridPriority = new List<Vector2Int>()
                {
                    new (0, 0),
                    new (1, 0)
                }
            }
        };
    }
}
