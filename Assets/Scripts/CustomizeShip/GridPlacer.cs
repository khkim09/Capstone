using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.ShaderGraph;
using UnityEngine;

/// <summary>
/// 함선 커스터마이징을 위한 그리드 시스템을 생성하고 방 배치를 관리하는 클래스.
/// 타일 생성, 배치 가능 여부 판단, 실제 설치 등을 처리합니다.
/// </summary>
public class GridPlacer : MonoBehaviour, IWorldGridSwitcher
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

    [SerializeField] private Vector2Int gridSize = new(60, 60);
    [SerializeField] private Vector3 gridOrigin = Vector3.zero;


    /// <summary>
    /// 인스턴스를 설정하고, 그리드 사용 상태 배열을 초기화합니다.
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
        gridOrigin = new Vector3(-gridSize.x / 2f * tileSize, -gridSize.y / 2f * tileSize, 0f);
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
        /*
        int x = (int)(Mathf.FloorToInt(worldPos.x / tileSize) - gridOrigin.x);
        int y = (int)(Mathf.FloorToInt(worldPos.y / tileSize) - gridOrigin.y);
        return new Vector2Int(x, y);
        */
        Vector3 local = new Vector3(worldPos.x, worldPos.y, 0) - gridOrigin;
        return new Vector2Int(Mathf.FloorToInt(local.x), Mathf.FloorToInt(local.y));
    }

    public void SetGridOrigin(Vector3 newOrigin)
    {
        gridOrigin = newOrigin;
    }

    public Vector3 GetGridCenter()
    {
        return gridOrigin + new Vector3(gridSize.x / 2f, gridSize.y / 2f, 0f);
    }

    public Vector2Int GetGridSize()
    {
        return gridSize;
    }

    /// <summary>
    /// 주어진 좌표에 해당 방을 배치할 수 있는지 확인
    /// </summary>
    public bool CanPlaceRoom(RoomData data, int level, Vector2Int position, int rotation)
    {
        RoomData.RoomLevel levelData = data.GetRoomData(level);
        Vector2Int size = RoomRotationUtility.GetRotatedSize(data.GetRoomData(level).size, rotation);
        RectInt area = new(position, size);

        // 그리드 범위 벗어나는지 체크
        if (position.x < 0 || position.y < 0 || position.x + size.x > gridSize.x || position.y + size.y > gridSize.y)
            return false;

        // 겹침 체크
        foreach (BlueprintRoom room in targetBlueprintShip.PlacedBlueprintRooms)
        {
            RectInt other = new(room.position, room.roomSize);
            if (area.Overlaps(other))
                return false;
        }

        return true;
    }

    /// <summary>
    /// 실제 방을 해당 위치에 배치함
    /// </summary>
    public void PlaceRoom(RoomData data, int level, Vector2Int position, int rotation)
    {
        Vector2Int size = RoomRotationUtility.GetRotatedSize(data.GetRoomData(level).size, rotation);
        Vector2 offset = RoomRotationUtility.GetRotationOffset(size, rotation);
        Vector3 worldPos = GridToWorldPosition(position) + (Vector3)offset;

        GameObject placed = Instantiate(roomPrefab, targetBlueprintShip.transform);
        placed.transform.position = worldPos;
        placed.transform.rotation = Quaternion.Euler(0, 0, -rotation);

        BlueprintRoom blueprintRoom = placed.GetComponent<BlueprintRoom>();
        blueprintRoom.Initialize(data, level, position, rotation);
        blueprintRoom.SetBlueprint(targetBlueprintShip);

        targetBlueprintShip.AddRoom(blueprintRoom);
    }
}
