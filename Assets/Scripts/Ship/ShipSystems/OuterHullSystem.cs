using UnityEngine;

public class OuterHullSystem : ShipSystem
{
    public override void Update(float deltaTime)
    {
    }

    public float ReduceDamage(float damage)
    {
        float damageAfterHull = damage * (100 - GetShipStat(ShipStat.DamageReduction)) / 100;
        return damageAfterHull;
    }
}
