using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoraleEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// 대상 종족 (None 이면 전체)
    /// </summary>
    public CrewRace targetRace;

    /// <summary>
    /// 적용되는 사기 값
    /// </summary>
    /// <returns></returns>
    [Range(-20, 20)] public int value;

    /// <summary>
    /// 지속 년도
    /// </summary>
    public int duration;

    /// <summary>
    /// 시작 연도
    /// </summary>
    public int startYear;

    /// <summary>
    /// 끝나는 년도 프로퍼티
    /// </summary>
    public int EndYear => startYear + duration;

    /// <summary>
    /// 스프라이트 렌더러.
    /// </summary>
    public SpriteRenderer spriteRenderer;


    private GameObject tooltipInstance;

    /// <summary>
    /// 호버했을 때의 툴팁
    /// </summary>
    [SerializeField] private GameObject tooltipPrefab;

    private RectTransform tooltipRectTransform;
    private TextMeshProUGUI tooltipTextComponent;
    private CanvasGroup canvasGroup;
    private Canvas parentCanvas;

    private Coroutine fadeCoroutine;
    [SerializeField] private float fadeTime = 0.1f;

    private void Update()
    {
        if (tooltipInstance != null && tooltipInstance.activeSelf) UpdateTooltipPosition();
    }

    public void Initialize()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        string spritePath = "Sprites/UI/";
        string change = "";
        string changeAmount = "";

        if (value >= 0)
        {
            change = "up";
            changeAmount = $"+{value.ToString()}";
        }
        else
        {
            change = "down";
            changeAmount = value.ToString();
        }

        switch (targetRace)
        {
            case CrewRace.None:
                spritePath += "all_";
                break;
            case CrewRace.Human:
                spritePath += "human_";
                break;
            case CrewRace.Amorphous:
                spritePath += "amorphous_";
                break;
            case CrewRace.Beast:
                spritePath += "beast_";
                break;
            case CrewRace.Insect:
                spritePath += "insect_";
                break;
            case CrewRace.MechanicTank:
            case CrewRace.MechanicSup:
                spritePath += "mechanic_";
                break;
        }

        spritePath += change;

        // Canvas 찾기
        Canvas canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        parentCanvas = canvas;

        // 툴팁 인스턴스 생성
        tooltipInstance = Instantiate(tooltipPrefab, canvas.transform, false);

        // 필요한 컴포넌트 가져오기
        tooltipRectTransform = tooltipInstance.GetComponent<RectTransform>();
        tooltipTextComponent = tooltipInstance.GetComponentInChildren<TextMeshProUGUI>();
        canvasGroup = tooltipInstance.GetComponent<CanvasGroup>();

        if (tooltipRectTransform == null) Debug.LogError("tooltipRectTransform is null!");

        if (tooltipTextComponent == null) Debug.LogError("tooltipTextComponent is null!");

        if (canvasGroup == null) Debug.LogError("canvasGroup is null!");

        Debug.Log(spritePath);
        spriteRenderer.sprite = Resources.Load<Sprite>(spritePath);

        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();

        if (spriteRenderer != null && boxCollider != null && spriteRenderer.sprite != null)
        {
            // 스프라이트 크기에 맞게 콜라이더 크기 설정
            boxCollider.size = spriteRenderer.sprite.bounds.size;
            spriteRenderer.sortingOrder = SortingOrderConstants.UI;
        }

        SetTooltipText("ui.morale.tooltip".Localize(targetRace.ToString(), changeAmount));

        tooltipInstance.SetActive(false);
    }

    // 마우스가 오브젝트 위에 들어왔을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("OnPointerEnter");
        ShowTooltip();
    }

    // 마우스가 오브젝트에서 나갔을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    private void ShowTooltip()
    {
        if (tooltipInstance == null) return;

        // 위치 업데이트
        tooltipInstance.SetActive(true);
        UpdateTooltipPosition();

        // 이전 페이드 코루틴 중지
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        // 페이드 인 코루틴 시작
        fadeCoroutine = StartCoroutine(FadeTooltip(0f, 1f, fadeTime));
    }

    private void HideTooltip()
    {
        if (tooltipInstance == null) return;

        // 이전 페이드 코루틴 중지
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        // 페이드 아웃 코루틴 시작
        fadeCoroutine = StartCoroutine(FadeTooltip(1f, 0f, fadeTime));
    }

    private IEnumerator FadeTooltip(float startAlpha, float targetAlpha, float duration)
    {
        if (canvasGroup == null) yield break;

        float elapsedTime = 0f;
        canvasGroup.alpha = startAlpha;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        // 페이드 아웃 완료 후 비활성화
        if (targetAlpha <= 0f) tooltipInstance.SetActive(false);
    }

    public void SetTooltipText(string text)
    {
        if (tooltipTextComponent != null) tooltipTextComponent.text = text;
    }

    private void UpdateTooltipPosition()
    {
        if (tooltipRectTransform == null || parentCanvas == null) return;

        // 마우스 위치 가져오기
        Vector2 mousePosition = Input.mousePosition;

        // 스크린 좌표를 캔버스 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.GetComponent<RectTransform>(),
            mousePosition,
            parentCanvas.worldCamera,
            out Vector2 localPoint);

        // 툴팁 위치 설정 (마우스 커서 오른쪽 위에 표시)
        tooltipRectTransform.localPosition = localPoint + new Vector2(300f, -140f);

        // 화면 밖으로 나가지 않도록 조정
        Vector2 canvasSize = parentCanvas.GetComponent<RectTransform>().rect.size;
        Vector2 tooltipSize = tooltipRectTransform.rect.size;
        Vector2 tooltipPos = tooltipRectTransform.localPosition;

        // 오른쪽 경계 체크
        if (tooltipPos.x + tooltipSize.x > canvasSize.x / 2) tooltipPos.x = localPoint.x - tooltipSize.x - 15f;

        // 왼쪽 경계 체크
        if (tooltipPos.x - tooltipSize.x < -canvasSize.x / 2) tooltipPos.x = -canvasSize.x / 2 + tooltipSize.x;

        // 위쪽 경계 체크
        if (tooltipPos.y + tooltipSize.y > canvasSize.y / 2) tooltipPos.y = canvasSize.y / 2 - tooltipSize.y;

        // 아래쪽 경계 체크
        if (tooltipPos.y - tooltipSize.y < -canvasSize.y / 2) tooltipPos.y = localPoint.y + 15f;

        tooltipRectTransform.localPosition = tooltipPos;
    }

    // 스크립트가 비활성화되거나 파괴될 때 코루틴 정리
    private void OnDisable()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
    }

    // 정적 인스턴스 정리를 위한 메서드
    private void OnDestroy()
    {
        if (tooltipInstance != null)
        {
            Destroy(tooltipInstance);
            tooltipInstance = null;
        }
    }
}
