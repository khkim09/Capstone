using System.Collections.Generic;
using UnityEngine;

public class ShipCustomizationManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public float cellSize = 1.0f;
    public Vector2 gridMin = new Vector2(-5.0f, -4.0f);
    public Vector2 gridMax = new Vector2(5.0f, 4.0f);

    [Header("Module Prefabs")]
    public GameObject corridorPrefab;
    public GameObject doorPrefab;
    // 다른 모듈 프리팹 추가 가능 (예: 무기 모듈 등)

    [Header("Customization Settings")]
    public Transform modulesContainer;  // 커스터마이징된 모듈들이 붙을 부모 Transform
    public int currency = 1000;         // 시작 재화

    [Header("Cost Settings")]
    public int corridorCost = 10;
    public int doorCost = 15;
    // 모듈별 추가 비용 설정 가능

    // 현재 배치된 모듈들을 그리드 좌표로 관리
    private Dictionary<Vector2Int, GameObject> placedModules = new Dictionary<Vector2Int, GameObject>();

    public Dictionary<Vector2Int, GameObject> PlacedModules
    {
        get { return placedModules; }
    }

    /// <summary>
    /// 주어진 월드 좌표에 모듈 프리팹을 배치합니다.
    /// 배치에 성공하면 재화를 차감하고 true를 반환합니다.
    /// </summary>
    public bool PlaceModule(GameObject modulePrefab, Vector2 worldPosition)
    {
        Vector2Int gridPos = SnapToGrid(worldPosition);
        if (!IsWithinGrid(gridPos))
        {
            Debug.LogWarning("그리드 범위를 벗어났습니다.");
            return false;
        }
        if (placedModules.ContainsKey(gridPos))
        {
            Debug.LogWarning("해당 그리드 셀에 이미 모듈이 배치되어 있습니다.");
            return false;
        }

        int cost = GetModuleCost(modulePrefab);
        if (currency < cost)
        {
            Debug.LogWarning("재화가 부족합니다.");
            return false;
        }

        // 배치할 모듈의 중앙 위치 계산
        Vector3 placePosition = GridCenter(gridPos);
        GameObject newModule = Instantiate(modulePrefab, placePosition, Quaternion.identity, modulesContainer);
        placedModules.Add(gridPos, newModule);
        currency -= cost;
        Debug.Log($"{modulePrefab.name} 배치됨 (위치: {gridPos}). 남은 재화: {currency}");
        return true;
    }

    /// <summary>
    /// 주어진 월드 좌표에 배치된 모듈을 삭제합니다.
    /// </summary>
    public bool RemoveModule(Vector2 worldPosition)
    {
        Vector2Int gridPos = SnapToGrid(worldPosition);
        if (placedModules.TryGetValue(gridPos, out GameObject module))
        {
            Destroy(module);
            placedModules.Remove(gridPos);
            // 선택: 삭제 시 일부 재화 환불 로직 추가 가능
            Debug.Log($"모듈 삭제됨 (위치: {gridPos}).");
            return true;
        }
        return false;
    }

    /// <summary>
    /// 월드 좌표를 그리드 좌표(Vector2Int)로 스냅합니다.
    /// </summary>
    private Vector2Int SnapToGrid(Vector2 position)
    {
        int x = Mathf.FloorToInt(position.x / cellSize);
        int y = Mathf.FloorToInt(position.y / cellSize);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// 그리드 좌표의 셀 중앙 위치(월드 좌표)를 반환합니다.
    /// </summary>
    private Vector3 GridCenter(Vector2Int gridPos)
    {
        float x = gridPos.x * cellSize + cellSize / 2;
        float y = gridPos.y * cellSize + cellSize / 2;
        return new Vector3(x, y, 0);
    }

    /// <summary>
    /// 주어진 그리드 좌표가 설정된 grid 범위 내에 있는지 검사합니다.
    /// </summary>
    private bool IsWithinGrid(Vector2Int gridPos)
    {
        // world 좌표로 변환해서 비교 (gridMin, gridMax는 월드 좌표 기준일 수 있음)
        Vector3 pos = GridCenter(gridPos);
        if (pos.x < gridMin.x || pos.x > gridMax.x)
            return false;
        if (pos.y < gridMin.y || pos.y > gridMax.y)
            return false;
        return true;
    }

    /// <summary>
    /// 모듈 프리팹에 따라 설치 비용을 반환합니다.
    /// </summary>
    private int GetModuleCost(GameObject modulePrefab)
    {
        if (modulePrefab.name.Contains("Corridor"))
            return corridorCost;
        else if (modulePrefab.name.Contains("Door"))
            return doorCost;
        // 추가 모듈 종류에 따른 비용 설정
        return 0;
    }

    /// <summary>
    /// 모든 배치된 모듈을 삭제하고 재설정합니다.
    /// </summary>
    public void ClearAllModules()
    {
        foreach (var module in placedModules.Values)
        {
            Destroy(module);
        }
        placedModules.Clear();
        Debug.Log("모든 모듈이 삭제되었습니다.");
    }

    /// <summary>
    /// 저장된 room 배치
    /// </summary>
    /// <param name="savedRoom"></param>
    public void PlaceSavedRoom(Room savedRoom)
    {
        Vector3 placePos = GridCenter(savedRoom.position);
        GameObject prefab = GetPrefabByRoomType(savedRoom.roomType);
        if (prefab == null) return;

        GameObject newRoomObj = Instantiate(prefab, placePos, Quaternion.identity, modulesContainer);
        Room roomInstance = newRoomObj.GetComponent<Room>();
        roomInstance.roomData = savedRoom.roomData;
        roomInstance.position = savedRoom.position;
        placedModules[savedRoom.position] = newRoomObj;
    }

    private GameObject GetPrefabByRoomType(RoomType type)
    {
        // RoomType별 프리팹을 반환
        switch (type)
        {
            case RoomType.Corridor: return corridorPrefab;
            // case RoomType.Door: return doorPrefab;
            // 필요시 추가
            default: return null;
        }
    }
}
