using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선 커스터마이징을 위한 그리드 시스템을 생성하고 방 배치를 관리하는 클래스.
/// 타일 생성, 배치 가능 여부 판단, 실제 설치 등을 처리합니다.
/// </summary>
public class GridPlacer : MonoBehaviour, IWorldGridSwitcher
{
    /// <summary>
    /// 생성할 타일의 프리팹입니다.
    /// </summary>
    public GameObject tilePrefab;

    /// <summary>
    /// 생성된 그리드 타일들의 부모 오브젝트입니다.
    /// </summary>
    public Transform gridTiles;

    /// <summary>
    /// 각 타일의 크기입니다.
    /// </summary>
    public float tileSize = 1f;

    /// <summary>
    /// 배치가 이루어질 설계도
    /// </summary>
    public BlueprintShip targetBlueprintShip;

    /// <summary>
    /// 공통 roomPrefab
    /// </summary>
    public GameObject roomPrefab;

    /// <summary>
    /// 현재 도안 설계 상태에서 점유된 타일들 (모든 BlueprintRoom 기준)
    /// </summary>
    private readonly HashSet<Vector2Int> occupiedGridTiles = new();

    [SerializeField] private Vector2Int gridSize = new(60, 60);
    [SerializeField] private Vector3 gridOrigin = Vector3.zero;

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
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 pos = GridToWorldPosition(new Vector2Int(x, y));
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, gridTiles);
                tile.transform.localScale = Vector3.one * tileSize;
                tile.transform.position += new Vector3(0, 0, 17);
            }
        }
    }

    /// <summary>
    /// 그리드 좌표를 월드 좌표로 변환합니다.
    /// </summary>
    /// <param name="gridPos">변환할 그리드 좌표.</param>
    /// <returns>해당 위치의 월드 좌표.</returns>
    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return gridOrigin + new Vector3((gridPos.x + 0.5f) * tileSize, (gridPos.y + 0.5f) * tileSize, 0f);
    }

    /// <summary>
    /// 월드 좌표를 그리드 좌표로 변환합니다.
    /// </summary>
    /// <param name="worldPos">변환할 월드 좌표.</param>
    /// <returns>해당 위치의 그리드 좌표.</returns>
    public Vector2Int WorldToGridPosition(Vector2 worldPos)
    {
        Vector3 local = new Vector3(worldPos.x, worldPos.y, 0) - gridOrigin;
        return new Vector2Int(Mathf.FloorToInt(local.x), Mathf.FloorToInt(local.y));
    }

    /// <summary>
    /// 설게도 화면에서 camera의 시작 위치 보정
    /// 설계도에 방이 없을 시 그리드 중앙 시작
    /// 설계도에 방이 있을 경우 방 들의 중앙 위치에서 시작
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCameraStartPosition()
    {
        BlueprintRoom[] allBPRooms = targetBlueprintShip.GetComponentsInChildren<BlueprintRoom>();

        // 배치된 방 없으면 그리드 중앙
        if (allBPRooms.Length == 0)
            return GridToWorldPosition(gridSize / 2);

        // 전체 타일 평균 위치 계산
        List<Vector2Int> allTiles = new();
        foreach (BlueprintRoom room in allBPRooms)
            allTiles.AddRange(room.occupiedTiles);

        Vector2 average = Vector2.zero;
        foreach (Vector2Int tile in allTiles)
            average += (Vector2)tile;

        average /= allTiles.Count;

        return GridToWorldPosition(Vector2Int.RoundToInt(average));
    }

    /// <summary>
    /// 해당 타일이 그리드 범위 내에 있는지 검사
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public bool IsInGrid(Vector2Int tile)
    {
        if (tile.x < 0 || tile.x >= gridSize.x || tile.y < 0 || tile.y >= gridSize.y)
            return false;
        return true;
    }

    /// <summary>
    /// 해당 타일이 현재 다른 방에 의해 점유되어 있는지 검사
    /// </summary>
    /// <param name="tile"></param>
    /// <returns></returns>
    public bool IsTileOccupied(Vector2Int tile)
    {
        return occupiedGridTiles.Contains(tile);
    }

    /// <summary>
    /// 해당 방이 점유하고 있는 모든 타일을 전체 점유 목록에 추가
    /// </summary>
    /// <param name="room"></param>
    public void MarkRoomOccupied(BlueprintRoom room)
    {
        foreach (Vector2Int tile in room.occupiedTiles)
            occupiedGridTiles.Add(tile);
    }

    /// <summary>
    /// 특정 방 제거, 되돌릴 경우 점유 타일 목록에서 제거
    /// </summary>
    /// <param name="room"></param>
    public void UnMarkRoomOccupied(BlueprintRoom room)
    {
        foreach (Vector2Int tile in room.occupiedTiles)
            occupiedGridTiles.Remove(tile);
    }

    /// <summary>
    /// 디버깅 용 점유 타일 반환
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Vector2Int> GetAllOccupiedTiles()
    {
        return occupiedGridTiles;
    }

    /// <summary>
    /// 해당 방을 현재 위치와 회전 상태로 배치 가능한지 검사
    /// </summary>
    /// <param name="data">방 데이터</param>
    /// <param name="level">레벨</param>
    /// <param name="position">좌하단 기준 시작 위치</param>
    /// <param name="rotation">회전 각도</param>
    /// <returns></returns>
    public bool CanPlaceRoom(RoomData data, int level, Vector2Int origin, int rotation)
    {
        RoomData.RoomLevel levelData = data.GetRoomDataByLevel(level);
        Vector2Int size = RoomRotationUtility.GetRotatedSize(levelData.size, rotation);

        List<Vector2Int> tilesToOccupy = RoomRotationUtility.GetOccupiedGridPositions(origin, size, rotation);

        // 그리드 범위 벗어나는지 체크
        foreach (Vector2Int tile in tilesToOccupy)
            if (!IsInGrid(tile))
                return false;

        // 겹침 체크
        foreach (Vector2Int tile in tilesToOccupy)
            if (IsTileOccupied(tile))
                return false;

        return true;
    }

    /// <summary>
    /// 실제 방을 해당 위치에 배치함
    /// </summary>
    /// <param name="data"></param>
    /// <param name="level"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    public void PlaceRoom(RoomData data, int level, Vector2Int position, int rotation)
    {
        Vector2Int size = RoomRotationUtility.GetRotatedSize(data.GetRoomDataByLevel(level).size, rotation);
        Vector2 offset = RoomRotationUtility.GetRotationOffset(size, rotation);
        Vector3 worldPos = GridToWorldPosition(position) + (Vector3)offset;

        GameObject bpRoomGO = Instantiate(roomPrefab, targetBlueprintShip.transform);
        bpRoomGO.transform.position = worldPos + new Vector3(0, 0, 10f);
        bpRoomGO.transform.rotation = Quaternion.Euler(0, 0, -rotation);

        BlueprintRoom bpRoom = bpRoomGO.GetComponent<BlueprintRoom>();
        bpRoom.SetGridPlacer(this);
        bpRoom.Initialize(data, level, position, rotation);
        bpRoom.SetBlueprint(targetBlueprintShip);

        MarkRoomOccupied(bpRoom);

        targetBlueprintShip.AddRoom(bpRoom);
    }
}
