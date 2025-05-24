using UnityEngine;

public class CombatEndingUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnWinClicked()
    {
        ResourceManager.Instance.ChangeResource(ResourceType.COMA, 1000);

        CombatManager.Instance.CombatDefeatedAndGoHome(false);
        GameManager.Instance.ChangeGameState(GameState.Warp);
        SceneChanger.Instance.CombatDefeatedAndGoHome(false);
    }

    public void OnLoseClicked()
    {
        int coma = ResourceManager.Instance.COMA;
        ResourceManager.Instance.SetResource(ResourceType.COMA, (int)(coma * 0.9f));
        //todo:근처 행성으로 이동시켜야됨

        CombatManager.Instance.CombatDefeatedAndGoHome(true);
        GameManager.Instance.ChangeGameState(GameState.Warp);
        SceneChanger.Instance.CombatDefeatedAndGoHome(true);
    }
}
