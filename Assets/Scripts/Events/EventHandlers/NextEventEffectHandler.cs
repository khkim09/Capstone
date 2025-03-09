public class NextEventEffectHandler : ISpecialEffectHandler
{
    public void HandleEffect(EventOutcome outcome)
    {
        if (outcome.nextEvent != null) EventManager.Instance.TriggerEvent(outcome.nextEvent);
    }
}
