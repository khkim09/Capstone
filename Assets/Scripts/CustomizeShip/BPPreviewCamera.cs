using System.Collections.Generic;
using UnityEngine;

public class BPPreviewCamera : MonoBehaviour
{
    [SerializeField] private Camera previewCamera;
    private float padding = 1f;

    /// <summary>
    /// preview Area 사이즈에 맞게 도안 띄우기
    /// </summary>
    /// <param name="tilePositions"></param>
    public void FitToBlueprint(List<Vector2Int> tilePositions)
    {
        if (tilePositions == null || tilePositions.Count == 0 || previewCamera == null)
            return;

        Vector2Int min = new(int.MaxValue, int.MaxValue);
        Vector2Int max = new(int.MinValue, int.MinValue);

        foreach (Vector2Int pos in tilePositions)
        {
            min = Vector2Int.Min(min, pos);
            max = Vector2Int.Max(max, pos);
        }

        // 중심 좌표 계산
        Vector2 centerGridPos = new Vector2((min.x + max.x) / 2f, (min.y + max.y) / 2f);
        Vector3 centerWorldPos = GridToWorldPosition(centerGridPos);

        previewCamera.transform.position = new Vector3(centerWorldPos.x, centerWorldPos.y, previewCamera.transform.position.z);

        float width = (max.x - min.x + 1) * Constants.Grids.CellSize;
        float height = (max.y - min.y + 1) * Constants.Grids.CellSize;
        float sizeX = width / previewCamera.aspect / 2f + padding;
        float sizeY = height / 2f + padding;
        previewCamera.orthographicSize = Mathf.Max(sizeX, sizeY);
    }

    /// <summary>
    /// 그리드 좌표를 월드 좌표로 변환합니다.
    /// </summary>
    /// <param name="gridPos">변환할 그리드 좌표.</param>
    /// <returns>해당 위치의 월드 좌표.</returns>
    public Vector3 GridToWorldPosition(Vector2 gridPos)
    {
        return Vector3.zero + new Vector3((gridPos.x + 0.5f) * Constants.Grids.CellSize,
            (gridPos.y + 0.5f) * Constants.Grids.CellSize, 0f);
    }

    /// <summary>
    /// 실제 월드 위치 반환
    /// </summary>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    public Vector3 GetWorldPositionFromGrid(Vector2 gridPos)
    {
        return transform.TransformPoint(GridToWorldPosition(gridPos));
    }
}
