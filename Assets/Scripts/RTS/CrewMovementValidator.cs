using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 선원 이동 시 경로 유효성 검사 담당
/// 방/복도 타일 이동 허용, 방-방 이동 시 문 방향, 위치 검사
/// </summary>
public class CrewMovementValidator : MonoBehaviour
{
    public List<Vector2Int> inspectorWalkable;

    private HashSet<Vector2Int> walkableTiles; // 이동 가능한 타일 목록 (방/복도 점유 타일)
    private List<Room> rooms; // 전체 방 리스트

    /// <summary>
    /// 초기화 - 방 리스트 등록
    /// </summary>
    public void Initialize(List<Room> allRooms)
    {
        walkableTiles = new HashSet<Vector2Int>();
        rooms = allRooms;

        foreach (var room in allRooms)
            foreach (var tile in room.GetOccupiedTiles())
                walkableTiles.Add(tile);

        inspectorWalkable = new List<Vector2Int>(walkableTiles);
    }

    /// <summary>
    /// 타일이 이동 가능한가 (방/복도에 속하는가)
    /// </summary>
    public bool IsTileWalkable(Vector2Int tile)
    {
        Debug.LogWarning($"{tile} istilewalkable : {walkableTiles.Contains(tile)}");
        return walkableTiles.Contains(tile);
    }

    /// <summary>
    /// 두 타일이 이동 가능한가 (같은 방 or 문을 통한 이동)
    /// </summary>
    public bool AreTilesConnected(Vector2Int from, Vector2Int to)
    {
        Room roomA = GetRoomAtTile(from);
        Room roomB = GetRoomAtTile(to);

        Debug.LogError($"{roomA} -> {roomB}");

        if (roomA == null || roomB == null)
            return false;

        // 같은 방 내부
        if (roomA == roomB)
            return true;

        // 복도 <-> 복도 : 항상 이동 가능
        if (roomA.roomType == RoomType.Corridor && roomB.roomType == RoomType.Corridor)
            return true;

        // 복도 <-> 방 : 방의 문 타일 위치에서만 이동 가능
        if (roomA.roomType == RoomType.Corridor || roomB.roomType == RoomType.Corridor)
            return CorridorRoomConnectionValid(roomA, roomB, from, to);

        // 방 <-> 방 : 연결성 검사 (문 위치, 방향)
        return AreRoomsConnectedThroughDoors(roomA, roomB, from, to);
    }

    /// <summary>
    /// 특정 타일을 점유하고 있는 방 찾기
    /// </summary>
    private Room GetRoomAtTile(Vector2Int tile)
    {
        foreach (var room in rooms)
            if (room.GetOccupiedTiles().Contains(tile))
                return room;
        return null;
    }

    /// <summary>
    /// 복도 <-> 방 이동 시 문 연결성 검사
    /// </summary>
    /// <param name="roomA"></param>
    /// <param name="roomB"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    private bool CorridorRoomConnectionValid(Room roomA, Room roomB, Vector2Int from, Vector2Int to)
    {
        // 복도, 일반 방 설정
        Room corridorRoom = roomA.roomType == RoomType.Corridor ? roomA : roomB;
        Room normalRoom = roomA.roomType == RoomType.Corridor ? roomB : roomA;

        Vector2Int corridorPos = corridorRoom == roomA ? from : to;

        List<DoorPosition> doors = normalRoom.GetRoomData().GetDoorPositionsWithDirection(normalRoom.GetCurrentLevel(), normalRoom.position, normalRoom.currentRotation);

        foreach (DoorPosition door in doors)
        {
            // 이웃 타일
            Vector2Int expectedNeighbor = door.position + GetDirectionOffset(door.direction);
            if (expectedNeighbor == corridorPos)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 방과 방 사이가 문으로 연결돼 있는지 검사
    /// ShipValidationHelper.cs의 CheckAllRoomsPathConnectedByDoors 방식 반영
    /// </summary>
    private bool AreRoomsConnectedThroughDoors(Room roomA, Room roomB, Vector2Int from, Vector2Int to)
    {
        List<DoorPosition> doorsA = roomA.GetRoomData().GetDoorPositionsWithDirection(roomA.GetCurrentLevel(), roomA.position, roomA.currentRotation);
        List<DoorPosition> doorsB = roomB.GetRoomData().GetDoorPositionsWithDirection(roomB.GetCurrentLevel(), roomB.position, roomB.currentRotation);

        foreach (DoorPosition doorA in doorsA)
        {
            foreach (DoorPosition doorB in doorsB)
            {
                // 조건1: 문 위치가 인접
                bool positionMatch = DoorPositionMatch(doorA.position, doorA.direction, doorB.position, doorB.direction);

                // 조건2: 문 방향이 서로 반대
                bool directionMatch = GetOppositeDoorDirection(doorA.direction) == doorB.direction;

                if (positionMatch && directionMatch)
                {
                    Debug.LogError($"{roomA}, {roomB}는 연결됨");
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 두 문의 위치가 인접한지 검사
    /// </summary>
    private bool DoorPositionMatch(Vector2Int doorPosA, DoorDirection dirA, Vector2Int doorPosB, DoorDirection dirB)
    {
        Vector2Int expectedNeighborForA = doorPosA + GetDirectionOffset(dirA);
        Vector2Int expectedNeighborForB = doorPosB + GetDirectionOffset(dirB);

        return (doorPosA == expectedNeighborForB) && (doorPosB == expectedNeighborForA);
    }

    /// <summary>
    /// 문 방향에 따른 그리드 타일 위치 offset 세팅
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    private Vector2Int GetDirectionOffset(DoorDirection dir)
    {
        return dir switch
        {
            DoorDirection.North => Vector2Int.up,
            DoorDirection.East => Vector2Int.right,
            DoorDirection.South => Vector2Int.down,
            DoorDirection.West => Vector2Int.left,
            _ => Vector2Int.zero
        };
    }

    /// <summary>
    /// 현재 문의 반대 방향 반환
    /// </summary>
    private DoorDirection GetOppositeDoorDirection(DoorDirection dir)
    {
        return dir switch
        {
            DoorDirection.North => DoorDirection.South,
            DoorDirection.South => DoorDirection.North,
            DoorDirection.East => DoorDirection.West,
            DoorDirection.West => DoorDirection.East,
            _ => dir
        };
    }
}
