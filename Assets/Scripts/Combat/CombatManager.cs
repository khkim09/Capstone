using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] private GraphicRaycaster combatUI;

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

        RTSSelectionManager.Instance.SetGRC(combatUI);
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
        RTSSelectionManager.Instance.playerShip = GameManager.Instance.playerShip;
        GameManager.Instance.SetCurrentEnemyShip(enemyShip);

        //enemyShip.MoveShipToFacingTargetShip(GameManager.Instance.playerShip);
        cam.EnemyCamAim();

        RTSSelectionManager.Instance.RefreshMovementData();

        enemyShip.WeaponSystem.SetAutoFireEnabled(false);
        GameManager.Instance.playerShip.HitpointSystem.RecalculateHitPoint();

        enemyShip.shipExplosion += CombatEnding;
        GameManager.Instance.playerShip.shipExplosion += CombatEnding;
    }

    public GameObject CCTV;

    public void CombatEnding(Ship ship)
    {
        foreach (ShipWeapon wp in GameManager.Instance.playerShip.GetAllWeapons()) wp.DisconnectAction();
        if (CCTV)
            CCTV.SetActive(false);
        Time.timeScale = 0;
        if (ship == GameManager.Instance.playerShip)
        {
            if (defeatUI)
                defeatUI.SetActive(true);
        }
        else
        {
            if (winUI)
                winUI.SetActive(true);
        }
    }

    public GameObject defeatUI;
    public GameObject winUI;


    public void CombatDefeatedAndGoHome(bool isDefeated)
    {
        foreach (CrewMember crew in GameManager.Instance.playerShip.allCrews)
            crew.Freeze();
        foreach (CrewMember crew in GameManager.Instance.playerShip.allEnemies)
            crew.Freeze();
        foreach (CrewMember crew in GameManager.Instance.currentEnemyShip.allCrews)
            crew.Freeze();
        foreach (CrewMember crew in GameManager.Instance.currentEnemyShip.allCrews)
            crew.Freeze();

        StartCoroutine(BackToTheHome(isDefeated));
    }

    /// <summary>
    /// 모선으로 복귀하라 (승리 : isDefeated = false, 패배 : isDefeated = true)
    /// </summary>
    /// <param name="isDefeated"></param>
    /// <returns></returns>
    private IEnumerator BackToTheHome(bool isDefeated) /// 이거 void로 바꿔서 해보기
        // 굳이 코루틴 안해도 될듯
    {
        // 승리
        if (!isDefeated)
        {
            // 내 함선에 있는 적 선원 복귀 및 자동 삭제
            List<CrewMember> needToDestroy = GameManager.Instance.playerShip.allEnemies;
            for (int i = needToDestroy.Count - 1; i >= 0; i--)
            {
                needToDestroy[i].Freeze();
                needToDestroy[i].InstantKill();
            }

            GameManager.Instance.playerShip.allEnemies.Clear();
            needToDestroy.Clear();

            // 적 함선에 있는 내 선원 복귀
            List<CrewMember> needToBackMine = GameManager.Instance.currentEnemyShip.allEnemies;
            for (int i = needToBackMine.Count - 1; i >= 0; i--)
            {
                needToBackMine[i].Freeze();
                StartCoroutine(needToBackMine[i].TeleportAfterDelay(needToBackMine[i], 0.3f));
            }

            GameManager.Instance.currentEnemyShip.allEnemies.Clear();
            needToBackMine.Clear();
        }
        else // 패배
        {
            // 내 함선 내 모든 선원 삭제
            List<CrewMember> playerShipAllCrews = GameManager.Instance.playerShip.allCrews;
            for (int i = playerShipAllCrews.Count - 1; i >= 0; i--) playerShipAllCrews[i].InstantKill();
            GameManager.Instance.playerShip.allCrews.Clear();
            playerShipAllCrews.Clear();

            List<CrewMember> playerShipAllEnemies = GameManager.Instance.playerShip.allEnemies;
            for (int i = playerShipAllEnemies.Count - 1; i >= 0; i--) playerShipAllEnemies[i].InstantKill();
            GameManager.Instance.playerShip.allEnemies.Clear();
            playerShipAllEnemies.Clear();
        }

        yield return null;
    }


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
