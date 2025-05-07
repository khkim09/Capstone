using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 실제 설계도 데이터를 관리하는 인스턴스 클래스. 방/무기 배치, 가격 계산, 중심 좌표 제공.
/// </summary>
public class BlueprintShip : MonoBehaviour
{
    [SerializeField] private Vector2Int gridSize = new(60, 60);
    [SerializeField] private List<IBlueprintPlaceable> placedBlueprintObjects = new();

    /// <summary>
    /// 함선 외갑판 레벨 (0: 레벨 1, 1: 레벨 2, 2: 레벨 3) - 함선 전체에 적용되는 하나의 값
    /// </summary>
    [SerializeField] private int currentHullLevel = 0;

    // 이전 코드와의 호환성을 위한 프로퍼티
    private List<BlueprintRoom> PlacedBlueprintRooms =>
        placedBlueprintObjects.OfType<BlueprintRoom>().ToList();

    private List<BlueprintWeapon> PlacedBlueprintWeapons =>
        placedBlueprintObjects.OfType<BlueprintWeapon>().ToList();

    /// <summary>
    /// 현재 설계도에 배치된 방 리스트
    /// </summary>
    public IReadOnlyList<BlueprintRoom> GetPlacedBlueprintRooms()
    {
        return PlacedBlueprintRooms;
    }

    /// <summary>
    /// 현재 설계도에 배치된 무기 리스트
    /// </summary>
    public IReadOnlyList<BlueprintWeapon> GetPlacedBlueprintWeapons()
    {
        return PlacedBlueprintWeapons;
    }

    /// <summary>
    /// 함선 설계도의 총 가격 반환
    /// </summary>
    /// <returns></returns>
    public int GetTotalBPCost()
    {
        int sum = 0;

        foreach (IBlueprintPlaceable placeable in placedBlueprintObjects)
            sum += placeable.GetCost();

        return sum;
    }

    /// <summary>
    /// 설계도에 배치된 모든 오브젝트 리스트 초기화
    /// </summary>
    public void ClearPlacedBPObjects()
    {
        placedBlueprintObjects.Clear();
    }

    /// <summary>
    /// 현재 함선의 외갑판 레벨 설정 - 모든 무기에 적용
    /// </summary>
    /// <param name="level">설정할 외갑판 레벨 (0-2)</param>
    public void SetHullLevel(int level)
    {
        if (level >= 0 && level < 3)
        {
            currentHullLevel = level;

            // 모든 무기에 일괄 적용
            foreach (BlueprintWeapon weapon in PlacedBlueprintWeapons) weapon.SetHullLevel(level);

            Debug.Log($"Ship hull level set to {level + 1}");
        }
    }

    /// <summary>
    /// 현재 함선의 외갑판 레벨 반환
    /// </summary>
    /// <returns>현재 외갑판 레벨 (0-2)</returns>
    public int GetHullLevel()
    {
        return currentHullLevel;
    }

    /// <summary>
    /// 유저가 배치한 모든 오브젝트의 중심값 호출 (함선의 중심을 기준으로 유저에게 보이는 UI 설계)
    /// </summary>
    public Vector2Int CenterPosition
    {
        get
        {
            if (placedBlueprintObjects.Count == 0)
                return gridSize / 2;

            // 모든 점유 타일 수집
            List<Vector2Int> allTiles = new();
            foreach (IBlueprintPlaceable placeable in placedBlueprintObjects)
                allTiles.AddRange(placeable.GetOccupiedTiles());

            int minX = allTiles.Min(t => t.x);
            int maxX = allTiles.Max(t => t.x);
            int minY = allTiles.Min(t => t.y);
            int maxY = allTiles.Max(t => t.y);

            return new Vector2Int((minX + maxX) / 2, (minY + maxY) / 2);
        }
    }

    /// <summary>
    /// 인자 gridPos에 위치한 오브젝트 반환
    /// </summary>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    public IBlueprintPlaceable GetObjectAt(Vector2Int gridPos)
    {
        foreach (IBlueprintPlaceable placeable in placedBlueprintObjects)
            if (placeable.GetOccupiedTiles().Contains(gridPos))
                return placeable;
        return null;
    }

    /// <summary>
    /// 설계도에 오브젝트를 추가하고 가격을 갱신합니다.
    /// </summary>
    /// <param name="placeable">배치 가능 오브젝트</param>
    public void AddPlaceable(IBlueprintPlaceable placeable)
    {
        placedBlueprintObjects.Add(placeable);

        // 무기인 경우 현재 함선의 외갑판 레벨 적용
        if (placeable is BlueprintWeapon weapon) weapon.SetHullLevel(currentHullLevel);
    }

    /// <summary>
    /// 설계도에서 오브젝트를 제거하고 가격을 갱신합니다.
    /// </summary>
    /// <param name="placeable">배치 가능 오브젝트</param>
    public void RemovePlaceable(IBlueprintPlaceable placeable)
    {
        placedBlueprintObjects.Remove(placeable);
    }

    /// <summary>
    /// 설계도 초기화
    /// </summary>
    public void ClearRooms()
    {
        foreach (IBlueprintPlaceable placeable in placedBlueprintObjects.ToList())
            Destroy(((MonoBehaviour)placeable).gameObject);

        placedBlueprintObjects.Clear();
    }
}
