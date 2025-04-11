using UnityEngine;

public class ShipGridHelper
{
    /// <summary>
    /// 룸의 월드 위치를 설정합니다.
    /// 좌측 하단 모서리를 기준으로 계산합니다.
    /// </summary>
    public static Vector3 GetRoomWorldPosition(Vector2Int gridPosition, Vector2Int size)
    {
        // 룸 크기의 절반
        float halfWidth = size.x * 0.5f;
        float halfHeight = size.y * 0.5f;

        // 그리드 위치 + 룸 크기의 절반 = 룸 중심 위치
        // 좌측 하단 모서리가 gridPosition에 위치하도록 계산
        Vector3 worldPosition = new(
            gridPosition.x + halfWidth,
            gridPosition.y + halfHeight,
            0
        );

        return worldPosition;
    }
}
