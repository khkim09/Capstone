using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이벤트 맵 상의 개별 노드를 나타내는 컴포넌트.
/// 레벨/인덱스/위험 여부 등 정보를 포함하며, 연결된 다음 노드를 관리합니다.
/// </summary>
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

    /// <summary>
    /// 노드 데이터를 설정합니다. (레벨, 인덱스, 위험 여부)
    /// </summary>
    /// <param name="level">노드의 레벨 인덱스.</param>
    /// <param name="index">노드의 고유 인덱스.</param>
    /// <param name="dangerous">위험 지역 여부 (기본값: false).</param>
    public void SetNodeData(int level, int index, bool dangerous = false)
    {
        levelIndex = level;
        nodeIndex = index;
        isDangerous = dangerous;
    }

    /// <summary>
    /// 연결된 다음 노드를 추가합니다.
    /// 중복 연결은 방지됩니다.
    /// </summary>
    /// <param name="node">연결할 노드.</param>
    public void AddNextNode(EventNode node)
    {
        if (!nextNodes.Contains(node)) nextNodes.Add(node);
    }

    /// <summary>
    /// 현재 연결된 모든 노드를 제거합니다.
    /// </summary>
    public void ClearConnections()
    {
        nextNodes.Clear();
    }
}
