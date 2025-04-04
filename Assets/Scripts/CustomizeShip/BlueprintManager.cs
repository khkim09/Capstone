using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 설계도 기반 커스터마이징 시스템을 관리하는 매니저.
/// 방 추가/삭제, 가격 계산, 조건 확인, 실제 함선으로 교체까지 담당.
/// </summary>
public class BlueprintManager : MonoBehaviour
{
    /// <summary>
    /// 싱글톤 인스턴스.
    /// </summary>
    public static BlueprintManager Instance { get; private set; }

    /// <summary>
    /// 현재 설계도에 배치된 방 리스트.
    /// </summary>
    public List<BlueprintRoom> placedBlueprintRooms = new();

    /// <summary>
    /// 현재 설계도 함선의 총 가격.
    /// </summary>
    public int totalBlueprintCost = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        Instance = this;
    }

    /// <summary>
    /// 설계도에 방을 추가하고 가격을 갱신합니다.
    /// </summary>
    public void AddRoom(BlueprintRoom room)
    {
        placedBlueprintRooms.Add(room);
        totalBlueprintCost += room.roomCost;
    }

    /// <summary>
    /// 설계도에서 방을 제거하고 가격을 갱신합니다.
    /// </summary>
    public void RemoveRoom(BlueprintRoom room)
    {
        placedBlueprintRooms.Remove(room);
        totalBlueprintCost -= room.roomCost;
    }

    /// <summary>
    /// 현재 설계도의 총 가격을 계산합니다.
    /// </summary>
    public int GetBlueprintTotalCost()
    {
        return totalBlueprintCost;
    }

    /// <summary>
    /// 설계도를 초기화 합니다.
    /// </summary>
    public void ResetBlueprintRooms()
    {
        placedBlueprintRooms.Clear();
        totalBlueprintCost = 0;
    }

    /// <summary>
    /// 현재 재화와 내구도를 기반으로 설계도 함선으로 교체 가능한지 여부를 반환합니다. (보유 재화량 >= 설계도 재화량 & 내구도 100%)
    /// </summary>
    public bool CanBuildShip(int currentCurrency, bool fullHitPoint)
    {
        return currentCurrency >= totalBlueprintCost && fullHitPoint;
    }

    /// <summary>
    /// 실제 함선을 설계도 기반으로 교체합니다.
    /// </summary>
    public void ApplyBlueprintToShip(Ship currentShip, int currentCurrency)
    {
        if (!CanBuildShip(currentCurrency, currentShip.IsFullHitPoint()))
            return;

        currentShip.ReplaceShipWithBlueprint(placedBlueprintRooms);
    }
}
