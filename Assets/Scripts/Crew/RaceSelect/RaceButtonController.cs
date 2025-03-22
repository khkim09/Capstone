using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class RaceButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Race Data")]
    public CrewRace raceType; // 버튼 종족
    public CrewRaceSettings crewRaceSettings; // scriptable object 할당
    public GameObject raceTooltipPrefab; // RaceTooltip 프리팹 할당

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
        if (raceTooltipPrefab == null)
            return;

        // Canvas 찾기
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
            return;

        // RaceTooltip 프리팹 생성
        activeTooltip = Instantiate(raceTooltipPrefab, canvas.transform);

        // RaceTooltip 설정
        RaceTooltip tooltip = activeTooltip.GetComponent<RaceTooltip>();
        tooltip.SetupTooltip(crewRaceSettings);

        // 스크린 → 월드 좌표로 변환
        RectTransform tooltipRect = activeTooltip.GetComponent<RectTransform>();

        // 월드 좌표 → UI 좌표로 변환
        Vector3 worldPosition = new Vector3(4.6f, 2.5f, 0.0f);
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        Vector2 localPoint;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, canvas.worldCamera, out localPoint);

        tooltipRect.localPosition = localPoint;
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
