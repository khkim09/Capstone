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
    MeleeHit, // 근접 공격
    RangeHit, // 원거리 공격
    Damage, // 피해 (방어)
    Heal, // 치유
    Trade, // 거래
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
