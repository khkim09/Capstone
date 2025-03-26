using System.Collections;
using UnityEngine;

// 게임 매니저 - 게임 전체 상태 관리
public class GameManager : MonoBehaviour
{
    private Ship playerShip;
    private Ship currentEnemyShip;


    public delegate void DayChangedHandler(int newDay);

    // 이벤트 및 상태 변화와 관련된 델리게이트
    public delegate void GameStateChangedHandler(GameState newState);

    [Header("Game State")]
    [SerializeField]
    private GameState currentState = GameState.MainMenu;

    [SerializeField] private int currentDay = 1;
    [SerializeField] private int maxDays = 30;

    [SerializeField] private int currentYear = 0; // 게임 시작 년도 (수정 가능)
    public int CurrentYear => currentYear;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 필요한 초기화
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public event GameStateChangedHandler OnGameStateChanged;
    public event DayChangedHandler OnDayChanged;

    // 게임 초기화
    private void InitializeGame()
    {
        // 다른 매니저들이 모두 초기화되었는지 확인
        StartCoroutine(WaitForManagers());

        // TODO: 임시로 씬에서 설치된 배를 찾아 플레이어 배로 등록
        playerShip = GameObject.Find("PlayerShip").GetComponent<Ship>();
    }

    private IEnumerator WaitForManagers()
    {
        // 다른 중요 매니저들이 초기화될 때까지 기다림
        yield return new WaitUntil(() =>
            ResourceManager.Instance != null &&
            EventManager.Instance != null &&
            CrewManager.Instance != null
        );
    }

    public Ship GetPlayerShip()
    {
        return playerShip;
    }

    public Ship GetCurrentEnemyShip()
    {
        return currentEnemyShip;
    }

    public void SetCurrentEnemyShip(Ship enemyShip)
    {
        currentEnemyShip = enemyShip;
    }


    // 게임 상태 변경
    public void ChangeGameState(GameState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            OnGameStateChanged?.Invoke(newState);

            // 상태에 따른 특정 로직
            switch (newState)
            {
                case GameState.MainMenu:
                    Time.timeScale = 0f;
                    break;
                case GameState.Gameplay:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    HandleGameOver();
                    break;
            }
        }
    }

    // 다음 날로 진행
    public void AdvanceDay()
    {
        currentDay++;
        OnDayChanged?.Invoke(currentDay);

        if (currentDay > maxDays)
            ChangeGameState(GameState.GameOver);
        else
            // 다음 날 시작 이벤트 발생
            EventManager.Instance.TriggerDailyEvent();
    }

    // 이벤트 처리 완료 후 호출
    public void OnEventCompleted()
    {
        // 이벤트 후 게임 상태 체크
        if (DefaultCrewManagerScript.Instance.GetAliveCrewCount() <= 0) ChangeGameState(GameState.GameOver);
    }

    private void HandleGameOver()
    {
        // 게임 오버 처리 로직
        Debug.Log("Game Over! You survived " + currentDay + " days.");
        // UI 업데이트, 점수 계산 등
    }

    // 워프 실행 시 1년 흐름
    public void AddYearByWarp()
    {
        currentYear++;

        // 워프로 인한 이벤트 처리
        // EventManager.Instance.TriggerYearlyWarpEvent();

        Debug.Log($"[워프 완료] 현재 연도 : {currentYear}");

        EventMoraleEffectManager.Instance.CheckEventExpirations(currentYear); // 불가사의 지속 기간 체크
    }
}

public enum GameState
{
    MainMenu,
    Gameplay,
    Warp,
    Event,
    Paused,
    GameOver
}
