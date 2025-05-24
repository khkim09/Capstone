using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public abstract class TooltipPanelBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// 표시할 툴팁 프리팹
    /// </summary>
    [SerializeField] protected GameObject tooltipPrefab;

    /// <summary>
    /// 툴팁 참조
    /// </summary>
    protected GameObject currentToolTip;

    /// <summary>
    /// 캔버스 참조
    /// </summary>
    protected Canvas canvasComponent;

    /// <summary>
    /// 캔버스의 RectTransform 참조
    /// </summary>
    protected RectTransform canvasRectTransform;

    /// <summary>
    /// 마우스가 오브젝트 위에 있는지 여부
    /// </summary>
    protected bool isMouseOver = false;

    /// <summary>
    /// 툴팁 코루틴
    /// </summary>
    protected Coroutine toolTipCoroutine;

    /// <summary>
    /// 툴팁 한 곳에 모아둘 부모
    /// </summary>
    protected GameObject tooltipParent;

    /// <summary>
    /// 마지막 마우스 위치 저장 (위치 업데이트 최적화용)
    /// </summary>
    protected Vector2 lastMousePosition;

    protected virtual void Start()
    {
        // 캔버스 컴포넌트 찾기
        canvasComponent = GetComponentInParent<Canvas>();
        if (canvasComponent != null)
            canvasRectTransform = canvasComponent.transform as RectTransform;

        tooltipParent = GameObject.FindWithTag("TooltipParent");

        if (tooltipParent == null)
        {
            tooltipParent = new GameObject("TooltipParent");
            tooltipParent.tag = "TooltipParent";
            tooltipParent.transform.SetParent(canvasComponent.transform, false);
        }

        // 툴팁 생성
        CreateTooltip();
    }

    protected virtual void Update()
    {
        // 수정: 마우스 위치가 변경됐을 때만 업데이트
        if (currentToolTip != null && currentToolTip.activeInHierarchy && isMouseOver)
        {
            Vector2 currentMousePos = Input.mousePosition;
            if (Vector2.Distance(currentMousePos, lastMousePosition) > 1f) // 1픽셀 이상 움직였을 때만
            {
                lastMousePosition = currentMousePos;
                UpdateToolTipPositionFromMouse();
            }
        }
    }

    protected virtual void CreateTooltip()
    {
        if (tooltipPrefab != null && currentToolTip == null)
        {
            if (tooltipParent != null)
                currentToolTip = Instantiate(tooltipPrefab, tooltipParent.transform);
            else
                currentToolTip = Instantiate(tooltipPrefab, canvasComponent.transform);

            // Canvas Group 추가
            CanvasGroup canvasGroup = currentToolTip.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = currentToolTip.AddComponent<CanvasGroup>();

            // 툴팁이 raycast를 차단하지 않도록 설정 (중요!)
            canvasGroup.blocksRaycasts = false;

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
        lastMousePosition = Input.mousePosition; // 마우스 위치 저장

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

        // 툴팁이 활성화된 직후에만 레이아웃 재계산 (최적화)
        if (toolTipRect.GetComponent<ContentSizeFitter>() != null ||
            toolTipRect.GetComponent<LayoutGroup>() != null)
        {
            // 이미 계산된 경우 다시 계산하지 않음
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(toolTipRect);
        }

        Vector2 toolTipSize = toolTipRect.sizeDelta;
        Vector2 canvasSize = canvasRectTransform.sizeDelta;

        float offsetX = 20f;
        float offsetY = 20f;

        Vector2 targetPosition = mousePosition;

        // 수정: 더 명확한 위치 계산 로직
        // 오른쪽에 표시할 수 있는지 확인
        bool canShowRight = mousePosition.x + toolTipSize.x + offsetX <= canvasSize.x / 2;

        // 아래쪽에 표시할 수 있는지 확인
        bool canShowBelow = mousePosition.y - toolTipSize.y - offsetY >= -canvasSize.y / 2;

        // 우선순위: 오른쪽-아래 > 오른쪽-위 > 왼쪽-아래 > 왼쪽-위
        float pivotX, pivotY;

        if (canShowRight)
        {
            // 오른쪽에 표시
            pivotX = 0f; // 툴팁의 왼쪽 끝이 기준점
            targetPosition.x += offsetX;
        }
        else
        {
            // 왼쪽에 표시
            pivotX = 1f; // 툴팁의 오른쪽 끝이 기준점
            targetPosition.x -= offsetX;
        }

        if (canShowBelow)
        {
            // 아래쪽에 표시
            pivotY = 1f; // 툴팁의 위쪽 끝이 기준점
            targetPosition.y -= offsetY;
        }
        else
        {
            // 위쪽에 표시
            pivotY = 0f; // 툴팁의 아래쪽 끝이 기준점
            targetPosition.y += offsetY;
        }

        toolTipRect.pivot = new Vector2(pivotX, pivotY);

        // 위치 적용
        toolTipRect.anchoredPosition = targetPosition;
    }

    protected virtual IEnumerator ShowToolTipWithDelay(PointerEventData eventData)
    {
        // 0.2초 대기
        yield return new WaitForSeconds(0.2f);

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

            // 레이아웃 계산 완료 대기
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
