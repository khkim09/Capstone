[System.Serializable]
public class EquipmentStats
{
    public float health;
    public float attack;
    public float defense;
    public float pilotSkill;
    public float engineSkill;
    public float powerSkill;
    public float shieldSkill;
    public float weaponSkill;
    public float ammunitionSkill;
    public float medbaySkill;
    public float repairSkill;

    public override string ToString()
    {

        return $@"
        HP : {health}\n
        ATK : {attack}\n
        DEF : {defense}\n
        Pilot Skill : {pilotSkill}\n
        Engine Skill : {engineSkill}\n
        Power Skill : {powerSkill}\n
        Shield Skill : {shieldSkill}\n
        Weapon Skill : {weaponSkill}\n
        Ammunition Skill : {ammunitionSkill}\n
        MedBay Skill : {medbaySkill}\n
        Repair Skill : {repairSkill}
        ";
    }

    public static string Compare(EquipmentStats oldStats, EquipmentStats newStats)
    {
        string hpDiff = GetDiff(oldStats.health, newStats.health);
        string atkDiff = GetDiff(oldStats.attack, newStats.attack);
        string defDiff = GetDiff(oldStats.defense, newStats.defense);
        string pilotSkillDiff = GetDiff(oldStats.pilotSkill, newStats.pilotSkill);
        string engineSkillDiff = GetDiff(oldStats.engineSkill, newStats.engineSkill);
        string powerSkillDiff = GetDiff(oldStats.powerSkill, newStats.powerSkill);
        string shieldSkillDiff = GetDiff(oldStats.shieldSkill, newStats.shieldSkill);
        string weaponSkillDiff = GetDiff(oldStats.weaponSkill, newStats.weaponSkill);
        string ammunitionSkillDiff = GetDiff(oldStats.ammunitionSkill, newStats.ammunitionSkill);
        string medbaySkillDiff = GetDiff(oldStats.medbaySkill, newStats.medbaySkill);
        string repairSkillDiff = GetDiff(oldStats.repairSkill, newStats.repairSkill);

        return $@"
        HP: {hpDiff}\n
        ATK: {atkDiff}\n
        DEF: {defDiff}\n
        Pilot Skill : {pilotSkillDiff}\n
        Engine Skill : {engineSkillDiff}\n
        Power Skill : {powerSkillDiff}\n
        Shield Skill : {shieldSkillDiff}\n
        Weapon Skill : {weaponSkillDiff}\n
        Ammunition Skill : {ammunitionSkillDiff}\n
        MedBay Skill : {medbaySkillDiff}\n
        Repair Skill : {repairSkillDiff}
        ";
    }

    private static string GetDiff(float oldVal, float newVal)
    {
        int diff = (int)(newVal - oldVal);

        return diff == 0 ? "±0" : diff > 0 ? $"+{diff}" : $"-{diff}";
    }
}
