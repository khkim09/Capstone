using UnityEngine;

public class GridHelper
{
    public static Vector3 GetWorldPosition(Vector2Int gridPosition, Vector2Int size)
    {
        // 룸 크기의 절반
        float halfWidth = size.x * 0.5f;
        float halfHeight = size.y * 0.5f;

        // 좌측 하단 모서리가 gridPosition에 위치하도록 계산
        Vector3 worldPosition = new(
            gridPosition.x + halfWidth,
            gridPosition.y + halfHeight,
            0
        );

        return worldPosition;
    }
}
