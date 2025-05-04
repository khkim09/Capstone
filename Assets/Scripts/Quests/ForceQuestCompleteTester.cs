using UnityEngine;
using UnityEngine.UI;

public class ForceQuestCompleteTester : MonoBehaviour
{
    public Button completeQuestButton;

    private void Start()
    {
        completeQuestButton.onClick.AddListener(ForceCompleteQuest);
    }

    /// <summary>
    /// 활성화된 첫 번째 퀘스트의 모든 목표를 강제로 완료시킵니다.
    /// </summary>
    private void ForceCompleteQuest()
    {
        var quests = QuestManager.Instance.GetActiveQuests();
        if (quests.Count == 0)
        {
            Debug.LogWarning("완료시킬 퀘스트가 없습니다.");
            return;
        }

        var quest = quests[0];

        for (int i = 0; i < quest.objectives.Count; i++)
        {
            int needed = quest.objectives[i].requiredAmount - quest.objectives[i].currentAmount;
            if (needed > 0)
            {
                QuestManager.Instance.UpdateQuestObjective(quest.id, i, needed);
            }
        }

        // 여기서 완료 UI 강제 호출
        QuestUIManager ui = FindObjectOfType<QuestUIManager>();
        if (ui != null)
        {
            ui.ShowCompletion(quest); // ✅ 여기서 quest는 QuestManager.Quest 타입
        }

        Debug.Log($"퀘스트 강제 완료 시도됨: {quest.title}");
    }

}
