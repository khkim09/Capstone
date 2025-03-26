using UnityEngine;

public class PowerSystem : ShipSystem
{
    public override void Update(float deltaTime)
    {
    }

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
