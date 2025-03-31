using UnityEngine;

public class OxygenSystem : ShipSystem
{
    public float currentOxygenRate;

    public override void Initialize(Ship ship)
    {
        base.Initialize(ship);
        currentOxygenRate = 100.0f;
    }

    public override void Update(float deltaTime)
    {
        currentOxygenRate += CalculateOxygenChange() * deltaTime;

        currentOxygenRate = Mathf.Clamp(currentOxygenRate, 0, 100);
    }

    public float CalculateOxygenChange()
    {
        float oxygenGeneratePerSecond = GetShipStat(ShipStat.OxygenGeneratePerSecond);
        float oxygenUsingPerSecond = GetShipStat(ShipStat.OxygenUsingPerSecond);

        return oxygenGeneratePerSecond - oxygenUsingPerSecond;
    }
}
