using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 함선의 전력 공급을 관리하는 시스템.
/// 각 방에 전원을 요청하거나 차단하며, 전체 전력 사용량을 기반으로 제어합니다.
/// </summary>
public class PowerSystem : ShipSystem
{
    private List<Room> powerRequestedRooms;
    /// <summary>
    /// 매 프레임마다 호출되어 시스템 상태를 갱신합니다.
    /// </summary>
    /// <param name="deltaTime">경과 시간 (초).</param>
    public override void Update(float deltaTime)
    {

    }

    public override void Initialize(Ship ship)
    {
        base.Initialize(ship);
        parentShip.powerCheckNeed += NotEnoughPower;
    }

    public override void Refresh()
    {
        if(powerRequestedRooms==null)
        {
            powerRequestedRooms = new List<Room>();
        }
        if(parentShip.IsPowerCheckNeedConnected())
            parentShip.powerCheckNeed += NotEnoughPower;

        powerRequestedRooms.Clear();
        foreach (Room room in parentShip.GetAllRooms())
        {
            if (room.GetIsPowerRequested())
                powerRequestedRooms.Add(room);
        }
        powerRequestedRooms.Sort((room1,room2)=>room2.GetPowerConsumption().CompareTo(room1.GetPowerConsumption()));
    }

    public void NotEnoughPower()
    {
        if(powerRequestedRooms==null)
            Refresh();
        float powerUsing = parentShip.GetStat(ShipStat.PowerUsing);
        float powerCapacity = parentShip.GetStat(ShipStat.PowerCapacity);
        foreach (Room room in powerRequestedRooms)
        {
            if (powerUsing > powerCapacity)
            {
                if (room.GetIsPowered())
                {
                    room.BlackOut();
                    powerUsing -= room.GetPowerConsumption();
                }
            }
            else
            {
                parentShip.RecalculateAllStats();
                return;
            }
        }
    }

    /// <summary>
    /// 특정 방에 전원을 공급하거나 차단합니다.
    /// 전원을 끄는 요청은 항상 성공하며, 전원을 켜는 경우엔 남은 전력을 확인합니다.
    /// </summary>
    /// <param name="room">전원을 제어할 대상 방.</param>
    /// <param name="powerOn">true면 전원 공급 요청, false면 차단 요청.</param>
    /// <returns>
    /// 요청 처리 결과.
    /// - 전원 끄기 요청이면 항상 true
    /// - 전원 켜기 요청은 전력이 충분하면 true, 부족하면 false
    /// </returns>
    public bool RequestPowerForRoom(Room room, bool powerOn)
    {
        if (!powerOn)
        {
            // 전원을 끄는 경우는 항상 성공
            room.SetPowerStatus(false, false);
            parentShip.RecalculateAllStats();
            return true;
        }

        // 전원을 켜려는 경우, 충분한 전력이 있는지 확인
        float availablePower = GetShipStat(ShipStat.PowerCapacity);
        float usedPower = GetShipStat(ShipStat.PowerUsing);
        float remainingPower = availablePower - usedPower;
        float requiredPower = room.GetPowerConsumption();

        if (remainingPower >= requiredPower)
        {
            // 충분한 전력이 있으면 전원 켜기
            room.SetPowerStatus(true, true);
            parentShip.RecalculateAllStats();
            return true;
        }
        else
        {
            // 전력이 부족하면 요청만 설정하고 실제로는 켜지 않음
            room.SetPowerStatus(false, true);
            return false;
        }
    }
}
