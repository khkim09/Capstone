using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 퀘스트 UI를 관리하는 클래스입니다.
/// 퀘스트 제안 및 완료 패널 표시, 수락/거절 처리 등을 담당합니다.
/// </summary>
public class QuestUIManager : MonoBehaviour
{
    /// <summary>퀘스트 제안 UI 패널</summary>
    public GameObject offerPanel;

    /// <summary>퀘스트 텍스트 (제목 + 설명)</summary>
    public TextMeshProUGUI questText;

    /// <summary>퀘스트 수락 버튼</summary>
    public Button acceptBtn;

    /// <summary>퀘스트 거절 버튼</summary>
    public Button declineBtn;

    /// <summary>퀘스트 완료 UI 패널</summary>
    public GameObject completePanel;

    /// <summary>퀘스트 완료 텍스트</summary>
    public TextMeshProUGUI completeText;

    /// <summary>퀘스트 완료 확인 버튼</summary>
    public Button completeBtn;

    /// <summary>현재 표시 중인 퀘스트 객체</summary>
    private RandomQuest currentQuest;

    /// <summary>완료 중 여부를 나타내는 플래그</summary>
    private bool isCompletingQuest = false;

    /// <summary>완료 전 QuestList 패널 상태 저장용</summary>
    private bool wasQuestListOpen = false;

    /// <summary>완료 전 QuestOffer 패널 상태 저장용</summary>
    private bool wasQuestOfferOpen = false;

    /// <summary>
    /// 버튼 클릭 이벤트를 등록합니다.
    /// </summary>
    private void Start()
    {
        offerPanel.SetActive(false);
        completePanel.SetActive(false);
        acceptBtn.onClick.AddListener(OnAccept);
        declineBtn.onClick.AddListener(OnDecline);
        completeBtn.onClick.AddListener(OnCompleteConfirmed);
    }

    /// <summary>
    /// 퀘스트 제안 UI를 표시합니다.
    /// </summary>
    /// <param name="quest">표시할 퀘스트</param>
    public void ShowQuestOffer(RandomQuest quest)
    {
        // Complete 중이면 ShowQuestOffer 방지
        if (isCompletingQuest)
            return;

        currentQuest = quest;
        offerPanel.SetActive(true);
        questText.text = "";
        StopAllCoroutines();
        StartCoroutine(TypeText(quest.title + "\n\n" + quest.description));
    }

    /// <summary>
    /// 퀘스트 텍스트를 타이핑 효과로 출력하는 코루틴입니다.
    /// </summary>
    /// <param name="text">출력할 텍스트</param>
    /// <returns>IEnumerator</returns>
    private System.Collections.IEnumerator TypeText(string text)
    {
        questText.text = "";
        foreach (char c in text)
        {
            questText.text += c;
            yield return new WaitForSeconds(0.015f);
        }
    }

    /// <summary>
    /// 수락 버튼 클릭 시 호출됩니다.
    /// 퀘스트를 수락하고 퀘스트 매니저에 등록합니다.
    /// 퀘스트 리스트 UI가 열려 있는 경우, 갱신합니다.
    /// </summary>
    private void OnAccept()
    {
        if (currentQuest != null)
        {
            currentQuest.Accept();
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.AddQuest(currentQuest);
            }
            else
            {
                Debug.LogWarning("QuestManager.Instance가 존재하지 않습니다.");
            }

            QuestListUI questListUI = FindObjectOfType<QuestListUI>();
            if (questListUI != null)
            {
                questListUI.Open();
            }
        }

        offerPanel.SetActive(false);
        currentQuest = null;
    }

    /// <summary>
    /// 거절 버튼 클릭 시 호출됩니다.
    /// 퀘스트를 거절하고 UI를 닫습니다.
    /// </summary>
    private void OnDecline()
    {
        if (currentQuest != null) currentQuest.Decline();

        offerPanel.SetActive(false);
        currentQuest = null;
    }

    /// <summary>
    /// 퀘스트 완료 UI를 표시합니다.
    /// 완료 전에 QuestList, QuestOffer 패널 상태를 저장하고 닫습니다.
    /// </summary>
    /// <param name="quest">완료된 퀘스트</param>
    public void ShowCompletion(RandomQuest quest)
    {
        isCompletingQuest = true;

        QuestListUI questListUI = FindObjectOfType<QuestListUI>();
        if (questListUI != null)
        {
            wasQuestListOpen = questListUI.IsOpen();
            if (wasQuestListOpen)
                questListUI.Close();
        }

        wasQuestOfferOpen = IsOfferPanelOpen();
        if (wasQuestOfferOpen)
            offerPanel.SetActive(false);

        completePanel.SetActive(true);
        completeText.text = "ui.quest.complete.text".Localize(quest.rewards[0].amount);
    }

    /// <summary>
    /// 퀘스트 완료 확인 버튼 클릭 시 호출됩니다.
    /// 완료 후 원래 QuestList, QuestOffer 패널을 복원합니다.
    /// </summary>
    private void OnCompleteConfirmed()
    {
        completePanel.SetActive(false);

        QuestListUI questListUI = FindObjectOfType<QuestListUI>();
        if (wasQuestListOpen && questListUI != null)
            questListUI.Open();

        if (wasQuestOfferOpen && !offerPanel.activeSelf)
            offerPanel.SetActive(true);

        wasQuestListOpen = false;
        wasQuestOfferOpen = false;
        isCompletingQuest = false;
    }

    /// <summary>
    /// 퀘스트 제안 패널이 열려있는지 여부를 반환합니다.
    /// </summary>
    public bool IsOfferPanelOpen()
    {
        return offerPanel.activeSelf;
    }

    /// <summary>
    /// 현재 퀘스트 완료 중인지 여부를 반환합니다.
    /// </summary>
    public bool IsCompletingQuest()
    {
        return isCompletingQuest;
    }
}
