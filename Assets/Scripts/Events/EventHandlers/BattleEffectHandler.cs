public class BattleEffectHandler : ISpecialEffectHandler
{
    public void HandleEffect(EventOutcome outcome)
    {
        EventManager.Instance.EventPanelController.ContinueButtonChange();
    }
}
