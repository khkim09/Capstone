using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 함선의 방 배치와 무기 배치가 유효한지 검증하는 클래스입니다.
/// 모든 방이 서로 연결되어 있고, 문이 적절히 배치되어 있는지 확인하며,
/// 무기의 발사 경로를 검증합니다. (무기 검사 구현 필요 - 필요 시 이전 버전 끌어와서 써야될 듯)
/// </summary>
public class ShipValidationHelper
{
    #region validation ㄹㅇ 찐찐막 울트라 최종 버전


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

        // 필수 방 배치 확인 (조종실, 엔진실, 전력실, 텔레포트실, 생활 시설)
        ValidationResult requiredRoomsCheck = CheckRequiredRooms(rooms);
        if (!requiredRoomsCheck.IsValid)
            return requiredRoomsCheck;

        // 모든 방의 연결성 확인 (선원이 모든 방을 순회하고 원위치로 복귀할 수 있는가)
        ValidationResult pathConnectivity = CheckAllRoomsPathConnectedByDoors(rooms);
        if (!pathConnectivity.IsValid)
            return pathConnectivity;

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
            return new ValidationResult(ShipValidationError.MissingRequiredRoom, "shipvalidation.error.nocrewquartersroom");

        // 모두 정상
        return new ValidationResult(ShipValidationError.None, "shipvalidation.error.none");
    }

    /// <summary>
    /// BFS + DFS 이용하여 방과 방의 연결성 및 이동 가능성 확인
    /// </summary>
    /// <param name="rooms"></param>
    /// <returns></returns>
    private ValidationResult CheckAllRoomsPathConnectedByDoors(List<Room> rooms)
    {
        // BFS 방문 추적용
        HashSet<Room> visitedRooms = new HashSet<Room>();
        // BFS Queue
        Queue<Room> queue = new Queue<Room>();
        // DFS 기반 복도 방향별 방문 추적
        HashSet<(Room, DoorDirection)> visitedCorridorDirections = new HashSet<(Room, DoorDirection)>();

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
            List<DoorPosition> doors = current.GetRoomData().GetDoorPositionsWithDirection(current.GetCurrentLevel(), current.position, current.currentRotation);

            // 현재 방의 모든 문에 대해 검사 (현재 방 : 복도 or 일반 방 무관)
            foreach (DoorPosition door in doors)
            {
                Debug.Log($"currentroompos : {current.position}, doorpos : {door.position}, doordir : {door.direction}");

                // 인접 타일, 인접 방 검색
                Vector2Int neighborTile = GetAdjacentTile(door.position, door.direction);
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
                List<DoorPosition> neighborDoors = neighborRoom.GetRoomData().GetDoorPositionsWithDirection(neighborRoom.GetCurrentLevel(), neighborRoom.position, neighborRoom.currentRotation);
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
                    bool positionMatch = DoorPositionMatch(door.position, door.direction, ndoor.position, nWorldDir);

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
    /// 각 방의 문이 위치한 타일에 대해 문으로 이동 가능하도록 인접했는지 여부
    /// </summary>
    /// <param name="doorPosA">현재 방 문 위치</param>
    /// <param name="dirA">현재 방 문 방향</param>
    /// <param name="doorPosB">반대 방 문 위치</param>
    /// <param name="dirB">반대 방 문 방향</param>
    /// <returns></returns>
    private bool DoorPositionMatch(Vector2Int doorPosA, DoorDirection dirA, Vector2Int doorPosB, DoorDirection dirB)
    {
        // 상호 인접 여부 검사
        Vector2Int expectedNeighborForA = doorPosA + GetDirectionOffset(dirA); // 이웃 타일 (A가 현재 타일)
        Vector2Int expectedNeighborForB = doorPosB + GetDirectionOffset(dirB); // 현재 타일

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

        GameObject go = new GameObject("ShipPathDebugVisualizer");
        ShipPathDebugVisualizer visualizer = go.AddComponent<ShipPathDebugVisualizer>();
        go.hideFlags = HideFlags.None;
        return visualizer;
    }

    #endregion
}
