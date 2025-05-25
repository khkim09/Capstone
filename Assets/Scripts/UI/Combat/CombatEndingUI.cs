using UnityEngine;

public class CombatEndingUI : MonoBehaviour
{
    public MapPanelController mapPanelController;
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
        ResourceManager.Instance.ChangeResource(ResourceType.COMA, -(coma*0.3f));

        mapPanelController.gameObject.SetActive(true);

        CombatManager.Instance.CombatDefeatedAndGoHome(true);
        GameManager.Instance.ChangeGameState(GameState.Looser);
        mapPanelController.GoToThePlanet(mapPanelController.NearestPlanet());
        SceneChanger.Instance.CombatDefeatedAndGoHome(true);
    }
}
