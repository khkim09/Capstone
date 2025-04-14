using UnityEngine;

/// <summary>
/// 그리드 <-> 월드 좌표 변환을 제공하는 인터페이스
/// </summary>
public interface IWorldGridSwitcher
{
    Vector2Int WorldToGridPosition(Vector2 worldPosition);
    Vector3 GridToWorldPosition(Vector2Int gridPosition);
}
