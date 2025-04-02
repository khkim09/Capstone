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

    /// <summary>
    /// 인스턴스를 초기화하고 싱글톤을 설정합니다.
    /// </summary>
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

    /// <summary>
    /// 게임 상태 전환 시, 해당 상태에 맞는 UI를 활성화합니다.
    /// 이전 UI는 모두 비활성화됩니다.
    /// </summary>
    /// <param name="stateType">전환할 게임 상태.</param>
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


    /// <summary>
    /// 현재 활성화된 모든 UI 컨트롤러를 비활성화합니다.
    /// </summary>
    public void DisalbeAllUI()
    {
        warpUIControllerPrefab.gameObject.SetActive(false);
        // 여기에다가 각 UI컨트롤러 만들 때마다 비활성화 코드 추가
    }

    /// <summary>
    /// 워프 UI 컨트롤러를 가져오거나, 없으면 새로 생성합니다.
    /// </summary>
    /// <returns>활성화된 WarpUIController 인스턴스.</returns>
    public WarpUIController GetOrCreateWarpUIController()
    {
        if (activeWarpUIController == null)
        {
            GameObject controllerObj = Instantiate(warpUIControllerPrefab);
            activeWarpUIController = controllerObj.GetComponent<WarpUIController>();
        }

        return activeWarpUIController;
    }

    /// <summary>
    /// 워프 UI 컨트롤러를 가져오거나, 없으면 새로 생성합니다.
    /// </summary>
    /// <returns>활성화된 WarpUIController 인스턴스.</returns>
    public void ShowPauseMenu()
    {
        pauseMenu.SetActive(true);
    }

    /// <summary>
    /// 일시정지 메뉴를 숨깁니다.
    /// </summary>
    public void HidePauseMenu()
    {
        pauseMenu.SetActive(false);
    }
}
