using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선 커스터마이징을 위한 그리드 시스템을 생성하고 방 배치를 관리하는 클래스.
/// 타일 생성, 배치 가능 여부 판단, 실제 설치 등을 처리합니다.
/// </summary>
public class GridPlacer : MonoBehaviour
{
    /// <summary>
    /// 싱글턴 인스턴스입니다.
    /// </summary>
    public static GridPlacer Instance;

    /// <summary>
    /// 생성할 타일의 프리팹입니다.
    /// </summary>
    public GameObject tilePrefab;

    /// <summary>
    /// 생성된 그리드 타일들의 부모 오브젝트입니다.
    /// </summary>
    public Transform gridTiles;

    /// <summary>
    /// 실제 배치된 방들의 부모 오브젝트입니다.
    /// </summary>
    public Transform placedRooms;

    /// <summary>
    /// 그리드의 가로 길이입니다.
    /// </summary>
    public int width = 60;

    /// <summary>
    /// 그리드의 세로 길이입니다.
    /// </summary>
    public int height = 60;

    /// <summary>
    /// 각 타일의 크기입니다.
    /// </summary>
    public float tileSize = 1f;

    /// <summary>
    /// 타일의 사용 상태와 배치된 방을 기록하는 딕셔너리입니다.
    /// </summary>
    private Dictionary<Vector2Int, GameObject> occupiedCells = new Dictionary<Vector2Int, GameObject>();

    /// <summary>
    /// 배치된 방들의 목록입니다.
    /// </summary>
    private List<GameObject> placedRoomObjects = new List<GameObject>();

    /// <summary>
    /// 인스턴스를 설정합니다.
    /// </summary>
    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 게임 시작 시 그리드 타일을 생성합니다.
    /// </summary>
    private void Start()
    {
        GenerateTiles();
    }

    /// <summary>
    /// 함선 커스터마이징에 사용할 그리드 생성
    /// 모든 tile object는 gridTiles 오브젝트의 자식으로 생성됩니다.
    /// </summary>
    public void GenerateTiles()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = ShipGridManager.Instance.GridToWorldPosition(new Vector2Int(x, y));
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, gridTiles);
                tile.transform.localScale = Vector3.one * tileSize;
                tile.transform.position += new Vector3(0, 0, 17);
            }
        }
    }

    /// <summary>
    /// 지정된 영역에 방 설치가 가능한지 여부를 판단합니다.
    /// 설치 가능 시 true 반환 (초록색), 불가능 시 false 반환 (빨간색).
    /// </summary>
    /// <param name="startGrid">방 설치 시작 위치 (그리드 좌표).</param>
    /// <param name="roomSize">방의 크기 (가로, 세로).</param>
    /// <returns>설치 가능 여부.</returns>
    public bool CanPlaceRoom(Vector2Int startGrid, Vector2Int roomSize)
    {
        // 그리드 범위 확인
        if (startGrid.x < 0 || startGrid.y < 0 ||
            startGrid.x + roomSize.x > width || startGrid.y + roomSize.y > height)
            return false;

        // 다른 방과 겹치는지 확인
        for (int x = 0; x < roomSize.x; x++)
        {
            for (int y = 0; y < roomSize.y; y++)
            {
                Vector2Int checkPos = startGrid + new Vector2Int(x, y);
                if (occupiedCells.ContainsKey(checkPos))
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Room을 그리드에 배치하고 등록합니다.
    /// </summary>
    /// <param name="position">방 설치 시작 위치 (그리드 좌표).</param>
    /// <param name="size">방의 크기 (가로, 세로).</param>
    /// <param name="room">배치할 Room 컴포넌트.</param>
    /// <returns>배치 성공 여부.</returns>
    public bool RegisterPlacedRoom(Vector2Int position, Vector2Int size, Room room)
    {
        if (!CanPlaceRoom(position, size))
            return false;

        // 방 오브젝트에 위치 설정
        GameObject roomObject = room.gameObject;

        // 그리드 셀 점유 표시
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int cellPos = position + new Vector2Int(x, y);
                occupiedCells[cellPos] = roomObject;
            }
        }

        // 배치된 방 목록에 추가
        placedRoomObjects.Add(roomObject);

        // 방에 위치 정보 설정
        room.position = position;

        // Z 위치 조정 (UI보다 뒤, grid보다 앞)
        Vector3 currentPos = roomObject.transform.position;
        roomObject.transform.position = new Vector3(currentPos.x, currentPos.y, 16);

        return true;
    }

    /// <summary>
    /// 방을 그리드에서 제거합니다.
    /// </summary>
    /// <param name="room">제거할 Room 컴포넌트.</param>
    public void RemoveRoom(Room room)
    {
        if (room == null)
            return;

        GameObject roomObject = room.gameObject;

        // 방이 차지하고 있는 모든 셀에서 제거
        List<Vector2Int> cellsToRemove = new List<Vector2Int>();

        foreach (var kvp in occupiedCells)
        {
            if (kvp.Value == roomObject)
                cellsToRemove.Add(kvp.Key);
        }

        foreach (Vector2Int cell in cellsToRemove)
        {
            occupiedCells.Remove(cell);
        }

        // 배치된 방 목록에서 제거
        placedRoomObjects.Remove(roomObject);
    }

    /// <summary>
    /// 실제 지정된 위치에 방을 배치합니다. (이전 버전과의 호환성을 위해 유지)
    /// </summary>
    /// <param name="startGrid">방 설치 시작 위치 (그리드 좌표).</param>
    /// <param name="roomSize">방의 크기 (가로, 세로).</param>
    /// <param name="roomPrefab">설치할 방 프리팹.</param>
    /// <param name="rotation">회전 각도 (Z축 기준).</param>
    /// <returns>생성된 방 GameObject. 실패 시 null.</returns>
    public GameObject PlaceRoom(Vector2Int startGrid, Vector2Int roomSize, GameObject roomPrefab, int rotation)
    {
        if (!CanPlaceRoom(startGrid, roomSize))
        {
            Debug.LogWarning("설치 실패 : 이미 설치됨");
            return null;
        }

        Vector2 centerOffset = new Vector2((roomSize.x - 1) * 0.5f, (roomSize.y - 1) * 0.5f);
        Vector3 worldPos = ShipGridManager.Instance.GridToWorldPosition(startGrid) + centerOffset;
        GameObject room = Instantiate(roomPrefab, worldPos, Quaternion.identity, placedRooms);

        room.transform.Rotate(0, 0, -rotation);
        room.transform.position += new Vector3(0, 0, 16); // UI보다 뒤, grid보다 앞

        // 그리드 셀 점유 표시
        for (int x = 0; x < roomSize.x; x++)
        {
            for (int y = 0; y < roomSize.y; y++)
            {
                Vector2Int cellPos = startGrid + new Vector2Int(x, y);
                occupiedCells[cellPos] = room;
            }
        }

        // 배치된 방 목록에 추가
        placedRoomObjects.Add(room);

        return room;
    }

    /// <summary>
    /// 모든 배치된 방을 제거합니다.
    /// </summary>
    public void ClearAllRooms()
    {
        foreach (GameObject room in placedRoomObjects)
        {
            if (room != null)
                Destroy(room);
        }

        placedRoomObjects.Clear();
        occupiedCells.Clear();
    }

    /// <summary>
    /// 특정 그리드 위치의 방을 반환합니다.
    /// </summary>
    /// <param name="gridPosition">확인할 그리드 위치.</param>
    /// <returns>해당 위치의 방 GameObject. 없으면 null.</returns>
    public GameObject GetRoomAtPosition(Vector2Int gridPosition)
    {
        if (occupiedCells.TryGetValue(gridPosition, out GameObject room))
            return room;

        return null;
    }
}
