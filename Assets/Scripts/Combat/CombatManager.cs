using UnityEngine;

/// <summary>
/// 전투 전체 흐름을 관리하는 매니저 클래스입니다.
/// 전투 상태를 전환하고, 관련 컨트롤러를 초기화하거나 업데이트합니다.
/// </summary>
public class CombatManager : MonoBehaviour
{
    public EnemyCamInteraction cam;
    /// <summary>
    /// CombatManager의 싱글톤 인스턴스입니다.
    /// </summary>
    public static CombatManager Instance { get; private set; }

    /// <summary>
    /// 외부 함선 전투 컨트롤러입니다.
    /// </summary>
    public OuterShipCombatController outerShipCombatController;

    /// <summary>
    /// 현재 상태입니다.
    /// </summary>
    private CombatPhase currentPhase;

    /// <summary>
    /// 전투 UI 컨트롤러입니다.
    /// </summary>
    private CombatUIController uiController;


    // [SerializeField] private ProjectilePool projectilePool; // 탄환 풀링

    /// <summary>
    /// CombatManager가 초기화될 때 호출되며 싱글톤을 설정합니다.
    /// 씬 전환 시에도 파괴되지 않도록 처리됩니다.
    /// </summary>
    private void Awake()
    {
        // 싱글톤 인스턴스 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 중복 인스턴스 방지
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject); // 씬 이동 시에도 유지
    }

    /// <summary>
    /// 전투 상태머신이 시작될 때 호출됩니다.
    /// 초기 상태로 설정됩니다.
    /// </summary>
    public void Start()
    {
        currentPhase = CombatPhase.Initialize;

        Ship enemyShip = GameObject.Find("EnemyShip").GetComponent<Ship>();
        enemyShip.Initialize();

        GameManager.Instance.CreateDefaultEnemyShip(enemyShip);

        RTSSelectionManager.Instance.enemyShip = enemyShip;
        RTSSelectionManager.Instance.playerShip=GameManager.Instance.playerShip;
        GameManager.Instance.SetCurrentEnemyShip(enemyShip);

        //enemyShip.MoveShipToFacingTargetShip(GameManager.Instance.playerShip);
        cam.EnemyCamAim();

        RTSSelectionManager.Instance.RefreshMovementData();

        enemyShip.WeaponSystem.SetAutoFireEnabled(true);
        GameManager.Instance.playerShip.HitpointSystem.RecalculateHitPoint();

        enemyShip.shipExplosion += CombatEnding;
        GameManager.Instance.playerShip.shipExplosion+= CombatEnding;
    }

    public void CombatEnding(Ship ship)
    {
        if (ship == GameManager.Instance.playerShip)
        {
            defeatUI.SetActive(true);
        }
        else
        {
            winUI.SetActive(true);
        }
    }

    public GameObject defeatUI;
    public GameObject winUI;

    /// <summary>
    /// 매 프레임마다 호출되며 현재 전투 상태에 따라 로직을 처리합니다.
    /// </summary>
    // public void Update()
    // {
    //     switch (currentPhase)
    //     {
    //         case CombatPhase.Initialize:
    //             break;
    //         case CombatPhase.Combat:
    //             break;
    //         case CombatPhase.DestroyAnimation:
    //             break;
    //         case CombatPhase.Reward:
    //             break;
    //         case CombatPhase.End:
    //             break;
    //         default:
    //             break;
    //     }
    // }

    /// <summary>
    /// 전투 상태가 종료될 때 호출됩니다. (현재 구현 없음)
    /// </summary>
    public void Exit()
    {
    }
}
