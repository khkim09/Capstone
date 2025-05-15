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
            Debug.LogWarning("완료 가능한 상태로 만들 퀘스트가 없습니다.");
            return;
        }

        // 랜덤으로 하나 선택
        RandomQuest quest = quests[Random.Range(0, quests.Count)];

        // 모든 목표의 currentAmount를 amount로 맞춰서 "완료 가능" 상태로 만듬
        for (int i = 0; i < quest.objectives.Count; i++)
        {
            quest.objectives[i].currentAmount = quest.objectives[i].amount;
            quest.objectives[i].isCompleted = true;
        }

        Debug.Log($"랜덤 퀘스트를 완료 가능 상태로 변경함: {quest.title}");

        // 리스트 갱신
        if (questListUI != null)
            questListUI.Open();
    }

}
