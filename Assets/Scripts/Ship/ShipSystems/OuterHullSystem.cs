using UnityEngine;

/// <summary>
/// 외갑판 시스템.
/// 공격에 대한 피해를 감소시키는 기능을 담당합니다.
/// </summary>
public class OuterHullSystem : ShipSystem
{
    /// <summary>
    /// 외부 선체 시스템의 현재 레벨을 나타냅니다.
    /// 이 레벨은 들어오는 공격에 대해 적용되는 피해 감소량에 영향을 줄 수 있습니다.
    /// </summary>
    private int currentOuterHullLevel;

    /// <summary>
    /// 매 프레임마다 호출되어 시스템 상태를 갱신합니다.
    /// </summary>
    /// <param name="deltaTime">경과 시간 (초).</param>
    public override void Update(float deltaTime)
    {
    }

    /// <summary>
    /// 외부 선체의 피해 감소율을 적용하여 실질적인 피해량을 계산합니다.
    /// </summary>
    /// <param name="damage">입력된 원래 피해량.</param>
    /// <returns>피해 감소가 적용된 최종 피해량.</returns>
    public float ReduceDamage(float damage)
    {
        float damageAfterHull = damage * (100 - GetShipStat(ShipStat.DamageReduction)) / 100;
        return damageAfterHull;
    }

    /// <summary>
    /// 외부 선체 시스템의 현재 레벨을 반환합니다.
    /// </summary>
    /// <returns>외부 선체 시스템의 현재 레벨.</returns>
    public int GetOuterHullLevel()
    {
        return currentOuterHullLevel;
    }

    /// <summary>
    /// 외부 선체 시스템의 현재 레벨을 설정합니다.
    /// </summary>
    /// <param name="level">새로운 레벨 값.</param>
    public void SetOuterHullLevel(int level)
    {
        currentOuterHullLevel = level;
    }
}
