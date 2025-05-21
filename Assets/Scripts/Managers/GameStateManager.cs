using UnityEngine;

/// <summary>
/// 게임의 상태 머신을 관리하는 매니저.
/// 상태 전환 및 현재 상태의 업데이트/종료/진입 흐름을 제어합니다.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    /// <summary>
    /// 싱글턴 인스턴스입니다.
    /// </summary>
    public static GameStateManager Instance { get; private set; }

    /// <summary>
    /// 현재 활성화된 상태 머신입니다.
    /// </summary>
    private IGameStateMachine currentStateMachine;

    /// <summary>
    /// 인스턴스를 초기화하고 싱글턴을 설정합니다.
    /// </summary>
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

    /// <summary>
    /// 게임 시작 시 초기 상태를 설정합니다.
    /// </summary>
    private void Start()
    {
    }

    /// <summary>
    /// 매 프레임마다 현재 상태의 Update 로직을 호출합니다.
    /// </summary>
    private void Update()
    {
        // 현재 상태 업데이트
        currentStateMachine?.Update();
    }

    /// <summary>
    /// 현재 상태를 종료하고, 새 상태로 전환한 뒤 진입 로직을 실행합니다.
    /// </summary>
    /// <param name="newStateMachine">전환할 새 상태 머신.</param>
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
