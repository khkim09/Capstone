using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    private IGameState currentState;

    private void Awake()
    {
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
        // 초기 상태 설정
        ChangeState(new WarpState());
    }

    private void Update()
    {
        // 현재 상태 업데이트
        currentState?.Update();
    }

    public void ChangeState(IGameState newState)
    {
        // 현재 상태 종료
        currentState?.Exit();

        // 새 상태로 전환
        currentState = newState;

        // 새 상태 진입
        currentState?.Enter();
    }
}
