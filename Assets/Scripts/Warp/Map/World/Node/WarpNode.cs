using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class WarpNode : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image nodeImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color startNodeColor = Color.green;
    [SerializeField] private Color endNodeColor = Color.red;
    [SerializeField] private Color selectedColor = Color.yellow;

    [SerializeField] private RectTransform connectionPoint;

    // 노드 데이터
    public WarpNodeData NodeData { get; private set; }

    // 클릭 이벤트 델리게이트
    public Action<WarpNode> onClicked;

    // 선택 상태
    private bool isSelected = false;

    public void SetNodeData(WarpNodeData data)
    {
        NodeData = data;
        UpdateVisual();
    }

    public void SetAsStartNode()
    {
        nodeImage.color = startNodeColor;
    }

    public void SetAsEndNode()
    {
        nodeImage.color = endNodeColor;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        nodeImage.color = selected ? selectedColor :
            NodeData.isStartNode ? startNodeColor :
            NodeData.isEndNode ? endNodeColor : normalColor;
    }

    private void UpdateVisual()
    {
        if (NodeData.isStartNode)
            SetAsStartNode();
        else if (NodeData.isEndNode)
            SetAsEndNode();
        else
            nodeImage.color = normalColor;
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
