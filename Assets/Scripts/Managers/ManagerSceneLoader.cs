using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagerSceneLoader : MonoBehaviour
{
    private static bool isManagerSceneLoaded = false;

    private void Awake()
    {
        if (!isManagerSceneLoaded)
        {
            SceneManager.LoadScene("ManagerScene", LoadSceneMode.Additive);
            isManagerSceneLoaded = true;
        }
    }
}
