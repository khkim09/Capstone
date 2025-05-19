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
        if (completeQuestButton != null)
            completeQuestButton.onClick.AddListener(ForceCompleteQuest);
        else
            Debug.LogWarning("퀘스트 완료 버튼이 할당되지 않았습니다.");
    }

    /// <summary>
    /// OnDestroy에서 리스너 제거
    /// </summary>
    private void OnDestroy()
    {
        if (completeQuestButton != null)
            completeQuestButton.onClick.RemoveListener(ForceCompleteQuest);
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
            Debug.LogWarning("완료 가능한 퀘스트가 없습니다.");
            return;
        }

        RandomQuest quest = quests[Random.Range(0, quests.Count)];
        foreach (var obj in quest.objectives)
        {
            obj.currentAmount = obj.amount;
            obj.isCompleted = true;
        }

        Debug.Log($"랜덤 퀘스트 완료 가능 상태로 변경함: {quest.title}");

        // GetCurrentPlanet 사용 전 QuestUIManager.Instance가 null인지 확인
        Planet planet = QuestUIManager.Instance != null ? QuestUIManager.Instance.GetCurrentPlanet() : null;
        if (questListUI != null && planet != null)
            questListUI.OpenQuestListForPlanet(planet);
    }
}
