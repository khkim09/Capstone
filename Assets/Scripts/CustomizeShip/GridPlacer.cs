using System.Collections.Generic;
using UnityEditor.ShaderGraph;
using UnityEngine;

public class GridPlacer : MonoBehaviour
{
    public static GridPlacer Instance;

    public GameObject tilePrefab;
    public Transform gridTiles;
    public Transform placedRooms;

    public int width = 60;
    public int height = 60;
    public float tileSize = 1f;

    private bool[,] gridOccupied;

    private void Awake()
    {
        Instance = this;
        gridOccupied = new bool[width, height];
    }

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
    /// 방 설치 전, 드래그 중인 방에 대해 해당 위치에 설치 가능한지 여부를 판단합니다. (preview 형식 - 실루엣)
    /// 해당 영역에 방 설치가 가능하다면 true 반환 (색상 : 초록색, alpha : 0.5f)
    /// 방 설치 불가하다면 false 반환 (색상 : 빨간색, alpha : 0.5f)
    /// </summary>
    /// <param name="startGrid"></param>
    /// <param name="roomSize"></param>
    /// <returns></returns>
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
    /// 실제로 호버한 위치에 해당 영역에 방을 설치합니다.
    /// 방을 설치하고자 하는 위치에 설치 가능 여부를 검사해, 설치를 진행합니다.
    /// </summary>
    /// <param name="startGrid"></param>
    /// <param name="roomSize"></param>
    /// <param name="roomPrefab"></param>
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
