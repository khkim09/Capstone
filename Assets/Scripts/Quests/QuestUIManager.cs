using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUIManager : MonoBehaviour
{
    [Header("제안 UI")]
    [SerializeField] private GameObject offerPanel;
    [SerializeField] private TextMeshProUGUI offerTitle;
    [SerializeField] private TextMeshProUGUI offerDesc;
    [SerializeField] private Button acceptBtn;
    [SerializeField] private Button declineBtn;

    [Header("진행 UI")]
    [SerializeField] private GameObject progressPanel;
    [SerializeField] private Transform objectivesContainer;
    [SerializeField] private GameObject objectivePrefab;

    [Header("완료 UI")]
    [SerializeField] private GameObject completePanel;
    [SerializeField] private TextMeshProUGUI completeTitle;
    [SerializeField] private TextMeshProUGUI completeRewards;
    [SerializeField] private Button confirmBtn;

    [Header("타이핑 효과 속도")]
    [SerializeField] private float typingSpeed = 0.05f;

    private RandomQuest currentQuest;

    private void Awake()
    {
        acceptBtn.onClick.AddListener(OnAccept);
        declineBtn.onClick.AddListener(OnDecline);
        confirmBtn.onClick.AddListener(() => completePanel.SetActive(false));

        // Storage에서 어떤 아이템이 바뀌어도 진행 UI 갱신
        if (Storage.Instance != null)
        {
            Storage.Instance.OnStorageChanged += _ =>
            {
                if (currentQuest != null)
                    RefreshObjectives();
            };
        }
    }

    private void Start()
    {
        offerPanel.SetActive(false);
        progressPanel.SetActive(false);
        completePanel.SetActive(false);
    }

    /// <summary>
    /// 퀘스트 제안창 띄우기
    /// </summary>
    public void ShowQuestOffer(RandomQuest rq)
    {
        currentQuest = rq;
        rq.status = RandomQuest.QuestStatus.NotStarted;

        offerPanel.SetActive(true);
        progressPanel.SetActive(false);
        completePanel.SetActive(false);

        offerTitle.text = rq.title;
        StartCoroutine(TypeText(offerDesc, rq.description));
    }

    private void OnAccept()
    {
        if (currentQuest == null) return;
        currentQuest.Accept();
        offerPanel.SetActive(false);

        // QuestManager에 런타임 데이터 등록
        QuestManager.Instance.AddQuest(currentQuest.ToQuest());

        // 진행 UI 열기 및 초기 갱신
        progressPanel.SetActive(true);
        RefreshObjectives();
    }

    private void OnDecline()
    {
        if (currentQuest == null) return;
        currentQuest.Decline();
        offerPanel.SetActive(false);
    }

    /// <summary>
    /// 목표 UI 갱신 및 완료 체크
    /// </summary>
    public void RefreshObjectives()
    {
        // 1) 각 목표의 targetId로 창고 수량 조회
        foreach (var o in currentQuest.objectives)
        {
            int qty = Storage.Instance.GetItemQuantityById(o.targetId);
            o.currentAmount = qty;
            o.isCompleted   = (qty >= o.requiredAmount);
        }

        // 2) UI 목록 갱신
        foreach (Transform t in objectivesContainer)
            Destroy(t.gameObject);

        foreach (var o in currentQuest.objectives)
        {
            var go  = Instantiate(objectivePrefab, objectivesContainer);
            var txt = go.GetComponentInChildren<TextMeshProUGUI>();
            txt.text = $"{o.description} ({o.currentAmount}/{o.requiredAmount})";
            if (o.isCompleted)
                txt.color = Color.green;
        }

        // 3) 자동 완료 체크
        currentQuest.CheckCompletion();
        if (currentQuest.status == RandomQuest.QuestStatus.Completed)
            ShowCompletion();
    }

    /// <summary>
    /// 완료 UI 표시 및 보상 텍스트
    /// </summary>
    private void ShowCompletion()
    {
        completePanel.SetActive(true);
        completeTitle.text = $"퀘스트 완료: {currentQuest.title}";

        string detail = "";
        foreach (var r in currentQuest.rewards)
            detail += $"{r.type}: {r.amount}\n";
        completeRewards.text = detail;

        // QuestManager에 완료 알림
        QuestManager.Instance.TriggerQuestCompleted(currentQuest.ToQuest());
    }

    private IEnumerator TypeText(TextMeshProUGUI comp, string msg)
    {
        comp.text = "";
        foreach (var c in msg)
        {
            comp.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
