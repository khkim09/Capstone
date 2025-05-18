using System.Collections.Generic;
using UnityEngine;

// 워프 노드 데이터 저장 클래스 - 순환 참조 해결
[System.Serializable]
public class WarpNodeData
{
    public Vector2 normalizedPosition;
    public bool isStartNode;
    public bool isEndNode;
    public int layer;
    public int indexInLayer;

    // 직렬화에서 제외하여 순환 참조 방지
    [System.NonSerialized] public List<WarpNodeData> connections = new();

    // 연결된 노드들의 ID만 저장 (직렬화 용)
    public List<int> connectionIds = new();

    // 노드 고유 ID
    public int nodeId;

    // ID로 연결 설정하는 메서드
    public void SetConnectionsByIds(Dictionary<int, WarpNodeData> nodeMap)
    {
        connections.Clear();
        foreach (int id in connectionIds)
            if (nodeMap.ContainsKey(id))
                connections.Add(nodeMap[id]);
    }

    // 연결 추가 시 ID도 함께 추가
    public void AddConnection(WarpNodeData targetNode)
    {
        if (!connections.Contains(targetNode))
        {
            connections.Add(targetNode);
            if (!connectionIds.Contains(targetNode.nodeId)) connectionIds.Add(targetNode.nodeId);
        }
    }
}
