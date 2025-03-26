using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    private IGameStateMachine currentStateMachine;

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
        ChangeState(new WarpStateMachine());
    }

    private void Update()
    {
        // 현재 상태 업데이트
        currentStateMachine?.Update();
    }

    public void ChangeState(IGameStateMachine newStateMachine)
    {
        // 현재 상태 종료
        currentStateMachine?.Exit();

        // 새 상태로 전환
        currentStateMachine = newStateMachine;

        // 새 상태 진입
        currentStateMachine?.Enter();
    }
}
