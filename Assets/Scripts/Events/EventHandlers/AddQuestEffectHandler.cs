using UnityEngine;

/// <summary>
/// 이벤트 결과로 퀘스트를 추가하는 특수 효과 핸들러입니다.
/// </summary>
public class AddQuestEffectHandler : ISpecialEffectHandler
{
    /// <summary>
    /// EventOutcome에서 지정된 퀘스트를 퀘스트 매니저에 등록합니다.
    /// </summary>
    /// <param name="outcome">이벤트 결과</param>
    public void HandleEffect(EventOutcome outcome)
    {
        if (outcome == null || outcome.questToAdd == null)
        {
            Debug.LogWarning("[AddQuestEffectHandler] 등록할 퀘스트가 없습니다.");
            return;
        }

        var newQuest = outcome.questToAdd.ToQuest();

        // 중복 등록 방지
        bool alreadyExists = QuestManager.Instance.GetActiveQuests()
            .Exists(q => q.id == newQuest.id);

        if (alreadyExists)
        {
            Debug.LogWarning($"[AddQuestEffectHandler] 이미 등록된 퀘스트입니다: {newQuest.title}");
            return;
        }

        QuestManager.Instance.AddQuest(newQuest);
        Debug.Log($"[AddQuestEffectHandler] 퀘스트가 성공적으로 추가되었습니다: {newQuest.title}");
    }
}
