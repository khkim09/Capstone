using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 배리어실의 레벨별 데이터를 저장하는 ScriptableObject.
/// 방어막 재생 시간, 최대 방어막 수치, 초당 재생량을 정의합니다.
/// </summary>
[CreateAssetMenu(fileName = "ShieldRoomData", menuName = "RoomData/ShieldRoom Data")]
public class ShieldRoomData : RoomData<ShieldRoomData.ShieldRoomLevel>
{
    /// <summary>
    /// 배리어실의 레벨별 데이터 구조.
    /// </summary>
    [System.Serializable]
    public class ShieldRoomLevel : RoomLevel
    {
        /// <summary>방어막 재생 시작까지 걸리는 시간 (초).</summary>
        public float shieldRespawnTime;

        /// <summary>방어막 최대 수치.</summary>
        public float shieldMaxAmount;

        /// <summary>방어막 최대 수치.</summary>
        public float shieldReneratePerSecond;
    }

    /// <summary>
    /// 기본 배리어실 레벨 데이터를 초기화합니다.
    /// </summary>
    protected override void InitializeDefaultLevels()
    {
        RoomLevels = new List<ShieldRoomLevel>
        {
            new()
            {
                roomName = "room.shield.level1",
                roomType = RoomType.Shield,
                category = RoomCategory.Auxiliary,
                level = 1,
                hitPoint = 200,
                size = new Vector2Int(3, 2),
                cost = 2000,
                powerRequirement = 30f,
                crewRequirement = 0,
                shieldRespawnTime = 30f,
                shieldMaxAmount = 100f,
                shieldReneratePerSecond = 5,
                damageHitPointRate = RoomDamageRates.Create(50f, 25f),
                possibleDoorPositions = new List<DoorPosition>() { new(new Vector2Int(2, 0), DoorDirection.South) }
            },
            new()
            {
                roomName = "room.shield.level2",
                roomType = RoomType.Shield,
                category = RoomCategory.Auxiliary,
                level = 2,
                hitPoint = 300,
                size = new Vector2Int(3, 2),
                cost = 4000,
                powerRequirement = 50f,
                crewRequirement = 0,
                shieldRespawnTime = 25f,
                shieldMaxAmount = 200f,
                shieldReneratePerSecond = 8,
                damageHitPointRate = RoomDamageRates.Create(50f, 25f),
                possibleDoorPositions = new List<DoorPosition>() { new(new Vector2Int(2, 0), DoorDirection.South) }
            }
        };
    }
}
