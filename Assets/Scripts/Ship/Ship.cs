using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    // 방 타입별 프리팹
    [SerializeField] private EngineRoom engineRoomPrefab;

    [SerializeField] private Vector2Int gridSize = new(10, 10);

    [SerializeField] private StorageRoom storageRoomPrefab;
    private readonly Dictionary<Vector2Int, Room> roomGrid = new();

    private readonly List<Room> rooms = new();
    // 다른 방 타입 프리팹들...

    public bool AddRoom(Room room, Vector2Int position)
    {
        if (!IsValidPosition(position, room.size))
            return false;

        room.position = position;
        rooms.Add(room);

        // 그리드에 방 등록
        for (var x = 0; x < room.size.x; x++)
        for (var y = 0; y < room.size.y; y++)
        {
            var gridPos = position + new Vector2Int(x, y);
            roomGrid[gridPos] = room;
        }

        room.OnPlaced();
        return true;
    }

    private bool IsValidPosition(Vector2Int pos, Vector2Int size)
    {
        // 경계 확인
        if (pos.x < 0 || pos.y < 0 ||
            pos.x + size.x > gridSize.x ||
            pos.y + size.y > gridSize.y)
            return false;

        // 다른 방과 겹치는지 확인
        for (var x = 0; x < size.x; x++)
        for (var y = 0; y < size.y; y++)
        {
            var checkPos = pos + new Vector2Int(x, y);
            if (roomGrid.ContainsKey(checkPos))
                return false;
        }

        return true;
    }

    public void SetupDefaultShip()
    {
        // 기본 엔진룸 생성
        var engineRoom = Instantiate(engineRoomPrefab);
        AddRoom(engineRoom, new Vector2Int(0, 0));

        // 기본 창고 생성
        var storage = Instantiate(storageRoomPrefab);
        storage.storageType = StorageType.Normal;
        AddRoom(storage, new Vector2Int(2, 0));

        // 다른 필수 방들 생성...
    }
}
