using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 퀘스트 UI 관리하는 클래스
/// </summary>
public class QuestUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject questPanel;
    [SerializeField] private TextMeshProUGUI questTitleText;
    [SerializeField] private TextMeshProUGUI questDescriptionText;
    [SerializeField] private Transform objectivesContainer;  // 목표 목록을 표시할 부모 오브젝트
    [SerializeField] private GameObject objectiveItemPrefab;   // 각 목표 항목 프리팹 (TextMeshProUGUI 포함)
    [SerializeField] private GameObject rewardsPanel;
    [SerializeField] private TextMeshProUGUI rewardsText;
    [SerializeField] private Button completeButton;

    [Header("Settings")]
    [SerializeField] private float textTypingSpeed = 0.05f;

    private RandomQuest currentQuest;
    public static QuestUIManager Instance { get; private set; }

    /// <summary>
    /// 로드될 때 호출
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// 시작시 호출
    /// </summary>
    private void Start()
    {
        questPanel.SetActive(false);
        rewardsPanel.SetActive(false);
        completeButton.onClick.AddListener(OnCompleteButtonClicked);
    }

    /// <summary>
    /// 퀘스트 데이터를 받아 UI에 표시
    /// </summary>
    /// <param name="quest"></param>
    public void ShowQuest(RandomQuest quest)
    {
        currentQuest = quest;
        questPanel.SetActive(true);

        // 퀘스트 제목 및 설명 설정 (타이핑 효과 적용)
        questTitleText.text = quest.title;
        StartCoroutine(TypeText(questDescriptionText, quest.description));

        // 목표와 보상 UI 설정
        SetupObjectives(quest);
        SetupRewards(quest);

        // 모든 목표가 완료된 경우에만 완료 버튼 활성화
        bool allObjectivesComplete = true;
        foreach (var objective in quest.objectives)
        {
            if (!objective.isCompleted)
            {
                allObjectivesComplete = false;
                break;
            }
        }
        completeButton.interactable = allObjectivesComplete;
    }

    /// <summary>
    /// 목표 목록을 UI에 동적으로 생성
    /// </summary>
    /// <param name="quest"></param>
    private void SetupObjectives(RandomQuest quest)
    {
        // 기존 목표 아이템 제거
        foreach (Transform child in objectivesContainer)
        {
            Destroy(child.gameObject);
        }

        // 각 목표 항목 생성
        foreach (var objective in quest.objectives)
        {
            GameObject objItem = Instantiate(objectiveItemPrefab, objectivesContainer);
            TextMeshProUGUI objText = objItem.GetComponentInChildren<TextMeshProUGUI>();
            if (objText != null)
            {
                objText.text = $"{objective.description} ({objective.currentAmount}/{objective.requiredAmount})";
            }
        }
    }

    /// <summary>
    /// 퀘스트가 완료된 경우 보상 정보를 UI에 표시
    /// </summary>
    /// <param name="quest"></param>
    private void SetupRewards(RandomQuest quest)
    {
        // 퀘스트 상태가 완료라면 보상 패널 활성화
        if (quest.status == RandomQuest.QuestStatus.Completed)
        {
            rewardsPanel.SetActive(true);
            string rewardDetails = "";
            foreach (var reward in quest.rewards)
            {
                rewardDetails += $"{reward.type} : {reward.amount}\n";
            }
            rewardsText.text = rewardDetails;
        }
        else
        {
            rewardsPanel.SetActive(false);
        }
    }


    /// <summary>
    /// 텍스트 타입
    /// </summary>
    /// <param name="textComponent"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    private IEnumerator TypeText(TextMeshProUGUI textComponent, string text)
    {
        textComponent.text = "";
        foreach (var c in text)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textTypingSpeed);
        }
    }

    /// <summary>
    /// 완료버튼 누를시 호ㅜㄹ
    /// </summary>
    private void OnCompleteButtonClicked()
    {
        questPanel.SetActive(false);
        // 퀘스트 완료 처리 (예: QuestManager 메서드 호출)
        QuestManager.Instance.TriggerQuestCompleted(currentQuest.ToQuest());

    }
}
