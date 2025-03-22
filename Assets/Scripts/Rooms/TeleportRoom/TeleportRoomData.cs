using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 텔레포트실의 레벨별 데이터를 저장하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "TeleportRoom", menuName = "RoomData/TeleportRoom Data")]
public class TeleportRoomData : RoomData<TeleportRoomData.TeleportRoomLevel>
{
    [System.Serializable]
    public class TeleportRoomLevel : RoomLevel
    {
    }
    // TODO: 텔레포터 관련 스탯 더 세부적으로 정해야할 것

    /// <summary>
    /// 기본 텔레포트실 레벨 데이터 초기화
    /// </summary>
    protected override void InitializeDefaultLevels()
    {
        RoomLevels = new List<TeleportRoomLevel>
        {
            new()
            {
                roomName = "room.teleport.level1",
                level = 1,
                hitPoint = 100,
                size = new Vector2Int(1, 1),
                cost = 700,
                powerRequirement = 20f,
                crewRequirement = 0,
                damageHitPointRate =
                    new Dictionary<RoomDamageLevel, float>()
                    {
                        { RoomDamageLevel.DamageLevelOne, 50f }, { RoomDamageLevel.DamageLevelTwo, 25f }
                    }
            }
        };
    }
}
