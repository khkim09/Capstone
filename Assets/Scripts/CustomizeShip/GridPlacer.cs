using System.Collections.Generic;
using UnityEditor.ShaderGraph;
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
    /// 타일이 이미 사용 중인지 여부를 저장하는 2차원 배열입니다.
    /// </summary>
    private bool[,] gridOccupied;

    /// <summary>
    /// 인스턴스를 설정하고, 그리드 사용 상태 배열을 초기화합니다.
    /// </summary>
    private void Awake()
    {
        Instance = this;
        gridOccupied = new bool[width, height];
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
        for (int x = 0; x < roomSize.x; x++)
        {
            for (int y = 0; y < roomSize.y; y++)
            {
                Vector2Int checkPos = startGrid + new Vector2Int(x, y);

                if (checkPos.x < 0 || checkPos.x >= width || checkPos.y < 0 || checkPos.y >= height || gridOccupied[checkPos.x, checkPos.y])
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 실제 지정된 위치에 방을 배치합니다.
    /// 설치 가능 여부를 검사한 뒤, 배치를 수행하고 해당 영역을 점유 처리합니다.
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

        for (int x = 0; x < roomSize.x; x++)
            for (int y = 0; y < roomSize.y; y++)
                gridOccupied[startGrid.x + x, startGrid.y + y] = true;

        return room;
    }
}
