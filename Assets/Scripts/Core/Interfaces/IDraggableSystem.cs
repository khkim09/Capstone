using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 드래그 가능한 요소 관리를 위한 공통 인터페이스
/// </summary>
public interface IDraggableSystem
{
    /// <summary>
    /// 드래그 시작
    /// </summary>
    /// <param name="mousePosition">마우스 위치</param>
    void StartDragging(Vector2 mousePosition);

    /// <summary>
    /// 지정된 위치에 오브젝트 배치 가능 여부 확인
    /// </summary>
    /// <param name="position">그리드 위치</param>
    /// <param name="rotation">회전 상태</param>
    /// <returns>배치 가능 여부</returns>
    bool CanPlace(Vector2Int position, int rotation);

    /// <summary>
    /// 지정된 위치에 오브젝트 배치
    /// </summary>
    /// <param name="position">그리드 위치</param>
    /// <param name="rotation">회전 상태</param>
    /// <returns>배치 성공 여부</returns>
    bool PlaceObject(Vector2Int position, int rotation);

    /// <summary>
    /// 드래그 취소
    /// </summary>
    void CancelDragging();

    /// <summary>
    /// 프리뷰 회전
    /// </summary>
    void RotatePreview();

    /// <summary>
    /// 드래그 중인지 여부
    /// </summary>
    bool IsDragging { get; }

    /// <summary>
    /// 현재 회전 상태
    /// </summary>
    int CurrentRotation { get; }
}
