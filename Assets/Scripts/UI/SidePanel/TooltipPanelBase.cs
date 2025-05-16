using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public abstract class TooltipPanelBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] protected GameObject tooltipPrefab;

    protected GameObject currentToolTip;
    protected Canvas canvasComponent;
    protected RectTransform canvasRectTransform;
    protected bool isMouseOver = false;
    protected Coroutine toolTipCoroutine;

    protected virtual void Start()
    {
        // 캔버스 컴포넌트 찾기
        canvasComponent = GetComponentInParent<Canvas>();
        if (canvasComponent != null)
            canvasRectTransform = canvasComponent.transform as RectTransform;

        // 툴팁 생성
        CreateTooltip();
    }

    protected virtual void Update()
    {
        // 툴팁이 활성화되어 있고 마우스가 패널 위에 있을 때 지속적으로 위치 업데이트
        if (currentToolTip != null && currentToolTip.activeInHierarchy && isMouseOver)
            UpdateToolTipPositionFromMouse();
    }

    protected virtual void CreateTooltip()
    {
        if (tooltipPrefab != null && currentToolTip == null)
        {
            currentToolTip = Instantiate(tooltipPrefab, canvasComponent.transform);

            // Canvas Group 추가
            CanvasGroup canvasGroup = currentToolTip.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = currentToolTip.AddComponent<CanvasGroup>();

            // 비활성화
            currentToolTip.SetActive(false);
        }
    }

    // IPointerEnterHandler, IPointerExitHandler 구현
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        OnMouseEnter(eventData);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        OnMouseExit(eventData);
    }

    protected virtual void OnMouseEnter(PointerEventData eventData)
    {
        isMouseOver = true;

        // 기존 코루틴이 실행 중이면 중지
        if (toolTipCoroutine != null)
        {
            StopCoroutine(toolTipCoroutine);
            toolTipCoroutine = null;
        }

        // 0.3초 후 툴팁 표시 (툴팁이 비활성 상태일 때만)
        if (currentToolTip != null && !currentToolTip.activeInHierarchy)
            toolTipCoroutine = StartCoroutine(ShowToolTipWithDelay(eventData));
    }

    protected virtual void OnMouseExit(PointerEventData eventData)
    {
        isMouseOver = false;

        // 코루틴이 실행 중이면 중지
        if (toolTipCoroutine != null)
        {
            StopCoroutine(toolTipCoroutine);
            toolTipCoroutine = null;
        }

        // 툴팁 숨기기
        if (currentToolTip != null)
            currentToolTip.SetActive(false);
    }

    protected virtual void UpdateToolTipPositionFromMouse()
    {
        if (currentToolTip == null || canvasComponent == null) return;

        // 현재 마우스 위치를 화면 좌표에서 캔버스 로컬 좌표로 변환
        Vector2 mousePosition;
        Vector2 screenMousePosition = Input.mousePosition;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform, screenMousePosition, canvasComponent.worldCamera, out mousePosition);

        // 툴팁 위치 업데이트
        UpdateToolTipPositionInternal(mousePosition);
    }

    protected virtual void UpdateToolTipPosition(PointerEventData eventData)
    {
        if (currentToolTip == null || canvasComponent == null) return;

        Vector2 mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform, eventData.position, canvasComponent.worldCamera, out mousePosition);

        UpdateToolTipPositionInternal(mousePosition);
    }

    protected virtual void UpdateToolTipPositionInternal(Vector2 mousePosition)
    {
        RectTransform toolTipRect = currentToolTip.GetComponent<RectTransform>();
        if (toolTipRect == null) return;

        Vector2 toolTipSize = toolTipRect.sizeDelta;
        Vector2 canvasSize = canvasRectTransform.sizeDelta;

        float offsetX = 20f;
        float offsetY = 20f;

        Vector2 targetPosition = mousePosition;

        // 왼쪽 아래를 기본으로 시도
        bool canShowLeft = mousePosition.x - toolTipSize.x - offsetX >= -canvasSize.x / 2;
        bool canShowBelow = mousePosition.y - toolTipSize.y - offsetY >= -canvasSize.y / 2;

        float pivotX = canShowLeft ? 1f : 0f;
        float pivotY = canShowBelow ? 1f : 0f;
        toolTipRect.pivot = new Vector2(pivotX, pivotY);

        targetPosition.x += canShowLeft ? -offsetX : offsetX;
        targetPosition.y += canShowBelow ? -offsetY : offsetY;

        // 위치 적용
        toolTipRect.anchoredPosition = targetPosition;
    }

    protected virtual IEnumerator ShowToolTipWithDelay(PointerEventData eventData)
    {
        // 0.3초 대기
        yield return new WaitForSeconds(0.3f);

        // 마우스가 여전히 패널 위에 있는지 확인
        if (isMouseOver && currentToolTip != null && !currentToolTip.activeInHierarchy)
        {
            // Canvas Group 가져오기
            CanvasGroup canvasGroup = currentToolTip.GetComponent<CanvasGroup>();

            // 완전히 투명하게 만들어서 보이지 않게 함
            canvasGroup.alpha = 0f;

            // 툴팁 활성화
            currentToolTip.SetActive(true);

            // 툴팁 텍스트 설정 (자식 클래스에서 구현)
            SetToolTipText();

            // 다음 프레임에서 위치 조정 (레이아웃이 완전히 계산된 후)
            yield return null;

            // 위치 조정
            UpdateToolTipPosition(eventData);

            // 0.1초 후 서서히 나타나게 함
            yield return new WaitForSeconds(0.1f);

            // 마우스가 여전히 위에 있는지 재확인
            if (isMouseOver && currentToolTip != null && currentToolTip.activeInHierarchy)
                canvasGroup.alpha = 1f;
        }

        // 코루틴 참조 초기화
        toolTipCoroutine = null;
    }

    protected virtual void OnDestroy()
    {
        // 실행 중인 코루틴 정리
        if (toolTipCoroutine != null)
        {
            StopCoroutine(toolTipCoroutine);
            toolTipCoroutine = null;
        }

        // 툴팁이 존재하면 명시적으로 삭제
        if (currentToolTip != null)
        {
            Destroy(currentToolTip);
            currentToolTip = null;
        }
    }

    // 자식 클래스에서 구현해야 하는 추상 메서드
    /// <summary>
    /// 툴팁에 표시할 텍스트를 설정합니다.
    /// </summary>
    protected abstract void SetToolTipText();
}
