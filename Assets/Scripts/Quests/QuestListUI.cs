using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 수락한 퀘스트들을 보여주는 UI 패널을 관리합니다.
/// 완료 가능한 퀘스트는 리스트 상단으로 올리고, 패널 이미지를 변경합니다.
/// </summary>
public class QuestListUI : MonoBehaviour
{
    /// <summary>퀘스트 패널 루트</summary>
    public GameObject panel;

    /// <summary>퀘스트 슬롯 프리팹</summary>
    public GameObject questSlotPrefab;

    /// <summary>퀘스트 슬롯이 들어갈 부모 오브젝트</summary>
    public Transform contentParent;

    /// <summary>현재 표시 중인 퀘스트 슬롯 목록</summary>
    private List<GameObject> spawnedSlots = new();

    /// <summary>실버 패널 Sprite (기본)</summary>
    private static Sprite silverPanelSprite;

    /// <summary>골드 패널 Sprite (완료 가능 시)</summary>
    private static Sprite goldPanelSprite;

    /// <summary>
    /// 시작할 때 실행되는 함수입니다.
    /// 처음에는 패널을 꺼두고, Sprite를 캐싱합니다.
    /// </summary>
    private void Awake()
    {
        if (silverPanelSprite == null)
            silverPanelSprite = Resources.Load<Sprite>("Sprites/Pixel Art Sci-Fi UI/Sprites/UI_Silver/Panels/Title C");

        if (goldPanelSprite == null)
            goldPanelSprite = Resources.Load<Sprite>("Sprites/Pixel Art Sci-Fi UI/Sprites/UI_Gold/Panels/Title C");
    }

    /// <summary>
    /// 시작 시 QuestCompleted 이벤트를 연결하고 패널을 꺼둡니다.
    /// </summary>
    private void Start()
    {
        panel.SetActive(false);
        QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
    }

    /// <summary>
    /// 퀘스트 목록 창을 열고 목록을 표시합니다.
    /// 완료 가능한 퀘스트를 먼저 추가한 뒤 진행 중 퀘스트를 추가합니다.
    /// </summary>
    public void Open()
    {
        Clear();
        panel.SetActive(true);

        List<RandomQuest> quests = QuestManager.Instance.GetActiveQuests();

        // 완료 가능한 퀘스트 먼저
        foreach (RandomQuest quest in quests)
            if (IsQuestCompleteReady(quest))
                AddToQuestListPanel(quest, true);

        // 나머지 진행 중 퀘스트
        foreach (RandomQuest quest in quests)
            if (!IsQuestCompleteReady(quest))
                AddToQuestListPanel(quest, false);
    }

    /// <summary>
    /// 외부 버튼에서 호출될 때 사용되는 열기/닫기 함수입니다.
    /// </summary>
    public void ToggleFromButton()
    {
        if (panel.activeSelf)
            Close();
        else
            Open();
    }

    /// <summary>
    /// 퀘스트 목록 창을 닫고 기존 슬롯들을 제거합니다.
    /// </summary>
    public void Close()
    {
        panel.SetActive(false);
        Clear();
    }

    /// <summary>
    /// 기존에 생성된 모든 퀘스트 슬롯들을 제거합니다.
    /// </summary>
    private void Clear()
    {
        foreach (GameObject slot in spawnedSlots)
            Destroy(slot);
        spawnedSlots.Clear();
    }

    /// <summary>
    /// 퀘스트 완료 시 호출되어 리스트를 자동 갱신합니다.
    /// </summary>
    private void OnQuestCompleted(RandomQuest quest)
    {
        if (IsOpen())
            Open();
    }

    /// <summary>
    /// 현재 패널이 열려 있는지 여부를 반환합니다.
    /// </summary>
    public bool IsOpen()
    {
        return panel.activeSelf;
    }

    /// <summary>
    /// 완료 준비 상태인지 판별합니다.
    /// 모든 목표가 완료된 활성 퀘스트만 true를 반환합니다.
    /// </summary>
    private bool IsQuestCompleteReady(RandomQuest quest)
    {
        if (quest.status != QuestStatus.Active)
            return false;

        foreach (QuestObjective obj in quest.objectives)
            if (!obj.isCompleted)
                return false;

        return true;
    }

    /// <summary>
    /// 퀘스트 슬롯을 생성하여 리스트에 추가합니다.
    /// 완료 가능한 경우 Sprite를 골드로 변경합니다.
    /// </summary>
    private void AddToQuestListPanel(RandomQuest quest, bool isCompleteReady = false)
    {
        GameObject slot = Instantiate(questSlotPrefab, contentParent);
        slot.SetActive(true);

        TextMeshProUGUI titleText = slot.transform.Find("QuestTitle")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descText = slot.transform.Find("QuestDescription")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI statusText = slot.transform.Find("QuestStatus")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI deadlineText = slot.transform.Find("QuestDeadline")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI rewardText = slot.transform.Find("QuestReward")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI planetText = slot.transform.Find("QuestPlanet")?.GetComponent<TextMeshProUGUI>();

        if (titleText != null)
            titleText.text = quest.title;

        if (descText != null)
            descText.text = quest.description;

        if (statusText != null)
            statusText.text = quest.status.ToString();

        if (deadlineText != null)
        {
            int currentYear = GameManager.Instance.CurrentYear;
            int yearsPassed = currentYear - quest.questAcceptedYear;
            int yearsLeft = Mathf.Max(0, 20 - yearsPassed);
            deadlineText.text = $"남은연도: {yearsLeft}";
        }

        if (rewardText != null && quest.rewards.Count > 0)
        {
            QuestReward reward = quest.rewards[0];
            rewardText.text = $"{reward.amount} {reward.questRewardType}";
        }

        if (planetText != null)
        {
            // objective 중 destinationPlanetId를 가진 첫 목표 가져오기
            string planetId = "-";
            foreach (var obj in quest.objectives)
            {
                if (!string.IsNullOrEmpty(obj.destinationPlanetId))
                {
                    planetId = obj.destinationPlanetId;
                    break;
                }
            }
            planetText.text = planetId;
        }

        // 패널 이미지 변경 (Silver / Gold)
        Image background = slot.GetComponent<Image>();
        if (background != null)
            background.sprite = isCompleteReady ? goldPanelSprite : silverPanelSprite;

        // 슬롯 클릭 시 완료 UI 호출
        Button btn = slot.GetComponent<Button>();
        if (btn != null && isCompleteReady)
        {
            btn.onClick.AddListener(() =>
            {
                QuestUIManager ui = FindObjectOfType<QuestUIManager>();
                if (ui != null)
                    ui.ShowCompletion(quest);
            });
        }

        // 취소 버튼 연결
        Button failButton = slot.transform.Find("FailButton")?.GetComponent<Button>();
        if (failButton != null)
        {
            failButton.onClick.AddListener(() =>
            {
                QuestManager.Instance.RequestFailQuest(quest);
                Open();
            });
        }

        spawnedSlots.Add(slot);
    }
}
