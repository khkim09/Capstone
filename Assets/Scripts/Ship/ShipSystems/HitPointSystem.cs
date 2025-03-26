using UnityEngine;

public class HitPointSystem : ShipSystem
{
    private float currentHitPoint;

    public override void Initialize(Ship ship)
    {
        base.Initialize(ship);

        currentHitPoint = GetShipStat(ShipStat.HitPointsMax);
    }

    public override void Update(float deltaTime)
    {
    }

    public float GetHitPoint()
    {
        return currentHitPoint;
    }

    public float GetHitPointPercentage()
    {
        return currentHitPoint / GetShipStat(ShipStat.HitPointsMax);
    }

    public void ChangeHitPoint(float amount)
    {
        currentHitPoint += amount;

        if (currentHitPoint <= 0) currentHitPoint = 0;
        if (currentHitPoint > GetShipStat(ShipStat.HitPointsMax)) currentHitPoint = GetShipStat(ShipStat.HitPointsMax);
    }
}
