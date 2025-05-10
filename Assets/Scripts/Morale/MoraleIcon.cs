using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MoraleIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    /// <summary>
    /// 종족 유형
    /// </summary>
    public CrewRace race;

    /// <summary>
    /// 아이콘에 표시되는 총 사기 변화량
    /// </summary>
    public int totalValue;

    /// <summary>
    /// 이미지 컴포넌트 (UI)
    /// </summary>
    public Image iconImage;

    /// <summary>
    /// 툴팁 관련 속성들
    /// </summary>
    [SerializeField] private GameObject tooltipPrefab;

    private GameObject tooltipInstance;
    private RectTransform tooltipRectTransform;
    private TextMeshProUGUI tooltipTextComponent;
    private CanvasGroup tooltipCanvasGroup;
    private Canvas parentCanvas;
    private Coroutine fadeCoroutine;
    [SerializeField] private float fadeTime = 0.1f;

    /// <summary>
    /// 이 아이콘을 통해 표시되는 개별 사기 효과들의 목록
    /// </summary>
    private List<MoraleEffectData> effectDataList = new();

    private void Update()
    {
        if (tooltipInstance != null && tooltipInstance.activeSelf)
            UpdateTooltipPosition();
    }

    /// <summary>
    /// 아이콘 초기화
    /// </summary>
    public void Initialize(CrewRace race)
    {
        this.race = race;

        // 이미지 컴포넌트 가져오기
        iconImage = GetComponent<Image>();
        if (iconImage == null) iconImage = gameObject.AddComponent<Image>();

        // 캔버스 찾기
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas not found!");
                return;
            }
        }

        parentCanvas = canvas;

        // 툴팁 인스턴스 생성
        tooltipInstance = Instantiate(tooltipPrefab, canvas.transform);

        // 필요한 컴포넌트 가져오기
        tooltipRectTransform = tooltipInstance.GetComponent<RectTransform>();
        tooltipTextComponent = tooltipInstance.GetComponentInChildren<TextMeshProUGUI>();
        tooltipCanvasGroup = tooltipInstance.GetComponent<CanvasGroup>();

        if (tooltipRectTransform == null)
            Debug.LogError("tooltipRectTransform is null!");

        if (tooltipTextComponent == null)
            Debug.LogError("tooltipTextComponent is null!");

        if (tooltipCanvasGroup == null)
            Debug.LogError("tooltipCanvasGroup is null!");

        // 기본 스프라이트 설정 (초기값 0으로 설정)
        totalValue = 0;
        UpdateSprite();

        tooltipInstance.SetActive(false);
    }

    /// <summary>
    /// 사기 효과 데이터 추가
    /// </summary>
    public void AddEffectData(MoraleEffectData data)
    {
        effectDataList.Add(data);
        RecalculateTotalValue();
        UpdateSprite();
        UpdateTooltipText();
    }

    /// <summary>
    /// 사기 효과 데이터 제거
    /// </summary>
    public void RemoveEffectData(MoraleEffectData data)
    {
        effectDataList.Remove(data);
        RecalculateTotalValue();
        UpdateSprite();
        UpdateTooltipText();
    }

    /// <summary>
    /// 모든 데이터 제거
    /// </summary>
    public void ClearEffectData()
    {
        effectDataList.Clear();
        totalValue = 0;
        UpdateSprite();
        UpdateTooltipText();
    }

    /// <summary>
    /// 전체 사기 값 재계산
    /// </summary>
    private void RecalculateTotalValue()
    {
        totalValue = 0;
        foreach (MoraleEffectData data in effectDataList) totalValue += data.value;
    }

    /// <summary>
    /// 스프라이트 업데이트
    /// </summary>
    private void UpdateSprite()
    {
        string spritePath = "Sprites/UI/";
        string change = totalValue >= 0 ? "up" : "down";

        switch (race)
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

        Sprite newSprite = Resources.Load<Sprite>(spritePath);
        if (newSprite == null)
        {
            Debug.LogError($"스프라이트를 찾을 수 없습니다: {spritePath}");
            return;
        }

        iconImage.sprite = newSprite;

        // Image의 Native Size로 설정
        iconImage.SetNativeSize();

        // RectTransform 크기 조정 (옵션)
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // 크기 조정이 필요한 경우 여기에 코드 추가
            // rectTransform.sizeDelta = new Vector2(50, 50);
        }
    }

    /// <summary>
    /// 툴팁 텍스트 업데이트
    /// </summary>
    private void UpdateTooltipText()
    {
        if (tooltipTextComponent == null) return;

        string raceStr = race.Localize();
        string changeAmount = totalValue >= 0 ? $"+{totalValue}" : totalValue.ToString();

        string baseText = "";


        if (race == CrewRace.None)
            baseText = "ui.morale.tooltip.4".Localize(changeAmount);
        else
            baseText = "ui.morale.tooltip.1".Localize(raceStr, changeAmount);


        // 세부 효과 목록 추가
        if (effectDataList.Count > 0)
        {
            baseText += "ui.morale.tooltip.2".Localize();

            foreach (MoraleEffectData data in effectDataList)
            {
                string effectValue = data.value >= 0 ? $"+{data.value}" : data.value.ToString();
                string source = !string.IsNullOrEmpty(data.source) ? $" ({data.source})" : "";
                int yearsLeft = data.EndYear - GameManager.Instance.CurrentYear;

                baseText += "ui.morale.tooltip.3".Localize(effectValue, yearsLeft);
            }
        }

        tooltipTextComponent.text = baseText;
    }

    // 마우스 이벤트 핸들러
    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowTooltip();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    private void ShowTooltip()
    {
        if (tooltipInstance == null) return;

        // 툴팁 텍스트 최신화
        UpdateTooltipText();

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
        if (tooltipCanvasGroup == null) yield break;

        float elapsedTime = 0f;
        tooltipCanvasGroup.alpha = startAlpha;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            tooltipCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            yield return null;
        }

        tooltipCanvasGroup.alpha = targetAlpha;

        // 페이드 아웃 완료 후 비활성화
        if (targetAlpha <= 0f) tooltipInstance.SetActive(false);
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
        tooltipRectTransform.localPosition = localPoint + new Vector2(290f, -140f);

        // 화면 밖으로 나가지 않도록 조정
        Vector2 canvasSize = parentCanvas.GetComponent<RectTransform>().rect.size;
        Vector2 tooltipSize = tooltipRectTransform.rect.size;
        Vector2 tooltipPos = tooltipRectTransform.localPosition;

        // 경계 체크 및 조정
        if (tooltipPos.x + tooltipSize.x > canvasSize.x / 2) tooltipPos.x = localPoint.x - tooltipSize.x - 15f;
        if (tooltipPos.x - tooltipSize.x < -canvasSize.x / 2) tooltipPos.x = -canvasSize.x / 2 + tooltipSize.x;
        if (tooltipPos.y + tooltipSize.y > canvasSize.y / 2) tooltipPos.y = canvasSize.y / 2 - tooltipSize.y;
        if (tooltipPos.y - tooltipSize.y < -canvasSize.y / 2) tooltipPos.y = localPoint.y + 15f;

        tooltipRectTransform.localPosition = tooltipPos;
    }

    // 정리 메서드
    private void OnDisable()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
    }

    private void OnDestroy()
    {
        if (tooltipInstance != null)
        {
            Destroy(tooltipInstance);
            tooltipInstance = null;
        }
    }
}
