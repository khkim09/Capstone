using System.Collections.Generic;
using UnityEngine;

public class ShipPathDebugVisualizer : MonoBehaviour
{
    public List<Vector2Int> visitedDoorTiles = new();
    public List<Vector2Int> reachableTiles = new();
    public Vector2Int startTile;

    private void OnDrawGizmos()
    {
        // 시작 문 위치: 녹색
        Gizmos.color = Color.green;
        Gizmos.DrawCube((Vector3Int)startTile + Vector3.one * 0.5f, Vector3.one * 0.3f);

        // 방문한 문 위치: 파랑
        Gizmos.color = Color.blue;
        foreach (var tile in visitedDoorTiles)
            Gizmos.DrawCube((Vector3Int)tile + Vector3.one * 0.5f, Vector3.one * 0.2f);

        // 연결된 방의 타일: 노랑
        Gizmos.color = Color.yellow;
        foreach (var tile in reachableTiles)
            Gizmos.DrawCube((Vector3Int)tile + Vector3.one * 0.5f, Vector3.one * 0.15f);
    }

}
