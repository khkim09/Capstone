using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 함선의 방 배치와 무기 배치가 유효한지 검증하는 클래스입니다.
/// 모든 방이 서로 연결되어 있고, 문이 적절히 배치되어 있는지 확인하며,
/// 무기의 발사 경로를 검증합니다.
/// </summary>
public class ShipValidationHelper
{
    #region Ship, Room 버전
    /// <summary>
    /// 함선 배치의 유효성을 검사합니다.
    /// </summary>
    /// <param name="ship">검증할 함선 객체</param>
    /// <returns>유효성 검증 결과와 실패 사유</returns>
    public ValidationResult ValidateShipLayout(Ship ship)
    {
        List<Room> rooms = ship.GetAllRooms();

        if (rooms == null || rooms.Count == 0)
        {
            return new ValidationResult(ShipValidationError.NoRoom, "shipvalidation.error.noroom");
        }

        // 필수 방 체크 (예: 조종실, 엔진실은 필수)
        ValidationResult requiredRoomsCheck = CheckRequiredRooms(rooms);
        if (!requiredRoomsCheck.IsValid)
        {
            return requiredRoomsCheck;
        }

        // 모든 방이 연결되어 있는지 확인 (BFS 또는 DFS로 탐색)
        ValidationResult connectivityCheck = CheckAllRoomsConnected(rooms);
        if (!connectivityCheck.IsValid)
        {
            return connectivityCheck;
        }

        // 모든 문이 적절히 연결되어 있는지 확인
        ValidationResult doorCheck = CheckAllDoorsConnected(rooms);
        if (!doorCheck.IsValid)
        {
            return doorCheck;
        }

        // 무기 배치 확인 - 무기가 배에 붙어있는지, 발사 경로에 방해물이 없는지 확인
        ValidationResult weaponCheck = CheckWeaponPlacement(ship);
        if (!weaponCheck.IsValid)
        {
            return weaponCheck;
        }

        return new ValidationResult(ShipValidationError.None, "shipvalidation.error.none");
    }

    /// <summary>
    /// 필수 방이 존재하는지 확인합니다.
    /// </summary>
    private ValidationResult CheckRequiredRooms(List<Room> rooms)
    {
        // 조종실이 있는지 확인
        bool hasCockpit = rooms.Any(r => r.roomType == RoomType.Cockpit);
        if (!hasCockpit)
        {
            return new ValidationResult(ShipValidationError.MissingRequiredRoom, "shipvalidation.error.nocockpit");
        }

        // 엔진실이 있는지 확인
        bool hasEngine = rooms.Any(r => r.roomType == RoomType.Engine);
        if (!hasEngine)
        {
            return new ValidationResult(ShipValidationError.MissingRequiredRoom, "shipvalidation.error.noengineroom");
        }

        // 전력실이 있는지 확인
        bool hasPower = rooms.Any(r => r.roomType == RoomType.Power);
        if (!hasPower)
        {
            return new ValidationResult(ShipValidationError.MissingRequiredRoom, "shipvalidation.error.nopowerroom");
        }

        return new ValidationResult(ShipValidationError.None, "shipvalidation.error.none");
    }

    /// <summary>
    /// 모든 방이 서로 연결되어 있는지 확인합니다.
    /// BFS(너비 우선 탐색)를 사용합니다.
    /// </summary>
    private ValidationResult CheckAllRoomsConnected(List<Room> rooms)
    {
        // 방문한 방을 추적하는 HashSet
        HashSet<Room> visitedRooms = new HashSet<Room>();

        // BFS에 사용할 큐
        Queue<Room> queue = new Queue<Room>();

        // 첫 번째 방을 시작점으로 설정
        queue.Enqueue(rooms[0]);
        visitedRooms.Add(rooms[0]);

        while (queue.Count > 0)
        {
            Room currentRoom = queue.Dequeue();

            // 현재 방에서 접근 가능한 모든 방 탐색
            List<Room> connectedRooms = GetConnectedRooms(currentRoom, rooms);

            foreach (Room connectedRoom in connectedRooms)
            {
                if (!visitedRooms.Contains(connectedRoom))
                {
                    visitedRooms.Add(connectedRoom);
                    queue.Enqueue(connectedRoom);
                }
            }
        }

        // 모든 방을 방문했는지 확인
        if (visitedRooms.Count != rooms.Count)
        {
            // 방문하지 못한 방 찾기
            List<Room> unreachableRooms = rooms.Where(r => !visitedRooms.Contains(r)).ToList();
            string roomNames = string.Join(", ", unreachableRooms.Select(r => r.name));
            return new ValidationResult(ShipValidationError.DisconnectedRooms, "shipvalidation.error.isolatedroom");
        }

        return new ValidationResult(ShipValidationError.None, "shipvalidation.error.none");
    }

    /// <summary>
    /// 모든 문이 적절히 연결되어 있는지 확인합니다.
    /// </summary>
    private ValidationResult CheckAllDoorsConnected(List<Room> rooms)
    {
        // 각 문의 배치 검증
        foreach (Room room in rooms)
        {
            // 복도는 문이 없으므로 건너뜀
            if (room.roomType == RoomType.Corridor) continue;

            foreach (Door door in GetDoorsFromRoom(room))
            {
                // 문이 연결된 방이 있는지 확인
                if (!IsDoorConnectedToAnotherRoom(door, room, rooms))
                {
                    return new ValidationResult(ShipValidationError.InvalidDoorConnection, "shipvalidation.error.unconnecteddoor");
                }
            }
        }

        return new ValidationResult(ShipValidationError.None, "shipvalidation.error.none");
    }

    /// <summary>
    /// 무기 배치가 적절한지 확인합니다.
    /// 모든 무기는 방에 붙어있어야 하고, 발사 경로에 방해물이 없어야 합니다.
    /// </summary>
    private ValidationResult CheckWeaponPlacement(Ship ship)
    {
        List<Room> rooms = ship.GetAllRooms();
        List<ShipWeapon> weapons = ship.GetAllWeapons();

        if (weapons == null || weapons.Count == 0)
        {
            // 무기가 없으면 검증 통과
            return new ValidationResult(ShipValidationError.None, "shipvalidation.error.none");
        }

        // 방과 무기의 그리드 맵 구축
        Dictionary<Vector2Int, object> gridMap = BuildGridMap(rooms, weapons);

        foreach (ShipWeapon weapon in weapons)
        {
            Vector2Int weaponPos = weapon.GetGridPosition();

            // 1. 무기가 방에 붙어있는지 확인
            if (!IsWeaponAttachedToShip(weapon, weaponPos, gridMap))
            {
                return new ValidationResult(ShipValidationError.WeaponPlacementError,
                    "shipvalidation.error.weaponnotattached");
            }

            // 2. 무기의 발사 경로(오른쪽)에 방해물이 없는지 확인
            if (!IsWeaponFiringPathClear(weapon, weaponPos, gridMap))
            {
                return new ValidationResult(ShipValidationError.WeaponPlacementError,
                    "shipvalidation.error.weaponfirepathblocked");
            }
        }

        return new ValidationResult(ShipValidationError.None, "shipvalidation.error.none");
    }

    /// <summary>
    /// 방과 무기를 포함한 그리드 맵을 구축합니다.
    /// </summary>
    private Dictionary<Vector2Int, object> BuildGridMap(List<Room> rooms, List<ShipWeapon> weapons)
    {
        Dictionary<Vector2Int, object> gridMap = new Dictionary<Vector2Int, object>();

        // 방 그리드 맵 구축
        foreach (Room room in rooms)
        {
            Vector2Int size = room.GetSize();
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Vector2Int position = room.position + new Vector2Int(x, y);
                    gridMap[position] = room;
                }
            }
        }

        // 무기 그리드 맵 구축 (무기는 항상 1x1 크기)
        foreach (ShipWeapon weapon in weapons)
        {
            Vector2Int weaponPos = weapon.GetGridPosition();
            gridMap[weaponPos] = weapon;
        }

        return gridMap;
    }

    /// <summary>
    /// 무기가 배에 붙어있는지 확인합니다.
    /// </summary>
    private bool IsWeaponAttachedToShip(ShipWeapon weapon, Vector2Int weaponPos, Dictionary<Vector2Int, object> gridMap)
    {
        // 무기의 위치와 장착 방향 확인
        Vector2Int adjacentPos;

        // 장착 방향에 따른 인접 타일 위치 계산
        switch (weapon.GetAttachedDirection())
        {
            case ShipWeaponAttachedDirection.East:
                adjacentPos = new Vector2Int(weaponPos.x - 1, weaponPos.y); // 왼쪽 타일
                break;
            case ShipWeaponAttachedDirection.North:
                adjacentPos = new Vector2Int(weaponPos.x, weaponPos.y - 1); // 아래쪽 타일
                break;
            case ShipWeaponAttachedDirection.South:
                adjacentPos = new Vector2Int(weaponPos.x, weaponPos.y + 1); // 위쪽 타일
                break;
            default:
                return false;
        }

        // 장착 방향 쪽에 방이 있는지 확인
        if (gridMap.TryGetValue(adjacentPos, out object obj) && obj is Room)
        {
            return true; // 방에 붙어있음
        }

        return false; // 방에 붙어있지 않음
    }

    /// <summary>
    /// 무기의 발사 경로에 방해물이 없는지 확인합니다.
    /// </summary>
    private bool IsWeaponFiringPathClear(ShipWeapon weapon, Vector2Int weaponPos, Dictionary<Vector2Int, object> gridMap)
    {
        // 무기 오른쪽의 모든 그리드 위치를 검사
        for (int x = weaponPos.x + 1; x < 100; x++) // 충분히 큰 범위로 검사
        {
            Vector2Int checkPos = new Vector2Int(x, weaponPos.y);

            // 해당 위치에 방이나 다른 무기가 있는지 확인
            if (gridMap.ContainsKey(checkPos))
            {
                // 방해물 발견
                return false;
            }
        }

        // 발사 경로에 방해물 없음
        return true;
    }

    /// <summary>
    /// 현재 방에서 접근 가능한 다른 방들을 반환합니다.
    /// </summary>
    private List<Room> GetConnectedRooms(Room currentRoom, List<Room> allRooms)
    {
        List<Room> connectedRooms = new List<Room>();

        // 현재 방이 복도인 경우 특별 처리
        if (currentRoom.roomType == RoomType.Corridor)
        {
            // 복도는 모든 방향으로 연결 가능 (복도는 항상 1x1 크기)
            Vector2Int corridorPos = currentRoom.position;

            // 복도의 각 방향 검사 (North, East, South, West)
            foreach (DoorDirection direction in System.Enum.GetValues(typeof(DoorDirection)))
            {
                // 해당 방향의 인접 위치 계산 (복도는 1x1이므로 직접 계산)
                Vector2Int adjacentPos = GetAdjacentPosition(corridorPos, direction);

                // 인접 위치에 있는 방 찾기
                foreach (Room otherRoom in allRooms)
                {
                    if (otherRoom == currentRoom) continue;

                    if (IsRoomAtPosition(otherRoom, adjacentPos))
                    {
                        // 일반 방인 경우 반대 방향의 문이 있는지 확인
                        if (otherRoom.roomType != RoomType.Corridor)
                        {
                            Door otherDoor = otherRoom.GetDoorInDirection(GetOppositeDirection(direction));
                            if (otherDoor != null)
                            {
                                connectedRooms.Add(otherRoom);
                            }
                        }
                        else
                        {
                            // 다른 복도인 경우 무조건 연결 가능
                            connectedRooms.Add(otherRoom);
                        }
                    }
                }
            }
        }
        else
        {
            // 일반 방인 경우 문을 통해 연결된 방 확인
            List<Door> doors = GetDoorsFromRoom(currentRoom);

            foreach (Door door in doors)
            {
                // 해당 문과 연결된 다른 방 찾기
                Room connectedRoom = FindConnectedRoomThroughDoor(door, currentRoom, allRooms);
                if (connectedRoom != null && connectedRoom != currentRoom)
                {
                    connectedRooms.Add(connectedRoom);
                }
            }
        }

        return connectedRooms;
    }

    /// <summary>
    /// 방에서 모든 문을 가져옵니다.
    /// </summary>
    private List<Door> GetDoorsFromRoom(Room room)
    {
        return room.GetConnectedDoors();
    }

    /// <summary>
    /// 주어진 문을 통해 연결된 다른 방을 찾습니다.
    /// </summary>
    private Room FindConnectedRoomThroughDoor(Door door, Room currentRoom, List<Room> allRooms)
    {
        // 문의 월드 위치 계산
        Vector2Int doorGridPosition = CalculateDoorWorldPosition(door, currentRoom);
        DoorDirection doorDirection = door.Direction;

        // 문의 방향에 따라 연결될 수 있는 다른 방의 위치 계산
        Vector2Int adjacentPosition = GetAdjacentPosition(doorGridPosition, doorDirection);

        // 반대 방향 계산
        DoorDirection oppositeDirection = GetOppositeDirection(doorDirection);

        // 인접한 위치에 있는 방 찾기
        foreach (Room otherRoom in allRooms)
        {
            if (otherRoom == currentRoom) continue;

            // 방이 인접해 있는지 확인
            if (IsRoomAtPosition(otherRoom, adjacentPosition))
            {
                // 복도인 경우 특별 처리 - 복도는 문이 없어도 연결 가능
                if (otherRoom.roomType == RoomType.Corridor)
                {
                    return otherRoom; // 복도는 항상 연결 가능
                }

                // 일반 방의 경우 반대 방향의 문이 있는지 확인
                Door otherDoor = otherRoom.GetDoorInDirection(oppositeDirection);
                if (otherDoor != null)
                {
                    return otherRoom;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 문이 다른 방과 연결되어 있는지 확인합니다.
    /// </summary>
    private bool IsDoorConnectedToAnotherRoom(Door door, Room currentRoom, List<Room> allRooms)
    {
        return FindConnectedRoomThroughDoor(door, currentRoom, allRooms) != null;
    }

    /// <summary>
    /// 문의 월드 위치를 계산합니다.
    /// </summary>
    private Vector2Int CalculateDoorWorldPosition(Door door, Room room)
    {
        // 방의 그리드 위치와 문의 상대 위치를 합산
        Vector2Int doorPosition = room.position + door.OriginalGridPosition;

        // 방의 회전을 고려하여 최종 위치 계산은 Room 클래스에서 처리됨
        return doorPosition;
    }

    /// <summary>
    /// 주어진 위치와 방향에 따라 인접한 위치를 계산합니다.
    /// </summary>
    private Vector2Int GetAdjacentPosition(Vector2Int position, DoorDirection direction)
    {
        switch (direction)
        {
            case DoorDirection.North:
                return new Vector2Int(position.x, position.y + 1);
            case DoorDirection.East:
                return new Vector2Int(position.x + 1, position.y);
            case DoorDirection.South:
                return new Vector2Int(position.x, position.y - 1);
            case DoorDirection.West:
                return new Vector2Int(position.x - 1, position.y);
            default:
                return position;
        }
    }

    /// <summary>
    /// 주어진 방향의 반대 방향을 반환합니다.
    /// </summary>
    private DoorDirection GetOppositeDirection(DoorDirection direction)
    {
        switch (direction)
        {
            case DoorDirection.North:
                return DoorDirection.South;
            case DoorDirection.East:
                return DoorDirection.West;
            case DoorDirection.South:
                return DoorDirection.North;
            case DoorDirection.West:
                return DoorDirection.East;
            default:
                return direction;
        }
    }

    /// <summary>
    /// 주어진 위치에 방이 있는지 확인합니다.
    /// </summary>
    private bool IsRoomAtPosition(Room room, Vector2Int position)
    {
        // 복도인 경우 단순 위치 체크 (1x1 크기)
        if (room.roomType == RoomType.Corridor)
        {
            return room.position.x == position.x && room.position.y == position.y;
        }

        // 일반 방인 경우 영역 체크
        Vector2Int roomSize = room.GetSize();

        // 방의 각 타일 위치 확인
        for (int x = 0; x < roomSize.x; x++)
        {
            for (int y = 0; y < roomSize.y; y++)
            {
                Vector2Int tilePosition = room.position + new Vector2Int(x, y);
                if (tilePosition == position)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 월드 좌표를 그리드 좌표로 변환합니다.
    /// </summary>
    private Vector2Int WorldToGridPosition(Vector2 worldPosition)
    {
        // 월드 좌표에서 그리드 좌표로 변환하는 로직
        // 구현은 게임의 그리드 시스템에 따라 다를 수 있음
        // 예시: 그리드 크기가 1x1인 경우
        return new Vector2Int(Mathf.RoundToInt(worldPosition.x), Mathf.RoundToInt(worldPosition.y));
    }
    #endregion

    #region bpShip, bpRoom 버전
    /// <summary>
    /// 설계도 방 배치, 무기 배치 유효성 검사 클래스
    /// 설계도에 설치한 모든 방이 서로 연결되어 있고, 각 방의 문이 서로 마주하여 정상 연결되어 있는지 확인
    /// </summary>
    /// <param name="bpShip"></param>
    /// <returns></returns>
    public ValidationResult ValidateBlueprintLayout(BlueprintShip bpShip)
    {
        // 설계도에 설치된 모든 방 list로 호출
        List<BlueprintRoom> bpRooms = bpShip.PlacedBlueprintRooms.ToList();

        // 설치된 방 0
        if (bpRooms == null || bpRooms.Count == 0)
            return new ValidationResult(ShipValidationError.NoRoom, "shipvalidation.error.noroom");

        // 모든 방 연결 확인
        ValidationResult bpRoomConnectivity = CheckAllBPRoomsConnected(bpRooms);
        if (!bpRoomConnectivity.IsValid)
            return bpRoomConnectivity;

        // 문 연결 확인
        ValidationResult bpDoorConnectivity = CheckAllBPDoorsConnected(bpRooms);
        if (!bpDoorConnectivity.IsValid)
            return bpDoorConnectivity;

        // 정상 설계
        return new ValidationResult(ShipValidationError.None, "shipvalidation.error.none");
    }

    /// <summary>
    /// 설계도의 모든 방이 연결되어 있는지 검사 (BFS)
    /// </summary>
    /// <param name="bpRooms"></param>
    /// <returns></returns>
    private ValidationResult CheckAllBPRoomsConnected(List<BlueprintRoom> bpRooms)
    {
        // 방문한 방 추적 HashSet
        HashSet<BlueprintRoom> visited = new HashSet<BlueprintRoom>();

        // BFS 사용 queue
        Queue<BlueprintRoom> queue = new Queue<BlueprintRoom>();

        // 첫 번째 방을 시작점으로 설정
        queue.Enqueue(bpRooms[0]);
        visited.Add(bpRooms[0]);

        // 모든 연결된 방 순회
        while (queue.Count > 0)
        {
            BlueprintRoom current = queue.Dequeue();

            // dequeue() 한 방에 인접한 모든 방 호출
            List<BlueprintRoom> connected = GetConnectedBPRooms(current, bpRooms);

            foreach (BlueprintRoom neighbor in connected)
            {
                // 아직 방문하지 않은 방만 영토 확장
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        // 남은 방이 있을 경우, 연결 안 된 방임
        if (visited.Count != bpRooms.Count)
        {
            // 방문하지 않은 방 이름 저장 (tooltip에 띄우기 등 고려)
            List<BlueprintRoom> unreachableBPRooms = bpRooms.Where(r => !visited.Contains(r)).ToList();
            string unreachableBPRoomNames = string.Join(", ", unreachableBPRooms.Select(r => r.name));
            return new ValidationResult(ShipValidationError.DisconnectedRooms, "shipvalidation.error.isolatedroom");
        }

        // 정상 설계
        return new ValidationResult(ShipValidationError.None, "shipvalidation.error.none");
    }

    /// <summary>
    /// 인접한 설계도 방 반환
    /// </summary>
    /// <param name="current"></param>
    /// <param name="allBPRooms"></param>
    /// <returns></returns>
    private List<BlueprintRoom> GetConnectedBPRooms(BlueprintRoom current, List<BlueprintRoom> allBPRooms)
    {
        List<BlueprintRoom> connected = new List<BlueprintRoom>();

        foreach (BlueprintRoom other in allBPRooms)
        {
            if (other == current)
                continue;

            bool isConnected = false;

            // 현재 bpRoom의 점유 타일과 각 bpRoom의 점유 타일 비교
            // 한 칸이라도 인접하면 다음 방으로 넘어가는 방식
            foreach (Vector2Int currentTile in current.occupiedTiles)
            {
                foreach (Vector2Int otherTile in other.occupiedTiles)
                {
                    if (Vector2Int.Distance(currentTile, otherTile) == 1)
                    {
                        connected.Add(other);
                        isConnected = true;
                        break;
                    }
                }
                if (isConnected)
                    break;
            }
        }

        return connected;
    }

    /// <summary>
    /// 인접한 방 사이 문이 마주보는지 여부
    /// </summary>
    /// <param name="bpRooms"></param>
    /// <returns></returns>
    private ValidationResult CheckAllBPDoorsConnected(List<BlueprintRoom> bpRooms)
    {
        foreach (BlueprintRoom bpRoom in bpRooms)
        {
            List<Vector2Int> bpDoors = bpRoom.bpRoomData.GetDoorPositions(bpRoom.bpLevelIndex, bpRoom.bpPosition, bpRoom.bpRotation);

            foreach (Vector2Int doorPos in bpDoors)
            {
                bool connected = false;

                foreach (BlueprintRoom other in bpRooms)
                {
                    if (other == bpRoom)
                        continue;

                    List<Vector2Int> otherBPDoors = other.bpRoomData.GetDoorPositions(other.bpLevelIndex, other.bpPosition, other.bpRotation);

                    if (otherBPDoors.Contains(doorPos))
                    {
                        connected = true;
                        break;
                    }
                }

                if (!connected)
                    return new ValidationResult(ShipValidationError.InvalidDoorConnection, "shipvalidation.error.unconnecteddoor");
            }
        }

        return new ValidationResult(ShipValidationError.None, "shipvalidation.error.none");
    }

    #endregion
}
