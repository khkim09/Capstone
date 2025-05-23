using UnityEngine;

public class CombatEndingUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void OnWinClicked()
    {
        ResourceManager.Instance.ChangeResource(ResourceType.COMA,1000);

        GameManager.Instance.ChangeGameState(GameState.Gameplay);
        SceneChanger.Instance.LoadScene("Idle");
    }

    public void OnLoseClicked()
    {
        int coma = ResourceManager.Instance.COMA;
        ResourceManager.Instance.SetResource(ResourceType.COMA, (int)(coma * 0.9f));
        //todo: 무역품 날리고 근처 행성으로 이동까지

        GameManager.Instance.ChangeGameState(GameState.Gameplay);
        SceneChanger.Instance.LoadScene("Idle");
    }
}
