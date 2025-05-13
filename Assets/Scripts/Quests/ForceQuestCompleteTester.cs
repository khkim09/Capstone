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
    /// 활성화된 첫 번째 퀘스트의 모든 목표를 강제로 완료 처리합니다.
    /// </summary>
    private void ForceCompleteQuest()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("QuestManager.Instance가 존재하지 않습니다.");
            return;
        }

        QuestListUI questListUI = FindObjectOfType<QuestListUI>();
        if (questListUI != null)
            questListUI.Close();

        List<RandomQuest> quests = QuestManager.Instance.GetActiveQuests();
        if (quests.Count == 0)
        {
            Debug.LogWarning("완료시킬 퀘스트가 없습니다.");
            return;
        }

        RandomQuest quest = quests[0];

        // 모든 목표를 강제로 완료 처리
        for (int i = 0; i < quest.objectives.Count; i++)
        {
            int needed = quest.objectives[i].amount - quest.objectives[i].currentAmount;
            if (needed > 0)
                QuestManager.Instance.UpdateQuestObjective(quest.questId, i, needed);
        }

        Debug.Log($"퀘스트 강제 완료 시도됨: {quest.title}");
    }
}
