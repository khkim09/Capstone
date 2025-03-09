using System;

[Serializable]
public class ResourceEffect
{
    public int amount; // 양수면 획득, 음수면 손실
    public ResourceType resourceType;
}

public enum ResourceType
{
    Fuel,
    Food,
    Water,
    Scrap,
    Missiles,
    Gold
}
