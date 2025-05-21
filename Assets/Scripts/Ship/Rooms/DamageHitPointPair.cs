using System;

[Serializable]
public class DamageHitPointPair
{
    public RoomDamageLevel damageLevel;
    public float hitPointRate;

    public DamageHitPointPair(RoomDamageLevel damageLevel, float hitPointRate)
    {
        this.damageLevel = damageLevel;
        this.hitPointRate = hitPointRate;
    }
}
