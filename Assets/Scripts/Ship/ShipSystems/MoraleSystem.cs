using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선 내 전체 승무원의 사기를 계산하는 시스템.
/// 방의 종류 및 승무원 수에 따라 전체 사기가 변화합니다.
/// </summary>
public class MoraleSystem : ShipSystem
{
    /// <summary>
    /// 시스템을 초기화합니다.
    /// </summary>
    /// <param name="ship">초기화할 대상 함선 객체.</param>
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

    /// <summary>
    /// 현재 함선의 전체 사기 수치를 계산합니다.
    /// 생명유지실과 승무원 침실이 기여하며, 승무원 수가 수용 가능 인원을 초과할 경우 패널티가 적용됩니다.
    /// </summary>
    /// <returns>계산된 전체 사기 수치.</returns>
    public float CalculateGlobalMorale()
    {
        float resultMorale = 0, crewCapacity = 0;

        foreach (Room room in parentShip.GetAllRooms())
        {
            if (room is LifeSupportRoom lifeSupportRoom)
                resultMorale += lifeSupportRoom.GetStatContributions()[ShipStat.CrewMoraleBonus];

            if (room is CrewQuartersRoom crewQuartersRoom)
                crewCapacity += crewQuartersRoom.GetStatContributions()[ShipStat.CrewCapacity];
        }

        if (crewCapacity < parentShip.GetCrewCount())
            resultMorale -= parentShip.GetCrewCount() - crewCapacity;


        return resultMorale;
    }
}
