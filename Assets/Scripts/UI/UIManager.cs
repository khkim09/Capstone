using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 전역 UI 상태 관리 및 메시지 중계 역할을 하는 싱글톤 매니저
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Global Settings")] [SerializeField]
    private string mainMenuSceneName = "MainMenu";

    // 현재 씬이 메인 메뉴인지 여부
    private bool isInMainMenu = false;

    // 씬 상태 변경 이벤트 - 메인 메뉴 진입/이탈 시 발생
    public event Action<bool> OnMainMenuStateChanged;

    // 전역 UI 이벤트
    public event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 씬 전환 이벤트 구독
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 현재 씬 상태 초기화
        UpdateMainMenuState();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 현재 씬 상태 업데이트
        UpdateMainMenuState();
    }

    /// <summary>
    /// 메인 메뉴 상태 업데이트 및 필요시 이벤트 발생
    /// </summary>
    private void UpdateMainMenuState()
    {
        // 현재 씬 이름 가져오기
        string currentSceneName = SceneManager.GetActiveScene().name;

        // 메인 메뉴 상태 체크
        bool newIsInMainMenu = currentSceneName == mainMenuSceneName;

        // 상태가 변경된 경우 이벤트 발생
        if (isInMainMenu != newIsInMainMenu)
        {
            isInMainMenu = newIsInMainMenu;
            OnMainMenuStateChanged?.Invoke(isInMainMenu);
        }
    }

    /// <summary>
    /// 현재 씬이 메인 메뉴인지 확인
    /// </summary>
    public bool IsInMainMenu()
    {
        return isInMainMenu;
    }

    /// <summary>
    /// 게임 상태 전환 - 이벤트 발생
    /// </summary>
    public void SwitchGameState(GameState gameState)
    {
        OnGameStateChanged?.Invoke(gameState);
    }


    private void OnDestroy()
    {
        // 이벤트 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
