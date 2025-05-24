using UnityEngine;

/// <summary>
/// 함선의 체력(Hit Point)을 관리하는 시스템.
/// 체력의 초기화, 변경 및 현재 상태를 조회하는 기능을 담당.
/// </summary>
public class HitPointSystem : ShipSystem
{
    /// <summary>
    /// 현재 체력 값입니다.
    /// </summary>
    private float currentHitPoint;

    /// <summary>
    /// 시스템을 초기화하고 최대 체력으로 설정합니다.
    /// </summary>
    /// <param name="ship">초기화 대상 함선 객체.</param>
    public override void Initialize(Ship ship)
    {
        base.Initialize(ship);

        Refresh();
    }

    public override void Refresh()
    {
        RecalculateHitPoint();
    }

    public void RecalculateHitPoint()
    {
        float sum = 0;
        foreach (Room room in parentShip.GetAllRooms())
        {
            sum += room.currentHitPoints;
        }

        currentHitPoint = sum;
    }

    /// <summary>
    /// 매 프레임마다 호출되어 시스템 상태를 갱신합니다.
    /// </summary>
    /// <param name="deltaTime">경과 시간 (초).</param>
    public override void Update(float deltaTime)
    {
    }

    /// <summary>
    /// 현재 체력 값을 반환합니다.
    /// </summary>
    /// <returns>현재 체력.</returns>
    public float GetHitPoint()
    {
        return currentHitPoint;
    }

    /// <summary>
    /// 현재 체력의 퍼센트 값을 반환합니다. (0 ~ 100)
    /// </summary>
    /// <returns>현재 체력 / 최대 체력 * 100 값.</returns>
    public float GetHitPointPercentage()
    {
        if (GetShipStat(ShipStat.HitPointsMax) == 0) return 0;
        return currentHitPoint / GetShipStat(ShipStat.HitPointsMax) * 100.0f;
    }

    /// <summary>
    /// 체력을 변경합니다. 양수이면 회복, 음수이면 피해를 의미합니다.
    /// 최대 체력을 초과하지 않으며, 0 아래로 내려가지 않습니다.
    /// </summary>
    /// <param name="amount">변경할 체력량. (양수/음수)</param>
    public void ChangeHitPoint(float amount)
    {
        currentHitPoint += amount;

        if (currentHitPoint <= 0) currentHitPoint = 0;
        if (currentHitPoint > GetShipStat(ShipStat.HitPointsMax)) currentHitPoint = GetShipStat(ShipStat.HitPointsMax);
    }
}
