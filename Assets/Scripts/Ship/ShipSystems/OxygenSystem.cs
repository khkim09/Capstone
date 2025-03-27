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
    }
}
