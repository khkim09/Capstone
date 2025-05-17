using UnityEngine;

public class MainMenuUIController : MonoBehaviour
{
    public void OnStartButtonClicked()
    {
        GameManager.Instance.StartNewGame();
    }

    public void OnSettingsButtonClicked()
    {
        GameManager.Instance.SaveGameData();
    }

    public void OnContinueButtonClicked()
    {
        GameManager.Instance.LoadGameData();
    }

    public void OnExitButtonClicked()
    {
    }
}
