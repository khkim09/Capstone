using UnityEngine;
using UnityEngine.UI;

public class EnemyInfoPanel : MonoBehaviour
{
    /// <summary>
    /// 표시 중인 적 함선의 최대 체력
    /// </summary>
    public float maxHealth;
    /// <summary>
    /// 표시 중인 적 함선의 최대 쉴드량
    /// </summary>
    public float maxShield;
    /// <summary>
    /// 표시 중인 적 함선
    /// </summary>
    public Ship enemyShip;
    /// <summary>
    /// 현재 아군 함선
    /// </summary>
    public Ship playerShip;
/// <summary>
/// 표시된 체력바
/// </summary>
    private Image hp;
    /// <summary>
    /// 표시된 쉴드량 바
    /// </summary>
    private Image shield;

    private GameObject display;
    void Start()
    {
        //적 함선과 아군 함선 연결 후 초기화
        enemyShip = GameObject.Find("EnemyShip").GetComponent<Ship>();
        playerShip = GameObject.Find("PlayerShip").GetComponent<Ship>();
        if (enemyShip != null)
        {
            maxHealth = enemyShip.HitpointSystem.GetHitPoint();
            maxShield = enemyShip.ShieldSystem.GetCurrentShield();
            enemyShip.InfoPanelChanged+=UpdateInfoPanel;
            hp = transform.Find("HealthBar").GetComponent<Image>();
            shield = transform.Find("ShieldBar").GetComponent<Image>();
        }
        else
        {
            Debug.LogError("EnemyInfoPanel에서 적 함선을 찾지 못했습니다.");
        }
        if(playerShip==null)
            Debug.LogError("EnemyInfoPanel에서 아군 함선을 찾지 못했습니다.");
        display=transform.parent.Find("CCTV").gameObject;
    }

    /// <summary>
    /// info panel에 필요한 정보를 갱신하여 다시 그린다.
    /// </summary>
    void UpdateInfoPanel()
    {
        hp.fillAmount = 1-enemyShip.GetCurrentHitPoints()/maxHealth;
        shield.fillAmount=1-enemyShip.ShieldSystem.GetCurrentShield()/maxShield;
    }


}
