using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

/// <summary>
/// 선원 생성 시, UI를 통한 종족 선택
/// </summary>
public class RaceButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    /// <summary>
    /// 종족 타입
    /// </summary>
    [Header("Race Data")] public CrewRace raceType; // 버튼 종족

    /// <summary>
    /// 종족 별 기본 스텟
    /// </summary>
    [FormerlySerializedAs("crewRaceSettings")]
    public CrewRaceStat crewRaceStat; // scriptable object 할당

    /// <summary>
    /// 종족 선택 시 호출될 tooltip
    /// </summary>
    public GameObject raceTooltipPrefab; // RaceTooltip 프리팹 할당

    /// <summary>
    /// 활성화 된 race tooltip
    /// </summary>
    private GameObject activeTooltip;

    /// <summary>
    /// 작업에 필요한 mainCanvas
    /// </summary>
    [SerializeField] Canvas mainCanvas;

    /// <summary>
    /// 버튼 highlight를 위한 참조
    /// </summary>
    private Button button;
    private Color normalColor;
    public Color highlightColor = Color.yellow;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
            normalColor = button.image.color;
    }

    /// <summary>
    /// 마우스가 버튼에 들어왔을 때(hover)
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowTooltip();
    }

    /// <summary>
    /// 마우스가 버튼에서 나갔을 때
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    /// <summary>
    /// 버튼 클릭
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // UIHandler 통해 현재 종족 선택 로직 실행
        CrewUIHandler.Instance.OnRaceButtonClicked(this);
    }

    /// <summary>
    /// 현재 버튼이 선택되었을 때(하이라이트)
    /// </summary>
    /// <param name="highlighted"></param>
    public void SetHighlighted(bool highlighted)
    {
        if (button != null)
            button.image.color = highlighted ? highlightColor : normalColor;
    }

    /// <summary>
    /// 실제 tooltip 게임 화면에 띄우기
    /// </summary>
    private void ShowTooltip()
    {
        if (raceTooltipPrefab == null)
            return;

        // RaceTooltip 프리팹 생성
        activeTooltip = Instantiate(raceTooltipPrefab, mainCanvas.transform);

        // RaceTooltip 설정
        RaceTooltip tooltip = activeTooltip.GetComponent<RaceTooltip>();
        tooltip.SetupTooltip(crewRaceStat, raceType);

        // 스크린 → 월드 좌표로 변환
        RectTransform tooltipRect = activeTooltip.GetComponent<RectTransform>();

        // 월드 좌표 → UI 좌표로 변환
        tooltipRect.anchoredPosition = new Vector2(1500f, -270f);
    }

    /// <summary>
    /// tooltip 가리기
    /// </summary>
    private void HideTooltip()
    {
        if (activeTooltip != null)
        {
            Destroy(activeTooltip);
            activeTooltip = null;
        }
    }
}
