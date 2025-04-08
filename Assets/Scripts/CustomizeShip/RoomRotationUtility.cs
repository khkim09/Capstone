using System.Collections.Generic;
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
        Vector2 baseOffset;

        switch (rotation % 360)
        {
            case 0:
                baseOffset = new Vector2(size.x / 2f - 0.5f, size.y / 2f - 0.5f);
                break;
            case 90:
                baseOffset = new Vector2(size.y / 2f - 0.5f, -size.x / 2f + 0.5f);
                break;
            case 180:
                baseOffset = new Vector2(-size.x / 2f + 0.5f, -size.y / 2f + 0.5f);
                break;
            case 270:
                baseOffset = new Vector2(-size.y / 2f + 0.5f, size.x / 2f - 0.5f);
                break;
            default:
                baseOffset = Vector2.zero;
                break;
        }

        // 짝수 크기 보정
        if (size.x % 2 == 0 || size.y % 2 == 0)
        {
            if (size.x == size.y)
                return baseOffset;

            if (Mathf.Abs(size.x - size.y) == 1)
            {
                if (rotation % 360 == 90)
                    baseOffset -= new Vector2(0.5f, 0.5f);
                else if (rotation % 360 == 270)
                    baseOffset += new Vector2(0.5f, 0.5f);
            }
            else
            {
                if (rotation % 360 == 90)
                    baseOffset -= new Vector2(1.0f, 1.0f);
                else if (rotation % 360 == 270)
                    baseOffset += new Vector2(1.0f, 1.0f);
            }
        }

        return baseOffset;
    }

    /// <summary>
    /// 기준 타일(anchor)과 회전 상태에 따른 실제 점유 타일 목록 반환
    /// </summary>
    public static List<Vector2Int> GetOccupiedTiles(Vector2Int anchor, Vector2Int size, int rotation)
    {
        List<Vector2Int> tiles = new();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int offset = rotation switch
                {
                    0 => new Vector2Int(x, y),
                    90 => new Vector2Int(size.y - 1 - y, x),
                    180 => new Vector2Int(size.x - 1 - x, size.y - 1 - y),
                    270 => new Vector2Int(y, size.x - 1 - x),
                    _ => Vector2Int.zero,
                };
                tiles.Add(anchor + offset);
            }
        }

        return tiles;
    }
}
