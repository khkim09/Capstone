using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선 설계도에 배치 가능한 모든 오브젝트가 구현해야 하는 인터페이스.
/// 방, 무기 등 모든 배치 가능 오브젝트의 공통 기능을 정의합니다.
/// </summary>
public interface IBlueprintPlaceable
{
    /// <summary>
    /// 현재 회전 상태 반환
    /// </summary>
    object GetRotation();

    /// <summary>
    /// 현재 그리드 위치 반환
    /// </summary>
    Vector2Int GetGridPosition();

    /// <summary>
    /// 그리드 위치 설정
    /// </summary>
    void SetGridPosition(Vector2Int position);

    /// <summary>
    /// 점유하고 있는 타일 목록 반환
    /// </summary>
    List<Vector2Int> GetOccupiedTiles();

    /// <summary>
    /// 그리드 배치 담당 오브젝트 세팅
    /// </summary>
    void SetGridPlacer(GridPlacer placer);

    /// <summary>
    /// 설계도 함선 할당
    /// </summary>
    void SetBlueprint(BlueprintShip bpShip);

    /// <summary>
    /// 설계도 함선 호출
    /// </summary>
    BlueprintShip GetBlueprintShip();

    /// <summary>
    /// 설치 비용 반환
    /// </summary>
    int GetCost();

    /// <summary>
    /// 드래그 모드 설정
    /// </summary>
    void SetDragMode(bool isDragging);
}
