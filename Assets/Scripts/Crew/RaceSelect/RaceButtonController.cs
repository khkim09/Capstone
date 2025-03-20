using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class RaceButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Race Data")]
    public CrewRace raceType;                  // 이 버튼이 어떤 종족인지
    public GameObject raceTooltipPrefab;       // RaceTooltip 프리팹 할당

    private GameObject activeTooltip;

    // 버튼 하이라이트를 위해 참조
    private Button button;
    private Color normalColor;
    public Color highlightColor = Color.yellow;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
            normalColor = button.image.color;
    }

    // 마우스가 버튼에 들어왔을 때(hover)
    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowTooltip();
    }

    // 마우스가 버튼에서 나갔을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    // 버튼 클릭
    public void OnPointerClick(PointerEventData eventData)
    {
        // UIHandler 통해 현재 종족 선택 로직 실행
        CrewUIHandler.Instance.OnRaceButtonClicked(this);
    }

    // 현재 버튼이 선택되었을 때(하이라이트)
    public void SetHighlighted(bool highlighted)
    {
        if (button != null)
            button.image.color = highlighted ? highlightColor : normalColor;
    }

    private void ShowTooltip()
    {
        if (raceTooltipPrefab == null) return;

        // 씬 내 Canvas 찾기
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("씬에 Canvas가 없습니다!");
            return;
        }

        // RaceTooltip 프리팹 인스턴스화
        activeTooltip = Instantiate(raceTooltipPrefab, canvas.transform);

        // RaceTooltip 스크립트 세팅
        RaceTooltip tooltip = activeTooltip.GetComponent<RaceTooltip>();
        if (tooltip != null)
        {
            // 종족 데이터에 맞춰 툴팁 내용 업데이트
            tooltip.SetupTooltip(raceType);
            Debug.Log("종족 별 tooltip 호출 성공");
        }

        // 툴팁의 위치를 마우스 근처로 배치 (예시)
        RectTransform tooltipRect = activeTooltip.GetComponent<RectTransform>();
        tooltipRect.position = Input.mousePosition;
    }

    private void HideTooltip()
    {
        if (activeTooltip != null)
        {
            Destroy(activeTooltip);
            activeTooltip = null;
        }
    }
}
