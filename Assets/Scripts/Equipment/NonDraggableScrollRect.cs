using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// ScrollRect 기능을 확장하여, 스크롤바 핸들 외의 영역에서는 드래그를 비활성화하는 스크립트.
/// 마우스 휠 스크롤은 정상 작동하며, 수직 스크롤만 허용됩니다.
/// </summary>
public class NonDraggableScrollRect : ScrollRect
{
    /// <summary>
    /// 현재 포인터가 스크롤바 핸들 위에 있는지 여부를 판단합니다.
    /// </summary>
    /// <param name="eventData">이벤트 데이터.</param>
    /// <returns>스크롤바 핸들 위라면 true.</returns>
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

    /// <summary>
    /// 드래그 시작 시 호출됩니다.
    /// 스크롤바 핸들 위일 경우에만 기본 드래그 동작을 수행합니다.
    /// </summary>
    /// <param name="eventData">이벤트 데이터.</param>
    public override void OnBeginDrag(PointerEventData eventData)
    {
        // 스크롤바 핸들 위를 드래그한 경우만 기본 동작 수행
        if (IsScrollbarHandle(eventData))
            base.OnBeginDrag(eventData);
    }

    /// <summary>
    /// 드래그 중 호출됩니다.
    /// 스크롤바 핸들 위일 경우에만 기본 드래그 동작을 수행합니다.
    /// </summary>
    /// <param name="eventData">이벤트 데이터.</param>
    public override void OnDrag(PointerEventData eventData)
    {
        if (IsScrollbarHandle(eventData))
            base.OnDrag(eventData);
    }

    /// <summary>
    /// 드래그 종료 시 호출됩니다.
    /// 스크롤바 핸들 위일 경우에만 기본 드래그 동작을 수행합니다.
    /// </summary>
    /// <param name="eventData">이벤트 데이터.</param>
    public override void OnEndDrag(PointerEventData eventData)
    {
        if (IsScrollbarHandle(eventData))
            base.OnEndDrag(eventData);
    }

    /// <summary>
    /// 마우스 휠 스크롤 이벤트를 허용합니다.
    /// </summary>
    /// <param name="data">이벤트 데이터.</param>
    public override void OnScroll(PointerEventData data)
    {
        base.OnScroll(data);
    }
}
