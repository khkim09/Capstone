using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 휠 스크롤은 수평으로 반전 및 감도 조절하여 허용,
/// 스크롤바 핸들 위에서만 마우스 드래그를 허용하는 ScrollRect 확장.
/// </summary>
public class InvertedHorizontalScroll : ScrollRect
{
    [Tooltip("휠 스크롤 감도. 작을수록 천천히 스크롤.")]
    [SerializeField] private float scrollSpeed = 0.01f;

    /// <summary>
    /// 현재 포인터가 스크롤바 핸들 위에 있는지 여부를 판단
    /// </summary>
    private bool IsOnScrollbarHandle(PointerEventData eventData)
    {
        GameObject go = eventData.pointerCurrentRaycast.gameObject;
        if (go == null)
            return false;

        if (horizontalScrollbar && go == horizontalScrollbar.handleRect?.gameObject)
            return true;

        if (verticalScrollbar && go == verticalScrollbar.handleRect?.gameObject)
            return true;

        return false;
    }

    /// <summary>
    /// 휠 스크롤 반전 및 감도 적용
    /// </summary>
    public override void OnScroll(PointerEventData data)
    {
        if (horizontal)
            horizontalNormalizedPosition -= data.scrollDelta.y * scrollSpeed;
        else
            base.OnScroll(data); // 수직인 경우 기본 처리 유지
    }

    /// <summary>
    /// 핸들 위일 때만 드래그 시작 허용
    /// </summary>
    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (IsOnScrollbarHandle(eventData))
            base.OnBeginDrag(eventData);
    }

    /// <summary>
    /// 핸들 위일 때만 드래그 허용
    /// </summary>
    public override void OnDrag(PointerEventData eventData)
    {
        if (IsOnScrollbarHandle(eventData))
            base.OnDrag(eventData);
    }

    /// <summary>
    /// 핸들 위일 때만 드래그 종료 허용
    /// </summary>
    public override void OnEndDrag(PointerEventData eventData)
    {
        if (IsOnScrollbarHandle(eventData))
            base.OnEndDrag(eventData);
    }
}
