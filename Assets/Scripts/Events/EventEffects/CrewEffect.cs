using System;

[Serializable]
public class CrewEffect
{
    public CrewEffectType effectType;
    public int targetCrewIndex = -1; // -1이면 랜덤 선택
    public float healthChange;
    public CrewStatus statusEffect;
}

public enum CrewEffectType
{
    Damage,
    Heal,
    StatusChange,
    Kill,
    AddCrew,
    RemoveCrew
}

public enum CrewStatus
{
    Normal,
    Injured,
    Sick,
    Insane
}
