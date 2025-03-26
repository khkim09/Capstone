using UnityEngine;

public class CombatManager : MonoBehaviour, IGameStateMachine
{
    public static CombatManager Instance { get; private set; }
    public OuterShipCombatController outerShipCombatController;

    // 현재 단계
    private CombatPhase currentPhase;

    private CombatUIController uiController;

    // [SerializeField] private ProjectilePool projectilePool; // 탄환 풀링

    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 중복 인스턴스 방지
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬 이동 시에도 유지
    }


    public void Enter()
    {
        currentPhase = CombatPhase.Initialize;
    }

    public void Update()
    {
        switch (currentPhase)
        {
            case CombatPhase.Initialize:
                break;
            case CombatPhase.Combat:
                break;
            case CombatPhase.DestroyAnimation:
                break;
            case CombatPhase.Reward:
                break;
            case CombatPhase.End:
                break;
            default:
                break;
        }
    }

    public void Exit()
    {
    }
}
