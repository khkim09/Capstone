/// <summary>
/// 모든 함선 시스템의 추상 기반 클래스.
/// 각 시스템은 함선 객체를 기반으로 동작하며, 공통적인 초기화 및 스탯 조회 기능을 제공합니다.
/// </summary>
public abstract class ShipSystem
{
    /// <summary>
    /// 이 시스템이 소속된 함선 객체입니다.
    /// </summary>
    protected Ship parentShip;

    /// <summary>
    /// 시스템을 초기화합니다.
    /// 함선 참조를 저장하여 시스템 동작에 사용됩니다.
    /// </summary>
    /// <param name="ship">이 시스템이 속한 함선 객체.</param>
    public virtual void Initialize(Ship ship)
    {
        parentShip = ship;
    }

    /// <summary>
    /// 매 프레임마다 호출되는 시스템 업데이트 함수입니다.
    /// 구체적인 동작은 각 서브 클래스에서 구현됩니다.
    /// </summary>
    /// <param name="deltaTime">경과 시간 (초).</param>
    public abstract void Update(float deltaTime);

    /// <summary>
    /// 현재 함선의 특정 스탯 값을 가져옵니다.
    /// </summary>
    /// <param name="stat">조회할 스탯 종류.</param>
    /// <returns>해당 스탯의 현재 값.</returns>
    protected float GetShipStat(ShipStat stat)
    {
        return parentShip.GetStat(stat);
    }
}
