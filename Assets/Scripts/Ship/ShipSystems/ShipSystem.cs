public abstract class ShipSystem
{
    protected Ship parentShip;

    public virtual void Initialize(Ship ship)
    {
        parentShip = ship;
    }

    public abstract void Update(float deltaTime);

    protected float GetShipStat(ShipStat stat)
    {
        return parentShip.GetStat(stat);
    }
}
