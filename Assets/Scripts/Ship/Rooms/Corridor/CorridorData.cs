using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 복도(RoomType.Corridor)의 ScriptableObject 데이터
/// </summary>
[CreateAssetMenu(fileName = "CorridorRoomData", menuName = "RoomData/CorridorRoom Data")]
public class CorridorRoomData : RoomData<CorridorRoomData.CorridorRoomLevel>
{
    /// <summary>
    /// 복도의 레벨별 데이터 구조
    /// </summary>
    [System.Serializable]
    public class CorridorRoomLevel : RoomLevel
    {
        // 복도는 특별한 능력치 없이 이동속도만 부여
    }

    /// <summary>
    /// 기본 복도 레벨 데이터를 초기화 합니다.
    /// </summary>
    protected override void InitializeDefaultLevels()
    {
        RoomLevels = new List<CorridorRoomLevel>
        {
            new()
            {
                roomName = "room.corridor.level1",
                roomType = RoomType.Corridor,
                category = RoomCategory.Etc,
                level = 1,
                size = new Vector2Int(1, 1),
                cost = 10,
                powerRequirement = 0f,
                crewRequirement = 0,
                possibleDoorPositions = new List<DoorPosition>
                {
                    new(new Vector2Int(0, 0), DoorDirection.North),
                    new(new Vector2Int(0, 0), DoorDirection.East),
                    new(new Vector2Int(0, 0), DoorDirection.South),
                    new(new Vector2Int(0, 0), DoorDirection.West)
                },
                crewEntryGridPriority = new List<Vector2Int>()
                {
                    new(0, 0)
                }
            }
        };
    }
}
