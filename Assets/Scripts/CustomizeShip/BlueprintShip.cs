using System.Collections.Generic;
using System.Linq;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

/// <summary>
/// 실제 설계도 데이터를 관리하는 인스턴스 클래스. 방 배치, 가격 계산, 중심 좌표 제공.
/// </summary>
public class BlueprintShip : MonoBehaviour
{
    [SerializeField] private Vector2Int gridSize = new(60, 60);
    [SerializeField] private List<BlueprintRoom> placedBlueprintRooms = new();
    [SerializeField] private HashSet<Vector2Int> occupiedTiles = new();

    /// <summary>
    /// 현재 설계도에 배치된 방 리스트
    /// </summary>
    public IReadOnlyList<BlueprintRoom> PlacedBlueprintRooms => placedBlueprintRooms;

    /// <summary>
    /// 현재 설계도의 총 가격
    /// </summary>
    public int totalBlueprintCost => placedBlueprintRooms.Sum(r => r.bpRoomCost);

    /// <summary>
    /// 유저가 배치한 방들의 중심값 호출 (함선의 중심을 기준으로 유저에게 보이는 UI 설계)
    /// </summary>
    public Vector2Int CenterPosition
    {
        get
        {
            if (placedBlueprintRooms.Count == 0) // 수정 필요
                return gridSize / 2;
            int minX = placedBlueprintRooms.Min(r => r.bpPosition.x);
            int maxX = placedBlueprintRooms.Max(r => r.bpPosition.x + r.bpRoomSize.x);
            int minY = placedBlueprintRooms.Min(r => r.bpPosition.y);
            int maxY = placedBlueprintRooms.Max(r => r.bpPosition.y + r.bpRoomSize.y);
            return new Vector2Int((minX + maxX) / 2, (minY + maxY) / 2);
        }
    }

    /// <summary>
    /// 인자 gridPos에 위치한 방 반환
    /// </summary>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    public BlueprintRoom GetRoomAt(Vector2Int gridPos)
    {
        foreach (BlueprintRoom room in placedBlueprintRooms)
        {
            for (int x = 0; x < room.bpRoomSize.x; x++)
                for (int y = 0; y < room.bpRoomSize.y; y++)
                {
                    if (room.bpPosition + new Vector2Int(x, y) == gridPos)
                        return room;
                }
        }
        return null;
    }

    /// <summary>
    /// 설계도에 방을 추가하고 가격을 갱신합니다.
    /// </summary>
    /// <param name="room"></param>
    public void AddRoom(BlueprintRoom room)
    {
        placedBlueprintRooms.Add(room);
    }

    /// <summary>
    /// 설계도에서 방을 제거하고 가격을 갱신합니다.
    /// </summary>
    /// <param name="room"></param>
    public void RemoveRoom(BlueprintRoom room)
    {
        placedBlueprintRooms.Remove(room);
    }

    /// <summary>
    /// 설계도 초기화
    /// </summary>
    public void ClearRooms()
    {
        foreach (BlueprintRoom room in placedBlueprintRooms)
            Destroy(room.gameObject);
        placedBlueprintRooms.Clear();
    }
}
