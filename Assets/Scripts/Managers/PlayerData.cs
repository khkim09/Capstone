using System;

[Serializable]
public class PlayerData
{
    public int COMA => ResourceManager.Instance.COMA;

    public int questCleared = 0;

    public int mysteryFound = 0;

    public int pirateDefeated = 0;
}
