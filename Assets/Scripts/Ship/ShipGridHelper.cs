using System.Collections.Generic;
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

    /// <summary>
    /// 월드 좌표를 그리드 좌표로 변환합니다.
    /// </summary>
    /// <param name="worldPos">변환할 월드 좌표.</param>
    /// <returns>해당 위치의 그리드 좌표.</returns>
    public static Vector2Int WorldToGridPosition(Vector2 worldPos)
    {
        Vector3 local = new(worldPos.x, worldPos.y, 0);
        return new Vector2Int(Mathf.FloorToInt(local.x / Constants.Grids.CellSize),
            Mathf.FloorToInt(local.y / Constants.Grids.CellSize));
    }

    /// <summary>
    /// 그리드 좌표를 월드 좌표로 변환합니다.
    /// </summary>
    /// <param name="gridPos">변환할 그리드 좌표.</param>
    /// <returns>해당 위치의 월드 좌표.</returns>
    public static Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return new Vector3((gridPos.x + 0.5f) * Constants.Grids.CellSize,
            (gridPos.y + 0.5f) * Constants.Grids.CellSize, 0f);
    }

    /// <summary>
    /// 배가 차지하는 그리드 위치를 기반으로 회전 오프셋을 계산합니다
    /// </summary>
    /// <param name="ship">오프셋을 계산할 배</param>
    /// <param name="rotation">회전 상태</param>
    /// <returns>회전에 따른 오프셋 값</returns>
    public static Vector2 GetShipRotationOffset(Ship ship, Constants.Rotations.Rotation rotation)
    {
        // 배가 차지하는 모든 그리드 위치 수집
        List<Vector2Int> occupiedPositions = new();
        foreach (Room room in ship.GetAllRooms())
        foreach (Vector2Int pos in room.GetOccupiedTiles())
            occupiedPositions.Add(pos);

        if (occupiedPositions.Count == 0)
        {
            Debug.LogWarning("배가 차지하는 그리드 위치가 없습니다!");
            return Vector2.zero;
        }

        // 최소/최대 X, Y 값 찾기
        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;

        foreach (Vector2Int pos in occupiedPositions)
        {
            minX = Mathf.Min(minX, pos.x);
            maxX = Mathf.Max(maxX, pos.x);
            minY = Mathf.Min(minY, pos.y);
            maxY = Mathf.Max(maxY, pos.y);
        }

        // 배 크기 계산 (그리드 단위)
        Vector2Int shipSize = new(maxX - minX + 1, maxY - minY + 1);
        Debug.LogError(shipSize);
        // GetRotationOffset 함수 사용
        return RoomRotationUtility.GetRotationOffset(shipSize, rotation);
    }
}
