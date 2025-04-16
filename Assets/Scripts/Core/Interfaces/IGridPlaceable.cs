using UnityEngine;

/// <summary>
/// 드래그하여 그리드에 배치 가능한 오브젝트 인터페이스
/// </summary>
public interface IGridPlaceable
{
    /// <summary>
    /// 드래그 모드 설정 (프리뷰 사용 중일 때 원본 오브젝트의 가시성 설정)
    /// </summary>
    /// <param name="isDragging">드래그 모드 여부</param>
    void SetDragMode(bool isDragging);

    /// <summary>
    /// 오브젝트 회전
    /// </summary>
    /// <param name="rotation">새 회전 상태</param>
    void Rotate(RotationConstants.Rotation rotation);

    /// <summary>
    /// 현재 회전 상태
    /// </summary>
    object GetRotation();

    /// <summary>
    /// 그리드 위치
    /// </summary>
    Vector2Int GetGridPosition();

    /// <summary>
    /// 그리드 위치 설정
    /// </summary>
    /// <param name="position">새 그리드 위치</param>
    void SetGridPosition(Vector2Int position);
}
