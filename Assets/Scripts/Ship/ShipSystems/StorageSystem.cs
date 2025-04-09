using UnityEngine;

/// <summary>
/// </summary>
public class StorageSystem : ShipSystem
{
    /// <summary>
    /// </summary>
    /// <param name="ship">초기화 대상 함선 객체.</param>
    public override void Initialize(Ship ship)
    {
        base.Initialize(ship);
    }

    /// <summary>
    /// 매 프레임마다 호출되어 시스템 상태를 갱신합니다.
    /// </summary>
    /// <param name="deltaTime">경과 시간 (초).</param>
    public override void Update(float deltaTime)
    {
    }
}
