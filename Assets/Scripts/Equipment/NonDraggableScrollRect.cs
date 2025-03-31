using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 상하 스크롤만 가능하도록 제한하는 스크립트
/// </summary>
public class NonDraggableScrollRect : ScrollRect
{
    private bool IsScrollbarHandle(PointerEventData eventData)
    {
        // 현재 포인터가 닿은 UI 오브젝트
        GameObject go = eventData.pointerCurrentRaycast.gameObject;
        if (go == null)
            return false;

        // 수직 스크롤바 핸들이 있는 경우
        if (verticalScrollbar && go == verticalScrollbar.handleRect.gameObject)
            return true;

        // 수평 스크롤바 핸들이 있는 경우
        if (horizontalScrollbar && go == horizontalScrollbar.handleRect.gameObject)
            return true;

        return false;
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        // 스크롤바 핸들 위를 드래그한 경우만 기본 동작 수행
        if (IsScrollbarHandle(eventData))
            base.OnBeginDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (IsScrollbarHandle(eventData))
            base.OnDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (IsScrollbarHandle(eventData))
            base.OnEndDrag(eventData);
    }

    // 마우스 휠(스크롤) 이벤트는 그대로 허용
    public override void OnScroll(PointerEventData data)
    {
        base.OnScroll(data);
    }
}
