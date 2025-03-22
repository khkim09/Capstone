using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 의무실의 레벨별 데이터를 저장하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "MedBayRoom", menuName = "RoomData/MedBayRoom Data")]
public class MedBayRoomData : RoomData<MedBayRoomData.MedBayRoomLevel>
{
    [System.Serializable]
    public class MedBayRoomLevel : RoomLevel
    {
        public float healPerSecond;
    }

    /// <summary>
    /// 기본 의무실 레벨 데이터 초기화
    /// </summary>
    protected override void InitializeDefaultLevels()
    {
        RoomLevels = new List<MedBayRoomLevel>
        {
            new()
            {
                roomName = "room.medbay.level1",
                level = 1,
                hitPoint = 100,
                size = new Vector2Int(2, 2),
                cost = 2000,
                powerRequirement = 10f,
                crewRequirement = 0,
                healPerSecond = 5f,
                damageHitPointRate =
                    new Dictionary<RoomDamageLevel, float>()
                    {
                        { RoomDamageLevel.DamageLevelOne, 60f }, { RoomDamageLevel.DamageLevelTwo, 30f }
                    }
            },
            new()
            {
                roomName = "room.medbay.level2",
                level = 2,
                hitPoint = 120,
                size = new Vector2Int(2, 3),
                cost = 4000,
                powerRequirement = 20f,
                crewRequirement = 0,
                healPerSecond = 8f,
                damageHitPointRate =
                    new Dictionary<RoomDamageLevel, float>()
                    {
                        { RoomDamageLevel.DamageLevelOne, 60f }, { RoomDamageLevel.DamageLevelTwo, 30f }
                    }
            }
        };
    }
}
