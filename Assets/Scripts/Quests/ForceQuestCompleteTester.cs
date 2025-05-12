using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 테스트용으로 퀘스트를 강제로 완료시키는 기능을 제공합니다.
/// </summary>
public class ForceQuestCompleteTester : MonoBehaviour
{
    /// <summary>퀘스트 완료 버튼</summary>
    public Button completeQuestButton;

    /// <summary>
    /// 시작 시 버튼 클릭 이벤트를 등록합니다.
    /// </summary>
    private void Start()
    {
        completeQuestButton.onClick.AddListener(ForceCompleteQuest);
    }

    /// <summary>
    /// 활성화된 첫 번째 퀘스트의 모든 목표를 강제로 완료시키고,
    /// 완료 전 패널 상태를 기억 후 완료 후 복구합니다.
    /// </summary>
    private void ForceCompleteQuest()
    {
        List<RandomQuest> quests = QuestManager.Instance.GetActiveQuests();
        if (quests.Count == 0)
        {
            Debug.LogWarning("완료시킬 퀘스트가 없습니다.");
            return;
        }

        RandomQuest quest = quests[0];

        // ✅ 상태 기억 (퀘스트 완료 전 패널 상태 저장)
        QuestListUI questListUI = FindObjectOfType<QuestListUI>();
        QuestUIManager questUI = FindObjectOfType<QuestUIManager>();

        bool wasQuestListOpen = questListUI != null && questListUI.IsOpen();
        bool wasQuestOfferOpen = questUI != null && questUI.IsOfferPanelOpen();

        // ✅ 모든 목표를 강제로 완료 처리
        for (int i = 0; i < quest.objectives.Count; i++)
        {
            int needed = quest.objectives[i].amount - quest.objectives[i].currentAmount;
            if (needed > 0)
                QuestManager.Instance.UpdateQuestObjective(quest.questId, i, needed);
        }

        // ✅ 완료 후 상태 복원
        if (wasQuestListOpen && questListUI != null)
            questListUI.Open();
        if (wasQuestOfferOpen && questUI != null)
            questUI.ShowQuestOffer(quest);

        Debug.Log($"퀘스트 강제 완료 시도됨: {quest.title}");
    }
}
