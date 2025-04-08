using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    /// <summary>
    /// "구입" 버튼에서 호출할 메서드입니다.
    /// PlanetTradeBuyScene으로 씬을 전환합니다.
    /// </summary>
    public void OnClickGoToPlanetTradeBuyScene()
    {
        SceneManager.LoadScene("PlanetTradeBuyScene");
    }
    /// <summary>
    /// "판매" 버튼에서 호출할 메서드입니다.
    /// PlanetTradeSellScene으로 씬을 전환합니다.
    /// </summary>
    public void OnClickGoToPlanetTradeSellScene()
    {
        SceneManager.LoadScene("PlanetTradeSellScene");
    }

}
