// 메인 UI 매니저 - 싱글톤으로 구현

using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // 상태별 UI 컨트롤러 참조
    [SerializeField] private GameObject warpUIControllerPrefab;

    public WarpUIController activeWarpUIController;

    // 여기에다가 각 UI 만들 때마다 컨트롤러 등록

    // 상태와 무관한 공통 UI 요소들
    [SerializeField] private GameObject commonHUD;
    [SerializeField] private GameObject pauseMenu;


    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 게임 상태 변경에 따른 UI 전환
    public void SwitchToGameState(GameState stateType)
    {
        // 모든 UI 컨트롤러 비활성화
        DisalbeAllUI();

        // 해당 상태 UI 활성화
        switch (stateType)
        {
            case GameState.Warp:
                // true로 설정
                warpUIControllerPrefab.SetActive(true);
                break;
        }
    }

    public void DisalbeAllUI()
    {
        warpUIControllerPrefab.gameObject.SetActive(false);
        // 여기에다가 각 UI컨트롤러 만들 때마다 비활성화 코드 추가
    }

    public WarpUIController GetOrCreateWarpUIController()
    {
        if (activeWarpUIController == null)
        {
            GameObject controllerObj = Instantiate(warpUIControllerPrefab);
            activeWarpUIController = controllerObj.GetComponent<WarpUIController>();
        }

        return activeWarpUIController;
    }

    // 공통 UI 기능
    public void ShowPauseMenu()
    {
        pauseMenu.SetActive(true);
    }

    public void HidePauseMenu()
    {
        pauseMenu.SetActive(false);
    }
}
