using UnityEngine;

[CreateAssetMenu(fileName = "CrewRaceSettings", menuName = "Crew/CrewRaceSettings")]
public class CrewRaceSettings : ScriptableObject
{
    public CrewRace race;
    public float maxHealth;
    public float attack;
    public float defense;
    public float learningSpeed;
    public bool needsOxygen;

    public float[] maxSkillValueArray = new float[8];

    public float initialPilotSkill;
    public float initialEngineSkill;
    public float initialPowerSkill;
    public float initialShieldSkill;
    public float initialWeaponSkill;
    public float initialAmmunitionSkill;
    public float initialMedBaySkill;
    public float initialRepairSkill;
}
