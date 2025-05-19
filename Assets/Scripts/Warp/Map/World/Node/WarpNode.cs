using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class WarpNode : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image nodeImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color startNodeColor = Color.green;
    [SerializeField] private Color endNodeColor = Color.red;
    [SerializeField] private Color selectedColor = Color.yellow;

    [SerializeField] private Sprite startNodeImage;
    [SerializeField] private Sprite eventNodeImage;
    [SerializeField] private Sprite combatNodeImage;
    [SerializeField] private Sprite disableNodeImage;
    [SerializeField] private Sprite currentEventNodeImage;
    [SerializeField] private Sprite currentCombatNodeImage;

    [SerializeField] private RectTransform connectionPoint;


    // 노드 데이터
    public WarpNodeData NodeData { get; private set; }

    // 클릭 이벤트 델리게이트
    public Action<WarpNode> onClicked;

    // 활성화/비활성화 상태
    private bool isEnabled = true;

    // 플레이어 현재 위치 여부
    private bool isCurrentPlayerNode = false;

    public void SetNodeData(WarpNodeData data)
    {
        NodeData = data;
        UpdateNodeState();

        UpdateVisual();
    }

    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;
    }

    public void SetAsStartNode()
    {
        SetEnabled(true);
        nodeImage.sprite = startNodeImage;
    }


    public void SetAsEndNode(Planet planet = null)
    {
        if (planet == null)
        {
        }

        nodeImage.sprite = planet?.GetCurrentSprite();
    }

    public void SetCurrentPlayerNode(bool isCurrent)
    {
        isCurrentPlayerNode = isCurrent;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        // 비활성화된 노드 처리
        if (!isEnabled)
        {
            nodeImage.sprite = disableNodeImage;
            return;
        }

        if (NodeData.isStartNode)
            SetAsStartNode();
        else if (NodeData.isEndNode)
            return;
        else
            switch (NodeData.nodeType)
            {
                case WarpNodeType.Event:
                    if (isCurrentPlayerNode)
                        nodeImage.sprite = currentEventNodeImage;
                    else
                        nodeImage.sprite = eventNodeImage;

                    break;
                case WarpNodeType.Combat:
                    if (isCurrentPlayerNode)
                        nodeImage.sprite = currentCombatNodeImage;
                    else
                        nodeImage.sprite = combatNodeImage;
                    break;
                case WarpNodeType.Default:
                    nodeImage.sprite = eventNodeImage;
                    break;
                default:
                    break;
            }
    }

    private void UpdateNodeState()
    {
        if (NodeData == null) return;

        // 현재 플레이어 위치 확인
        int currentPlayerNodeId = GameManager.Instance.CurrentWarpNodeId;
        isCurrentPlayerNode = NodeData.nodeId == currentPlayerNodeId;

        // 도달 가능성 확인
        if (NodeData.isStartNode)
            // 시작 노드는 항상 활성화
            isEnabled = true;
        else
            // 일반 노드는 현재 플레이어가 도달할 수 있는지 확인
            isEnabled = CanReachFromCurrentPlayer();
    }

    private bool CanReachFromCurrentPlayer()
    {
        int currentPlayerNodeId = GameManager.Instance.CurrentWarpNodeId;

        // 현재 노드가 설정되지 않았다면 시작 노드에서 확인
        if (currentPlayerNodeId == -1)
        {
            WarpNodeData startNode = GameManager.Instance.WarpNodeDataList.Find(n => n.isStartNode);
            currentPlayerNodeId = startNode?.nodeId ?? -1;
        }

        if (currentPlayerNodeId == -1) return false;

        // BFS로 현재 플레이어 위치에서 도달 가능한지 확인
        HashSet<int> visited = new();
        Queue<int> queue = new();

        queue.Enqueue(currentPlayerNodeId);
        visited.Add(currentPlayerNodeId);

        // 노드 ID를 키로 하는 딕셔너리 생성
        Dictionary<int, WarpNodeData> nodeMap = new();
        foreach (WarpNodeData node in GameManager.Instance.WarpNodeDataList) nodeMap[node.nodeId] = node;

        while (queue.Count > 0)
        {
            int currentId = queue.Dequeue();

            // 목표 노드에 도달했는지 확인
            if (currentId == NodeData.nodeId) return true;

            // 현재 노드에서 연결된 노드들 탐색
            if (nodeMap.ContainsKey(currentId))
            {
                WarpNodeData currentNode = nodeMap[currentId];
                foreach (int connectedId in currentNode.connectionIds)
                    if (!visited.Contains(connectedId))
                    {
                        visited.Add(connectedId);
                        queue.Enqueue(connectedId);
                    }
            }
        }

        return false;
    }

    public bool IsDirectlyConnectedToPlayer()
    {
        if (!isEnabled) return false;

        int currentPlayerNodeId = GameManager.Instance.CurrentWarpNodeId;
        if (currentPlayerNodeId == -1)
        {
            // 시작 노드에서 확인
            WarpNodeData startNode = GameManager.Instance.WarpNodeDataList.Find(n => n.isStartNode);
            currentPlayerNodeId = startNode?.nodeId ?? -1;
        }

        if (currentPlayerNodeId == -1) return false;

        // 현재 플레이어 노드를 찾아서 연결 확인
        WarpNodeData playerNode = GameManager.Instance.WarpNodeDataList.Find(n => n.nodeId == currentPlayerNodeId);
        if (playerNode == null) return false;

        // connectionIds가 비어있다면 connections 리스트를 확인
        if (playerNode.connectionIds.Count > 0)
        {
            return playerNode.connectionIds.Contains(NodeData.nodeId);
        }
        else
        {
            // connections 리스트에서 확인
            foreach (WarpNodeData connectedNode in playerNode.connections)
                if (connectedNode.nodeId == NodeData.nodeId)
                    return true;

            return false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClicked?.Invoke(this);
    }

    public Vector2 GetConnectionPosition()
    {
        return connectionPoint.position;
    }
}
