using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class Planet : TooltipPanelBase
{
    [Header("PlanetData")] [SerializeField]
    private PlanetData planetData;

    [Header("행성 스프라이트 목록")] [SerializeField]
    private List<Sprite> planetSprites;

    private Sprite currentSprite;

    [Header("행성 거래 티어 스프라이트 목록")] [SerializeField]
    private List<Sprite> tradeTierSprites;

    [Header("행성 버튼")] [SerializeField] private Button planetButton;
    public System.Action<Planet> onClicked; // 외부에서 콜백 등록할 수 있게


    public PlanetData PlanetData => planetData;

    public bool HasEvent => planetData.activeEffects.Count != 0;
    public bool HasQuest => planetData.questsList.Count != 0;

    protected override void Start()
    {
        base.Start();

        // 게임 매니저에 연도 변경 이벤트 등록
        if (GameManager.Instance != null) GameManager.Instance.OnYearChanged += OnYearChanged;
    }

    private void OnEnable()
    {
        // 게임 매니저에 연도 변경 이벤트 등록
        if (GameManager.Instance != null) GameManager.Instance.OnYearChanged += OnYearChanged;
    }

    private void OnDisable()
    {
        // 게임 매니저에서 연도 변경 이벤트 해제
        if (GameManager.Instance != null) GameManager.Instance.OnYearChanged -= OnYearChanged;
    }

    private void OnClicked()
    {
        Debug.Log($"Planet clicked: {planetData.planetName}");

        // 외부에서 할당된 콜백 호출 (예: MapPanelController)
        onClicked?.Invoke(this);
    }

    /// <summary>
    /// 연도 변경 시 호출되는 이벤트 핸들러
    /// </summary>
    /// <param name="currentYear">현재 년도</param>
    private void OnYearChanged(int currentYear)
    {
        if (planetData != null)
            // 행성 효과 만료 체크
            planetData.CheckEventExpirations(currentYear);
    }

    protected override void SetToolTipText()
    {
        if (planetData == null || currentToolTip == null) return;

        PlanetTooltip planetTooltip = currentToolTip.GetComponent<PlanetTooltip>();

        if (planetTooltip != null)
        {
            planetTooltip.planetIcon.sprite = currentSprite;
            planetTooltip.planetNameText.text = planetData.planetName;
            planetTooltip.planetTradeTierIcon.sprite = tradeTierSprites[(int)planetData.currentTier];

            if (!HasQuest && !HasEvent)
            {
                planetTooltip.planetEventTitleText.text = "";
                planetTooltip.planetEventUpText.text = "";
                planetTooltip.planetEventDownText.text = "";

                planetTooltip.planetQuestTitleText.text = "";
                planetTooltip.planetCurrentQuestText.text = "";
                planetTooltip.planetDescriptionText.text =
                    $"{"ui.planetinfo." + planetData.itemPlanet.ToString().ToLower() + ".description"}".Localize();

                return;
            }

            planetTooltip.planetDescriptionText.text = "";
            planetTooltip.planetEventTitleText.text = "";
            planetTooltip.planetQuestTitleText.text = "ui.planetinfo.quest.title".Localize();
            if (HasEvent)
            {
                planetTooltip.planetEventTitleText.text = "ui.planetinfo.event.title".Localize();


                List<PlanetEffect> upList = planetData.activeEffects.Where(e => e.changeAmount > 0).ToList();
                List<PlanetEffect> downList = planetData.activeEffects.Where(e => e.changeAmount < 0).ToList();

                // 가격이 오르는 효과가 있으면
                if (upList.Count > 0)
                    planetTooltip.planetEventUpText.text =
                        $"- {string.Join(", ", upList.Select(up => $"{up.categoryType.Localize()} {up.changeAmount}%"))} {"ui.planetinfo.event.increase".Localize()}";

                if (downList.Count > 0)
                    planetTooltip.planetEventDownText.text =
                        $"- {string.Join(", ", downList.Select(down => $"{down.categoryType.Localize()} {down.changeAmount * -1}%"))} {"ui.planetinfo.event.decrease".Localize()}";
            }

            if (HasQuest)
            {
                List<RandomQuest> activeQuests = planetData.questsList
                    .Where(q => q.status == QuestStatus.Active)
                    .OrderByDescending(q => q.IsQuestCompleteReady())
                    .ToList();

                if (activeQuests.Count > 0)
                    planetTooltip.planetCurrentQuestText.text =
                        $"{activeQuests[0].title.Localize()} {(activeQuests.Count > 1 ? $" + {activeQuests.Count - 1}" : "")}";
            }
            else
            {
                planetTooltip.planetCurrentQuestText.text = "-";
            }
        }
    }

    public void SetPlanetData(PlanetData planetData)
    {
        this.planetData = planetData;
        currentSprite = planetSprites[(int)planetData.itemPlanet];
        GetComponentInChildren<Image>().sprite = currentSprite;

        if (planetButton != null)
        {
            planetButton.onClick.RemoveAllListeners(); // 중복 방지
            planetButton.onClick.AddListener(OnClicked);
        }
    }
}
