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
    /// <summary>
    /// 함선 배치의 유효성 검사
    /// </summary>
    /// <param name="ship"></param>
    /// <returns></returns>
    public ValidationResult ValidateShipLayout(Ship ship)
    {
        // 모든 방 순회
        List<Room> rooms = ship.GetAllRooms();

        // 설치된 방 없음
        if (rooms == null || rooms.Count == 0)
            return new ValidationResult(ShipValidationError.NoRoom, "shipvalidation.error.noroom");

        // // 필수 방 배치 확인 (조종실, 엔진실, 전력실, 텔레포트실, 생활 시설)
        // ValidationResult requiredRoomsCheck = CheckRequiredRooms(rooms);
        // if (!requiredRoomsCheck.IsValid)
        //     return requiredRoomsCheck;
        //
        // // 모든 방의 연결성 확인 (선원이 모든 방을 순회하고 원위치로 복귀할 수 있는가)
        // ValidationResult pathConnectivity = CheckAllRoomsPathConnectedByDoors(rooms, ship.GetGridSize());
        // if (!pathConnectivity.IsValid)
        //     return pathConnectivity;

        // 무기와 방의 연결성 검사
        ValidationResult weaponConnectivityCheck = CheckWeaponConnectivity(ship);
        if (!weaponConnectivityCheck.IsValid)
            return weaponConnectivityCheck;

        // 무기 방향과 방 연결 검사
        ValidationResult weaponAttachmentCheck = ValidateWeaponAttachmentDirection(ship);
        if (!weaponAttachmentCheck.IsValid)
            return weaponAttachmentCheck;

        // 무기의 오른쪽 방향 검사
        ValidationResult weaponCheck = ValidateWeaponPlacement(ship);
        if (!weaponCheck.IsValid)
            return weaponCheck;

        // 정상 연결
        return new ValidationResult(ShipValidationError.None, "shipvalidation.error.none");
    }

    /// <summary>
    /// 필수 방이 존재하는지 확인합니다.
    /// </summary>
    private ValidationResult CheckRequiredRooms(List<Room> rooms)
    {
        // 조종실 여부
        bool hasCockpitBP = rooms.Any(r => r.roomType == RoomType.Cockpit);
        if (!hasCockpitBP)
            return new ValidationResult(ShipValidationError.MissingRequiredRoom, "shipvalidation.error.nocockpit");

        // 엔진실 여부
        bool hasEngineBP = rooms.Any(r => r.roomType == RoomType.Engine);
        if (!hasEngineBP)
            return new ValidationResult(ShipValidationError.MissingRequiredRoom, "shipvalidation.error.noengineroom");

        // 전력실 여부
        bool hasPowerBP = rooms.Any(r => r.roomType == RoomType.Power);
        if (!hasPowerBP)
            return new ValidationResult(ShipValidationError.MissingRequiredRoom, "shipvalidation.error.nopowerroom");

        // 텔레포트실 여부
        bool hasTeleportBP = rooms.Any(r => r.roomType == RoomType.Teleporter);
        if (!hasTeleportBP)
            return new ValidationResult(ShipValidationError.MissingRequiredRoom, "shipvalidation.error.noteleportroom");

        // 생활 시설 여부
        bool hasCrewQuartersBP = rooms.Any(r => r.roomType == RoomType.CrewQuarters);
        if (!hasCrewQuartersBP)
            return new ValidationResult(ShipValidationError.MissingRequiredRoom,
                "shipvalidation.error.nocrewquartersroom");

        // 모두 정상
        return new ValidationResult(ShipValidationError.None, "shipvalidation.error.none");
    }

    /// <summary>
    /// 무기의 방향과 방 연결 상태를 검사합니다.
    /// 각 무기의 AttachedDirection에 따라 적절한 위치에 방이 있어야 합니다.
    /// </summary>
    /// <param name="ship">검사할 함선</param>
    /// <returns>검증 결과</returns>
    public ValidationResult ValidateWeaponAttachmentDirection(Ship ship)
    {
        // 함선의 모든 무기 가져오기
        List<ShipWeapon> weapons = ship.GetAllWeapons();

        // 무기가 없으면 유효함
        if (weapons == null || weapons.Count == 0)
            return new ValidationResult(ShipValidationError.None, "shipvalidation.error.none");

        // 모든 방 가져오기
        List<Room> rooms = ship.GetAllRooms();

        // 그리드 크기 가져오기
        Vector2Int gridSize = ship.GetGridSize();

        // 방이 점유하는 모든 타일 위치를 저장하는 집합
        HashSet<Vector2Int> roomTiles = new();

        // 모든 방이 점유하는 타일 수집
        foreach (Room room in rooms)
        {
            Vector2Int roomSize = room.GetSize();
            Vector2Int roomPos = room.position;

            // 방 크기에 따라 점유하는 모든 타일 추가
            for (int x = 0; x < roomSize.x; x++)
            for (int y = 0; y < roomSize.y; y++)
            {
                Vector2Int tile = roomPos + new Vector2Int(x, y);
                roomTiles.Add(tile);
            }
        }

        // 각 무기에 대해 검사
        foreach (ShipWeapon weapon in weapons)
        {
            // 무기 위치와 방향
            Vector2Int weaponPos = weapon.GetGridPosition();
            ShipWeaponAttachedDirection direction = weapon.GetAttachedDirection();

            // 확인할 타일 (방이 있어야 하는 위치)
            Vector2Int checkTile;

            switch (direction)
            {
                case ShipWeaponAttachedDirection.North:
                    // 기준점 아래에 방이 있어야 함
                    checkTile = new Vector2Int(weaponPos.x, weaponPos.y - 1);
                    break;

                case ShipWeaponAttachedDirection.East:
                    // 기준점 왼쪽에 방이 있어야 함
                    checkTile = new Vector2Int(weaponPos.x - 1, weaponPos.y);
                    break;

                case ShipWeaponAttachedDirection.South:
                    // 기준점 위에 방이 있어야 함
                    checkTile = new Vector2Int(weaponPos.x, weaponPos.y + 1);
                    break;

                default:
                    // 알 수 없는 방향 - 유효하지 않음
                    return new ValidationResult(
                        ShipValidationError.WeaponPlacementError,
                        "shipvalidation.error.invalidweaponattachment"
                    );
            }

            // 해당 위치에 방이 있는지 확인
            if (!roomTiles.Contains(checkTile))
                return new ValidationResult(
                    ShipValidationError.DisconnectedWeapons,
                    "shipvalidation.error.weaponnotattachedtoroom"
                );
        }

        // 모든 무기 방향 및 연결 검사 통과
        return new ValidationResult(ShipValidationError.None, "shipvalidation.error.none");
    }

    /// <summary>
    /// BFS + DFS 이용하여 방과 방의 연결성 및 이동 가능성 확인
    /// </summary>
    /// <param name="rooms">검사할 방 목록</param>
    /// <param name="gridSize">그리드 크기</param>
    /// <returns>검증 결과</returns>
    private ValidationResult CheckAllRoomsPathConnectedByDoors(List<Room> rooms, Vector2Int gridSize)
    {
        // BFS 방문 추적용
        HashSet<Room> visitedRooms = new();
        // BFS Queue
        Queue<Room> queue = new();
        // DFS 기반 복도 방향별 방문 추적
        HashSet<(Room, DoorDirection)> visitedCorridorDirections = new();

        // 첫 시작 방 지정 (BFS)
        Room startRoom = rooms[0];
        queue.Enqueue(startRoom);
        visitedRooms.Add(startRoom);

        int i = 0;

        // BFS + DFS 탐색
        while (queue.Count > 0)
        {
            // BFS 현재 방 dequeue
            Room current = queue.Dequeue();
            Debug.Log($"{i++} : {current} 방 진입 currentroompos : {current.position}");

            // 현재 방의 문 방향 리스트 (회전각 반영)
            List<DoorPosition> doors = current.GetRoomData()
                .GetDoorPositionsWithDirection(current.GetCurrentLevel(), current.position, current.currentRotation);

            // 현재 방의 모든 문에 대해 검사 (현재 방 : 복도 or 일반 방 무관)
            foreach (DoorPosition door in doors)
            {
                Debug.Log(
                    $"currentroompos : {current.position}, doorpos : {door.position}, doordir : {door.direction}");

                // 인접 타일, 인접 방 검색
                Vector2Int neighborTile = GetAdjacentTile(door.position, door.direction);

                // 그리드 범위 검사 추가
                if (neighborTile.x < 0 || neighborTile.y < 0 ||
                    neighborTile.x >= gridSize.x || neighborTile.y >= gridSize.y)
                    // 그리드 범위를 벗어난 경우 건너뜀
                    continue;

                Room neighborRoom = rooms.FirstOrDefault(r => r.OccupiesTile(neighborTile));

                if (neighborRoom != null)
                    Debug.Log($"{neighborRoom}, pos: {neighborRoom.position}");

                // 이미 방문한 방 통과 (BFS)
                if (neighborRoom == null || visitedRooms.Contains(neighborRoom))
                    continue;

                // 상황 1 : (반대편 방 == 복도) 항상 연결 (서로 맞닿아 있으면 무조건 이동 가능)
                if (neighborRoom.GetRoomType() == RoomType.Corridor)
                {
                    visitedRooms.Add(neighborRoom);
                    queue.Enqueue(neighborRoom);
                    continue;
                }

                // 상황 2 : (반대편 방 == 일반 방) 문 위치 일치, 방향 서로 반대 시 연결
                List<DoorPosition> neighborDoors = neighborRoom.GetRoomData()
                    .GetDoorPositionsWithDirection(neighborRoom.GetCurrentLevel(), neighborRoom.position,
                        neighborRoom.currentRotation);
                DoorDirection opposite = GetOppositeDoorDirection(door.direction);

                Debug.Log($"dir : {door.direction}, 현재 문의 반대방향 (추구) : {opposite}");

                bool isConnected = false;

                // 방의 모든 문에 대해 검사하고 한 개라도 조건 불만족 => fail 되도록 확장 필요
                // 지금은 각각에 대해서만 검사하고 있지만 결국 모든 문에 대해서 조건1,2가 만족해야 isconnected
                foreach (DoorPosition ndoor in neighborDoors)
                {
                    // 월드 좌표로 변환한 반대편 방 문 방향 (현실)
                    DoorDirection nWorldDir = ndoor.direction;

                    Debug.Log($"반대편 방 문 방향 (현실) : {nWorldDir}");

                    // 조건 1 : 인접한 위치
                    bool positionMatch = DoorPositionMatch(door.position, door.direction, ndoor.position, nWorldDir,
                        gridSize);

                    // 조건 2 : 문끼리 서로 마주봄
                    bool directionMatch = opposite == nWorldDir;

                    Debug.Log($"posMatch : {positionMatch}, dirMatch : {directionMatch}");

                    // 조건 1 & 2 만족 : 연결 보장
                    if (positionMatch && directionMatch)
                    {
                        isConnected = true;
                        break; // 하나의 문만 연결성 보장되면 일단 pass
                    }
                }

                // 연결되어 있으니 이동
                if (isConnected)
                {
                    Debug.Log($"연결 O - 위치 : {neighborTile}, 문 방향 : {opposite}");
                    visitedRooms.Add(neighborRoom);
                    queue.Enqueue(neighborRoom);
                }
            }
        }

        // 디버깅 시각화
        ShipPathDebugVisualizer visualizer = GetOrCreateVisualizer();

        visualizer.startRoom = startRoom;
        visualizer.visitedRooms = visitedRooms.ToList();
        visualizer.allRooms = rooms;

        Debug.LogWarning($"visited : {visitedRooms.Count}, rooms : {rooms.Count}");
        // 방문한 방 수 != 실제 설치된 방 수
        if (visitedRooms.Count != rooms.Count)
            return new ValidationResult(ShipValidationError.DisconnectedRooms, "shipvalidation.error.isolatedroom");

        // 정상 설계
        return new ValidationResult(ShipValidationError.None, "shipvalidation.error.none");
    }

    /// <summary>
    /// 무기 설치 위치 유효성 검사
    /// 무기 우측(x 증가 방향)에 그리드 끝까지 방이나 다른 무기가 있는지 확인
    /// </summary>
    /// <param name="ship">검사할 함선</param>
    /// <returns>검증 결과</returns>
    public ValidationResult ValidateWeaponPlacement(Ship ship)
    {
        // 함선의 모든 무기 가져오기
        List<ShipWeapon> weapons = ship.GetAllWeapons();

        // 무기가 없으면 유효함
        if (weapons == null || weapons.Count == 0)
            return new ValidationResult(ShipValidationError.None, "shipvalidation.error.none");

        // 모든 방 가져오기
        List<Room> rooms = ship.GetAllRooms();

        // 그리드 크기 가져오기
        Vector2Int gridSize = ship.GetGridSize();

        // 각 무기에 대해 검사
        foreach (ShipWeapon weapon in weapons)
        {
            // 무기 위치
            Vector2Int weaponPosition = weapon.GetGridPosition();

            // 무기 오른쪽(x 증가) 방향의 모든 타일 검사
            for (int x = weaponPosition.x + 1; x < gridSize.x; x++)
            {
                Vector2Int checkPosition = new(x, weaponPosition.y);

                // 해당 위치에 방이 있는지 확인
                bool roomExists = rooms.Any(room => room.OccupiesTile(checkPosition));

                // 해당 위치에 다른 무기가 있는지 확인
                bool weaponExists = weapons.Any(w => w.GetGridPosition() == checkPosition);

                // 무기 오른쪽에 방이나 무기가 있으면 유효하지 않음
                if (roomExists || weaponExists)
                    return new ValidationResult(
                        ShipValidationError.WeaponFrontObject,
                        "shipvalidation.error.weaponfrontobject"
                    );
            }
        }

        // 모든 무기 배치가 유효함
        return new ValidationResult(ShipValidationError.None, "shipvalidation.error.none");
    }

    /// <summary>
    /// 모든 무기가 방과 직접 또는 간접적으로 연결되어 있는지 확인합니다.
    /// BFS를 사용하여 무기와 방의 연결성을 검사합니다.
    /// </summary>
    /// <param name="ship">검사할 함선</param>
    /// <returns>검증 결과</returns>
    public ValidationResult CheckWeaponConnectivity(Ship ship)
    {
        // 함선의 모든 무기 가져오기
        List<ShipWeapon> weapons = ship.GetAllWeapons();

        // 무기가 없으면 유효함
        if (weapons == null || weapons.Count == 0)
            return new ValidationResult(ShipValidationError.None, "shipvalidation.error.none");

        // 모든 방 가져오기
        List<Room> rooms = ship.GetAllRooms();

        // 그리드 크기 가져오기
        Vector2Int gridSize = ship.GetGridSize();

        // 방이 점유하는 모든 타일 위치를 저장하는 집합
        HashSet<Vector2Int> roomTiles = new();

        // 모든 방이 점유하는 타일 수집
        foreach (Room room in rooms)
        {
            Vector2Int roomSize = room.GetSize();
            Vector2Int roomPos = room.position;

            // 방 크기에 따라 점유하는 모든 타일 추가
            for (int x = 0; x < roomSize.x; x++)
            for (int y = 0; y < roomSize.y; y++)
            {
                Vector2Int tile = roomPos + new Vector2Int(x, y);
                roomTiles.Add(tile);
            }
        }

        // 방문한 무기를 추적하는 집합
        HashSet<ShipWeapon> visitedWeapons = new();

        // BFS를 위한 큐
        Queue<ShipWeapon> queue = new();

        // 방에 직접 인접한 무기를 시작점으로 큐에 추가
        foreach (ShipWeapon weapon in weapons)
        {
            Vector2Int weaponPos = weapon.GetGridPosition();

            // 무기가 방에 인접한지 확인
            bool isAdjacentToRoom = IsAdjacentToRoomTiles(weaponPos, roomTiles, gridSize);

            if (isAdjacentToRoom)
            {
                queue.Enqueue(weapon);
                visitedWeapons.Add(weapon);
            }
        }

        // BFS로 연결된 모든 무기 탐색
        while (queue.Count > 0)
        {
            ShipWeapon currentWeapon = queue.Dequeue();
            Vector2Int currentPos = currentWeapon.GetGridPosition();

            // 현재 무기에 인접한 다른 무기 탐색
            foreach (ShipWeapon otherWeapon in weapons)
            {
                if (visitedWeapons.Contains(otherWeapon)) continue;

                Vector2Int otherPos = otherWeapon.GetGridPosition();

                // 인접 여부 확인
                if (AreAdjacent(currentPos, otherPos, gridSize))
                {
                    queue.Enqueue(otherWeapon);
                    visitedWeapons.Add(otherWeapon);
                }
            }
        }

        // 모든 무기가 방문되었는지 확인
        if (visitedWeapons.Count != weapons.Count)
            // 연결되지 않은 무기가 존재
            return new ValidationResult(
                ShipValidationError.DisconnectedWeapons,
                "shipvalidation.error.disconnectedweapons"
            );

        // 모든 무기가 연결됨
        return new ValidationResult(ShipValidationError.None, "shipvalidation.error.none");
    }

    /// <summary>
    /// 두 위치가 서로 인접해 있는지 확인합니다.
    /// </summary>
    /// <param name="pos1">첫 번째 위치</param>
    /// <param name="pos2">두 번째 위치</param>
    /// <param name="gridSize">그리드 크기</param>
    /// <returns>인접 여부</returns>
    private bool AreAdjacent(Vector2Int pos1, Vector2Int pos2, Vector2Int gridSize)
    {
        // 그리드 범위 확인
        if (pos1.x < 0 || pos1.y < 0 || pos1.x >= gridSize.x || pos1.y >= gridSize.y ||
            pos2.x < 0 || pos2.y < 0 || pos2.x >= gridSize.x || pos2.y >= gridSize.y)
            return false;

        // 인접 여부 확인 (상하좌우)
        int dx = Mathf.Abs(pos1.x - pos2.x);
        int dy = Mathf.Abs(pos1.y - pos2.y);

        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
    }

    /// <summary>
    /// 위치가 방 타일에 인접해 있는지 확인합니다.
    /// </summary>
    /// <param name="pos">확인할 위치</param>
    /// <param name="roomTiles">방이 점유하는 타일 집합</param>
    /// <param name="gridSize">그리드 크기</param>
    /// <returns>인접 여부</returns>
    private bool IsAdjacentToRoomTiles(Vector2Int pos, HashSet<Vector2Int> roomTiles, Vector2Int gridSize)
    {
        // 상하좌우 인접 타일 확인
        Vector2Int[] adjacentDirections =
        {
            new(0, 1), // 상
            new(0, -1), // 하
            new(-1, 0), // 좌
            new(1, 0) // 우
        };

        foreach (Vector2Int dir in adjacentDirections)
        {
            Vector2Int adjacentPos = pos + dir;

            // 그리드 범위 확인
            if (adjacentPos.x < 0 || adjacentPos.y < 0 ||
                adjacentPos.x >= gridSize.x || adjacentPos.y >= gridSize.y)
                continue;

            // 인접 타일이 방에 속하는지 확인
            if (roomTiles.Contains(adjacentPos))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 각 방의 문이 위치한 타일에 대해 문으로 이동 가능하도록 인접했는지 여부
    /// </summary>
    /// <param name="doorPosA">현재 방 문 위치</param>
    /// <param name="dirA">현재 방 문 방향</param>
    /// <param name="doorPosB">반대 방 문 위치</param>
    /// <param name="dirB">반대 방 문 방향</param>
    /// <param name="gridSize">그리드 크기</param>
    /// <returns></returns>
    private bool DoorPositionMatch(Vector2Int doorPosA, DoorDirection dirA, Vector2Int doorPosB, DoorDirection dirB,
        Vector2Int gridSize)
    {
        // 상호 인접 여부 검사
        Vector2Int expectedNeighborForA = doorPosA + GetDirectionOffset(dirA); // 이웃 타일 (A가 현재 타일)
        Vector2Int expectedNeighborForB = doorPosB + GetDirectionOffset(dirB); // 현재 타일

        // 그리드 범위 검사 추가
        if (expectedNeighborForA.x < 0 || expectedNeighborForA.y < 0 ||
            expectedNeighborForA.x >= gridSize.x || expectedNeighborForA.y >= gridSize.y ||
            expectedNeighborForB.x < 0 || expectedNeighborForB.y < 0 ||
            expectedNeighborForB.x >= gridSize.x || expectedNeighborForB.y >= gridSize.y)
            // 범위를 벗어난 경우
            return false;

        Debug.Log($"반대 방 문 pos : {doorPosB}, 반대 방 문 검사 : {expectedNeighborForA}");
        bool currentRoomCheck = doorPosB == expectedNeighborForA;

        Debug.Log($"현재 방 문 pos : {doorPosA}, 현재 방 문 검사 : {expectedNeighborForB}");
        bool neighborRoomCheck = doorPosA == expectedNeighborForB;

        return currentRoomCheck && neighborRoomCheck;
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
    /// 인접 타일 위치 반환
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
    private Vector2Int GetAdjacentTile(Vector2Int pos, DoorDirection dir)
    {
        return dir switch
        {
            DoorDirection.North => pos + Vector2Int.up,
            DoorDirection.South => pos + Vector2Int.down,
            DoorDirection.East => pos + Vector2Int.right,
            DoorDirection.West => pos + Vector2Int.left,
            _ => pos
        };
    }

    /// <summary>
    /// 현재 방 문의 반대 방향 반환
    /// </summary>
    /// <param name="dir">현재 방의 문 방향</param>
    /// <returns></returns>
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

    /// <summary>
    /// 디버깅 시각화 오브젝트 생성 (삭제할 예정)
    /// </summary>
    /// <returns></returns>
    private static ShipPathDebugVisualizer GetOrCreateVisualizer()
    {
        ShipPathDebugVisualizer existing = GameObject.FindFirstObjectByType<ShipPathDebugVisualizer>();
        if (existing != null)
            return existing;

        GameObject go = new("ShipPathDebugVisualizer");
        ShipPathDebugVisualizer visualizer = go.AddComponent<ShipPathDebugVisualizer>();
        go.hideFlags = HideFlags.DontSave;
        return visualizer;
    }
}
