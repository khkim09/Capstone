using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 회전된 사이즈와 좌하단 기준 보정 offset 계산
/// </summary>
public static class RoomRotationUtility
{
    /// <summary>
    /// 회전한 이후 방 사이즈 다시 계산
    /// </summary>
    /// <param name="originalSize">기존 방 사이즈(roomData)</param>
    /// <param name="rotation">기존 방 회전 각</param>
    /// <returns></returns>
    public static Vector2Int GetRotatedSize(Vector2Int originalSize, RotationConstants.Rotation rotation)
    {
        if (rotation == RotationConstants.Rotation.Rotation0 || rotation == RotationConstants.Rotation.Rotation180)
            return originalSize;

        return new Vector2Int(originalSize.y, originalSize.x);
    }

    /// <summary>
    /// 방의 좌하단 블록 좌표 반환
    /// </summary>
    /// <param name="size">방 사이즈</param>
    /// <param name="rotation">회전각</param>
    /// <returns></returns>
    public static Vector2 GetRotationOffset(Vector2Int size, RotationConstants.Rotation rotation)
    {
        Vector2 baseOffset;

        switch (rotation)
        {
            case RotationConstants.Rotation.Rotation0:
                baseOffset = new Vector2(size.x / 2f - 0.5f, size.y / 2f - 0.5f);
                break;
            case RotationConstants.Rotation.Rotation90:
                baseOffset = new Vector2(size.y / 2f - 0.5f, -size.x / 2f + 0.5f);
                break;
            case RotationConstants.Rotation.Rotation180:
                baseOffset = new Vector2(-size.x / 2f + 0.5f, -size.y / 2f + 0.5f);
                break;
            case RotationConstants.Rotation.Rotation270:
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
                if (rotation == RotationConstants.Rotation.Rotation90)
                    baseOffset -= new Vector2(0.5f, 0.5f);
                else if (rotation == RotationConstants.Rotation.Rotation270)
                    baseOffset += new Vector2(0.5f, 0.5f);
            }
            else
            {
                if (rotation == RotationConstants.Rotation.Rotation90)
                    baseOffset -= new Vector2(1.0f, 1.0f);
                else if (rotation == RotationConstants.Rotation.Rotation270)
                    baseOffset += new Vector2(1.0f, 1.0f);
            }
        }

        return baseOffset;
    }

    /// <summary>
    /// 회전된 타일 오프셋을 기준으로 기준점(origin)에 맞춘 점유 타일 좌표를 반환합니다.
    /// </summary>
    /// <param name="origin">좌하단 기준 좌표</param>
    /// <param name="size">회전각 적용된 방 사이즈</param>
    /// <param name="rot">회전각</param>
    /// <returns></returns>
    public static List<Vector2Int> GetOccupiedGridPositions(Vector2Int origin, Vector2Int size,
        RotationConstants.Rotation rot)
    {
        List<Vector2Int> result = new();

        switch (rot)
        {
            case RotationConstants.Rotation.Rotation0:
                for (int j = origin.y; j < origin.y + size.y; j++)
                    for (int i = origin.x; i < origin.x + size.x; i++)
                        result.Add(new Vector2Int(i, j));
                break;
            case RotationConstants.Rotation.Rotation90:
                for (int j = origin.y; j > origin.y - size.y; j--)
                    for (int i = origin.x; i < origin.x + size.x; i++)
                        result.Add(new Vector2Int(i, j));
                break;
            case RotationConstants.Rotation.Rotation180:
                for (int j = origin.y; j > origin.y - size.y; j--)
                    for (int i = origin.x; i > origin.x - size.x; i--)
                        result.Add(new Vector2Int(i, j));
                break;
            case RotationConstants.Rotation.Rotation270:
                for (int j = origin.y; j < origin.y + size.y; j++)
                    for (int i = origin.x; i > origin.x - size.x; i--)
                        result.Add(new Vector2Int(i, j));
                break;
        }

        return result;
    }

    /// <summary>
    /// 문 위치 반환
    /// </summary>
    /// <param name="localPos"></param>
    /// <param name="roomSize"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public static Vector2Int RotateDoorPos(Vector2Int localDoorPos, RotationConstants.Rotation rotation)
    {
        return rotation switch
        {
            RotationConstants.Rotation.Rotation0 => localDoorPos,
            RotationConstants.Rotation.Rotation90 => new Vector2Int(localDoorPos.y, -localDoorPos.x),
            RotationConstants.Rotation.Rotation180 => new Vector2Int(-localDoorPos.x, -localDoorPos.y),
            RotationConstants.Rotation.Rotation270 => new Vector2Int(-localDoorPos.y, localDoorPos.x),
            _ => localDoorPos
        };
    }

    /// <summary>
    /// 문 바라보는 방향 회전각 반환
    /// </summary>
    /// <param name="original"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public static DoorDirection RotateDoorDirection(DoorDirection original, RotationConstants.Rotation rotation)
    {
        int baseDir = (int)original;
        int rotated = (baseDir + ((int)rotation / 90)) % 4;
        return (DoorDirection)rotated;
    }

}
