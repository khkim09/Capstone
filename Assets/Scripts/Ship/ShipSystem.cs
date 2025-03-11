using System;

[Serializable]
public class ShipSystem
{
    public string name;
    public float health;
    public float maxHealth = 100f;
    public int level = 1;
    public bool isActive = true;
    public float efficiency = 1.0f;
}
