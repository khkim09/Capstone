using System.Collections;
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

        // 짝수 tilesize에 대해 회전각 보정
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
    /// 회전된 타일 오프셋을 기준으로 기준점(origin)에 맞춘 점유 타일 좌표를 반환합니다.
    /// </summary>
    public static List<Vector2Int> GetOccupiedGridPositions(Vector2Int origin, Vector2Int size, int rot)
    {
        List<Vector2Int> result = new();

        switch (rot)
        {
            case 0:
                for (int j = origin.y; j < origin.y + size.y; j++)
                    for (int i = origin.x; i < origin.x + size.x; i++)
                        result.Add(new Vector2Int(i, j));
                break;
            case 90:
                for (int j = origin.y; j > origin.y - size.y; j--)
                    for (int i = origin.x; i < origin.x + size.x; i++)
                        result.Add(new Vector2Int(i, j));
                break;
            case 180:
                for (int j = origin.y; j > origin.y - size.y; j--)
                    for (int i = origin.x; i > origin.x - size.x; i--)
                        result.Add(new Vector2Int(i, j));
                break;
            case 270:
                for (int j = origin.y; j < origin.y + size.y; j++)
                    for (int i = origin.x; i > origin.x - size.x; i++)
                        result.Add(new Vector2Int(i, j));
                break;
        }

        return result;
    }


}
