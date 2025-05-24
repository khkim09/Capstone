public class BattleEffectHandler : ISpecialEffectHandler
{
    public void HandleEffect(EventOutcome outcome)
    {
        GameManager.Instance.ChangeGameState(GameState.Combat);
        SceneChanger.Instance.LoadScene("Combat");
    }
}
