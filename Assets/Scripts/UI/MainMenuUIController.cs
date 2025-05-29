using UnityEngine;

public class MainMenuUIController : MonoBehaviour
{
    public void OnStartButtonClicked()
    {
        GameManager.Instance.StartNewGame();
    }

    public void OnSettingsButtonClicked()
    {
    }

    public void OnContinueButtonClicked()
    {
        GameManager.Instance.ContinueGame();
    }

    public void OnExitButtonClicked()
    {
        Application.Quit();
    }
}
