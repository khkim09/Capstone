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

    [SerializeField] public RectTransform tooltipRectTransform;
    private static TextMeshProUGUI tooltipTextComponent;
    private Coroutine fadeCoroutine;
    [SerializeField] private float fadeTime = 0.1f;
    [SerializeField] private static CanvasGroup canvasGroup;

    private void Update()
    {
        if (tooltipInstance.activeSelf) UpdateTooltipPosition();
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

        tooltipInstance = Instantiate(tooltipPrefab, GameObject.Find("EventCanvas").transform, false);

        spriteRenderer.sprite = Resources.Load<Sprite>(spritePath);

        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();

        if (spriteRenderer != null && boxCollider != null && spriteRenderer.sprite != null)
        {
            // 스프라이트 크기에 맞게 콜라이더 크기 설정
            boxCollider.size = spriteRenderer.sprite.bounds.size;
            spriteRenderer.sortingOrder = SortingOrderConstants.UI;
        }

        tooltipTextComponent = tooltipInstance.GetComponentInChildren<TextMeshProUGUI>();

        canvasGroup = tooltipInstance.GetComponent<CanvasGroup>();

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
        // 이전 페이드 코루틴 중지
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        // 페이드 아웃 코루틴 시작
        fadeCoroutine = StartCoroutine(FadeTooltip(1f, 0f, fadeTime));
    }

    private IEnumerator FadeTooltip(float startAlpha, float targetAlpha, float duration)
    {
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
        tooltipTextComponent.text = text;
    }

    private void UpdateTooltipPosition()
    {
        // 마우스 위치 가져오기
        Vector2 mousePosition = Input.mousePosition;

        // 툴팁 위치 설정 (마우스 커서 오른쪽 위에 표시)
        tooltipRectTransform.position = new Vector2(mousePosition.x + 15f, mousePosition.y - 15f);

        // 화면 밖으로 나가지 않도록 조정
        Canvas canvas = tooltipInstance.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            Vector2 canvasSize = canvas.GetComponent<RectTransform>().sizeDelta;
            Vector2 tooltipSize = tooltipRectTransform.sizeDelta;

            if (tooltipRectTransform.position.x + tooltipSize.x > canvasSize.x)
            {
                float newX = mousePosition.x - tooltipSize.x - 15f;
                tooltipRectTransform.position = new Vector2(newX, tooltipRectTransform.position.y);
            }

            if (tooltipRectTransform.position.y - tooltipSize.y < 0)
            {
                float newY = mousePosition.y + tooltipSize.y + 15f;
                tooltipRectTransform.position = new Vector2(tooltipRectTransform.position.x, newY);
            }
        }
    }

    // 스크립트가 비활성화되거나 파괴될 때 코루틴 정리
    private void OnDisable()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
    }

    // 정적 인스턴스 정리를 위한 메서드
    private void OnDestroy()
    {
        if (tooltipInstance != null && gameObject.GetInstanceID() == tooltipInstance.GetInstanceID())
        {
            Destroy(tooltipInstance);
            tooltipInstance = null;
        }
    }
}
