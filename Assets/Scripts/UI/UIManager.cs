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

    // 전역 UI 이벤트
    public event Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        // 싱글톤 패턴 구현
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

    private void Start()
    {
    }


    /// <summary>
    /// 현재 씬이 메인 메뉴인지 확인
    /// </summary>
    public bool IsInMainMenu()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        bool isInMainMenu = currentSceneName == "MainMenu";
        return isInMainMenu;
    }

    /// <summary>
    /// 게임 상태 전환 - 이벤트 발생
    /// </summary>
    public void SwitchGameState(GameState gameState)
    {
        OnGameStateChanged?.Invoke(gameState);
    }
}
