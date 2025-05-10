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
        {health}\n
        {attack}\n
        {defense}\n
        {pilotSkill}\n
        {engineSkill}\n
        {powerSkill}\n
        {shieldSkill}\n
        {weaponSkill}\n
        {ammunitionSkill}\n
        {medbaySkill}\n
        {repairSkill}
        ";
    }
}
