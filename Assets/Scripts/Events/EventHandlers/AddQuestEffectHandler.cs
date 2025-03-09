public class AddQuestEffectHandler : ISpecialEffectHandler
{
    public void HandleEffect(EventOutcome outcome)
    {
        // 퀘스트 관련 데이터가 추가될 수 있음
        // outcome.questToAdd와 같은 필드를 추가할 수 있음
        //QuestManager.Instance.AddQuest(outcome.questData);
    }
}
