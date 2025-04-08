using UnityEngine;

/// <summary>
/// 회전된 사이즈와 좌하단 기준 보정 offset 계산
/// </summary>
public static class RoomRotationUtility
{
    public static Vector2Int GetRotatedSize(Vector2Int originalSize, int rotation)
    {
        return (rotation % 180 == 0) ? originalSize : new Vector2Int(originalSize.y, originalSize.x);
    }

    public static Vector2 GetRotationOffset(Vector2Int size, int rotation)
    {
        switch (rotation % 360)
        {
            case 0:
                Debug.Log($"x : {size.x}, y : {size.y}");
                return new Vector2(size.x / 2f - 0.5f, size.y / 2f - 0.5f);
            case 90:
                Debug.Log($"x : {size.x}, y : {size.y}");
                return new Vector2(size.y / 2f - 0.5f, -size.x / 2f + 0.5f);
            case 180:
                Debug.Log($"x : {size.x}, y : {size.y}");
                return new Vector2(-size.x / 2f + 0.5f, -size.y / 2f + 0.5f);
            case 270:
                Debug.Log($"x : {size.x}, y : {size.y}");
                return new Vector2(-size.y / 2f + 0.5f, size.x / 2f - 0.5f);
            default:
                return Vector2.zero;
        }
    }
}
