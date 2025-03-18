using System.Collections.Generic;
using UnityEngine;

// 개선된 노드 UI 컴포넌트
public class EventNode : MonoBehaviour
{
    [SerializeField] private int levelIndex;
    [SerializeField] private int nodeIndex;
    [SerializeField] private bool isDangerous; // 위험지대 여부 추가

    // 연결된 다음 노드 목록
    private List<EventNode> nextNodes = new();

    public int LevelIndex => levelIndex;
    public int NodeIndex => nodeIndex;
    public List<EventNode> NextNodes => nextNodes;
    public bool IsDangerous => isDangerous; // 위험지대 여부 속성 노출

    public void SetNodeData(int level, int index, bool dangerous = false)
    {
        levelIndex = level;
        nodeIndex = index;
        isDangerous = dangerous;
    }

    public void AddNextNode(EventNode node)
    {
        if (!nextNodes.Contains(node)) nextNodes.Add(node);
    }

    public void ClearConnections()
    {
        nextNodes.Clear();
    }
}
