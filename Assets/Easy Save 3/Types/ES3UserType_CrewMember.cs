using System;
using UnityEngine;

namespace ES3Types
{
    [UnityEngine.Scripting.Preserve]
    [ES3PropertiesAttribute("crewName", "isPlayerControlled", "race", "maxHealth", "attack", "defense", "learningSpeed",
        "needsOxygen", "maxSkillValueArray", "maxPilotSkillValue", "maxEngineSkillValue", "maxPowerSkillValue",
        "maxShieldSkillValue", "maxWeaponSkillValue", "maxAmmunitionSkillValue", "maxMedBaySkillValue",
        "maxRepairSkillValue", "skills", "equipAdditionalSkills", "equippedWeapon", "equippedShield",
        "equippedAssistant", "currentRoom", "position", "targetPosition", "moveSpeed", "health", "status", "isAlive",
        "isMoving", "currentShip")]
    public class ES3UserType_CrewMember : ES3ComponentType
    {
        public static ES3Type Instance = null;

        public ES3UserType_CrewMember() : base(typeof(CrewMember))
        {
            Instance = this;
            priority = 1;
        }


        protected override void WriteComponent(object obj, ES3Writer writer)
        {
            CrewMember instance = (CrewMember)obj;

            writer.WriteProperty("crewName", instance.crewName, ES3Type_string.Instance);
            writer.WriteProperty("isPlayerControlled", instance.isPlayerControlled, ES3Type_bool.Instance);
            writer.WriteProperty("race", instance.race, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(CrewRace)));
            writer.WriteProperty("maxHealth", instance.maxHealth, ES3Type_float.Instance);
            writer.WriteProperty("attack", instance.attack, ES3Type_float.Instance);
            writer.WriteProperty("defense", instance.defense, ES3Type_float.Instance);
            writer.WriteProperty("learningSpeed", instance.learningSpeed, ES3Type_float.Instance);
            writer.WriteProperty("needsOxygen", instance.needsOxygen, ES3Type_bool.Instance);
            writer.WriteProperty("maxSkillValueArray", instance.maxSkillValueArray,
                ES3Internal.ES3TypeMgr.GetOrCreateES3Type(
                    typeof(System.Collections.Generic.Dictionary<SkillType, float>)));
            writer.WriteProperty("maxPilotSkillValue", instance.maxPilotSkillValue, ES3Type_float.Instance);
            writer.WriteProperty("maxEngineSkillValue", instance.maxEngineSkillValue, ES3Type_float.Instance);
            writer.WriteProperty("maxPowerSkillValue", instance.maxPowerSkillValue, ES3Type_float.Instance);
            writer.WriteProperty("maxShieldSkillValue", instance.maxShieldSkillValue, ES3Type_float.Instance);
            writer.WriteProperty("maxWeaponSkillValue", instance.maxWeaponSkillValue, ES3Type_float.Instance);
            writer.WriteProperty("maxAmmunitionSkillValue", instance.maxAmmunitionSkillValue, ES3Type_float.Instance);
            writer.WriteProperty("maxMedBaySkillValue", instance.maxMedBaySkillValue, ES3Type_float.Instance);
            writer.WriteProperty("maxRepairSkillValue", instance.maxRepairSkillValue, ES3Type_float.Instance);
            writer.WriteProperty("skills", instance.skills,
                ES3Internal.ES3TypeMgr.GetOrCreateES3Type(
                    typeof(System.Collections.Generic.Dictionary<SkillType, float>)));
            writer.WriteProperty("equipAdditionalSkills", instance.equipAdditionalSkills,
                ES3Internal.ES3TypeMgr.GetOrCreateES3Type(
                    typeof(System.Collections.Generic.Dictionary<SkillType, float>)));
            writer.WritePropertyByRef("equippedWeapon", instance.equippedWeapon);
            writer.WritePropertyByRef("equippedShield", instance.equippedShield);
            writer.WritePropertyByRef("equippedAssistant", instance.equippedAssistant);
            writer.WritePropertyByRef("currentRoom", instance.currentRoom);
            writer.WriteProperty("position", instance.position, ES3Type_Vector2.Instance);
            writer.WriteProperty("targetPosition", instance.targetPosition, ES3Type_Vector2.Instance);
            writer.WriteProperty("moveSpeed", instance.moveSpeed, ES3Type_float.Instance);
            writer.WriteProperty("health", instance.health, ES3Type_float.Instance);
            writer.WriteProperty("isAlive", instance.isAlive, ES3Type_bool.Instance);
            writer.WriteProperty("isMoving", instance.isMoving, ES3Type_bool.Instance);
            writer.WritePropertyByRef("currentShip", instance.currentShip);
        }

        protected override void ReadComponent<T>(ES3Reader reader, object obj)
        {
            CrewMember instance = (CrewMember)obj;
            foreach (string propertyName in reader.Properties)
                switch (propertyName)
                {
                    case "crewName":
                        instance.crewName = reader.Read<string>(ES3Type_string.Instance);
                        break;
                    case "isPlayerControlled":
                        instance.isPlayerControlled = reader.Read<bool>(ES3Type_bool.Instance);
                        break;
                    case "race":
                        instance.race = reader.Read<CrewRace>();
                        break;
                    case "maxHealth":
                        instance.maxHealth = reader.Read<float>(ES3Type_float.Instance);
                        break;
                    case "attack":
                        instance.attack = reader.Read<float>(ES3Type_float.Instance);
                        break;
                    case "defense":
                        instance.defense = reader.Read<float>(ES3Type_float.Instance);
                        break;
                    case "learningSpeed":
                        instance.learningSpeed = reader.Read<float>(ES3Type_float.Instance);
                        break;
                    case "needsOxygen":
                        instance.needsOxygen = reader.Read<bool>(ES3Type_bool.Instance);
                        break;
                    case "maxSkillValueArray":
                        instance.maxSkillValueArray =
                            reader.Read<System.Collections.Generic.Dictionary<SkillType, float>>();
                        break;
                    case "maxPilotSkillValue":
                        instance.maxPilotSkillValue = reader.Read<float>(ES3Type_float.Instance);
                        break;
                    case "maxEngineSkillValue":
                        instance.maxEngineSkillValue = reader.Read<float>(ES3Type_float.Instance);
                        break;
                    case "maxPowerSkillValue":
                        instance.maxPowerSkillValue = reader.Read<float>(ES3Type_float.Instance);
                        break;
                    case "maxShieldSkillValue":
                        instance.maxShieldSkillValue = reader.Read<float>(ES3Type_float.Instance);
                        break;
                    case "maxWeaponSkillValue":
                        instance.maxWeaponSkillValue = reader.Read<float>(ES3Type_float.Instance);
                        break;
                    case "maxAmmunitionSkillValue":
                        instance.maxAmmunitionSkillValue = reader.Read<float>(ES3Type_float.Instance);
                        break;
                    case "maxMedBaySkillValue":
                        instance.maxMedBaySkillValue = reader.Read<float>(ES3Type_float.Instance);
                        break;
                    case "maxRepairSkillValue":
                        instance.maxRepairSkillValue = reader.Read<float>(ES3Type_float.Instance);
                        break;
                    case "skills":
                        instance.skills = reader.Read<System.Collections.Generic.Dictionary<SkillType, float>>();
                        break;
                    case "equipAdditionalSkills":
                        instance.equipAdditionalSkills =
                            reader.Read<System.Collections.Generic.Dictionary<SkillType, float>>();
                        break;
                    case "equippedWeapon":
                        instance.equippedWeapon = reader.Read<EquipmentItem>();
                        break;
                    case "equippedShield":
                        instance.equippedShield = reader.Read<EquipmentItem>();
                        break;
                    case "equippedAssistant":
                        instance.equippedAssistant = reader.Read<EquipmentItem>();
                        break;
                    case "currentRoom":
                        instance.currentRoom = reader.Read<Room>();
                        break;
                    case "position":
                        instance.position = reader.Read<Vector2Int>(ES3Type_Vector2.Instance);
                        break;
                    case "targetPosition":
                        instance.targetPosition = reader.Read<Vector2Int>(ES3Type_Vector2.Instance);
                        break;
                    case "moveSpeed":
                        instance.moveSpeed = reader.Read<float>(ES3Type_float.Instance);
                        break;
                    case "health":
                        instance.health = reader.Read<float>(ES3Type_float.Instance);
                        break;
                    case "isAlive":
                        instance.isAlive = reader.Read<bool>(ES3Type_bool.Instance);
                        break;
                    case "isMoving":
                        instance.isMoving = reader.Read<bool>(ES3Type_bool.Instance);
                        break;
                    case "currentShip":
                        instance.currentShip = reader.Read<Ship>(ES3UserType_Ship.Instance);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
        }
    }


    public class ES3UserType_CrewMemberArray : ES3ArrayType
    {
        public static ES3Type Instance;

        public ES3UserType_CrewMemberArray() : base(typeof(CrewMember[]), ES3UserType_CrewMember.Instance)
        {
            Instance = this;
        }
    }
}
