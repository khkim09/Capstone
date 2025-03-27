using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CrewRaceStat", menuName = "Crew/CrewRaceStat")]
public class CrewRaceStat : ScriptableObject
{
    public class RaceStat
    {
        public CrewRace race;
        public float maxHealth;
        public float attack;
        public float defense;
        public float learningSpeed;
        public bool needsOxygen;

        public Dictionary<SkillType, float> maxSkillValueArray = new();

        public float initialPilotSkill;
        public float initialEngineSkill;
        public float initialPowerSkill;
        public float initialShieldSkill;
        public float initialWeaponSkill;
        public float initialAmmunitionSkill;
        public float initialMedBaySkill;
        public float initialRepairSkill;
    }

    public Dictionary<CrewRace, RaceStat> ByRace;

    private void InitializeDefaultStats()
    {
        ByRace = new Dictionary<CrewRace, RaceStat>();

        // 부정형 (CrewRace.Amorphous) : 기본 숙련도 / 최대 숙련도 = 100/150, 80/150, 80/150, 80/150, 100/150, 60/150, 100/150, 80/150
        ByRace.Add(CrewRace.Amorphous,
            new RaceStat()
            {
                race = CrewRace.Amorphous,
                maxHealth = 100f,
                attack = 9f,
                defense = 6f,
                learningSpeed = 0.8f,
                needsOxygen = false,
                initialPilotSkill = 100f,
                initialEngineSkill = 80f,
                initialPowerSkill = 80f,
                initialShieldSkill = 80f,
                initialWeaponSkill = 100f,
                initialAmmunitionSkill = 60f,
                initialMedBaySkill = 100f,
                initialRepairSkill = 80f,
                maxSkillValueArray = new Dictionary<SkillType, float>()
                {
                    { SkillType.PilotSkill, 150f },
                    { SkillType.EngineSkill, 150f },
                    { SkillType.PowerSkill, 150f },
                    { SkillType.ShieldSkill, 150f },
                    { SkillType.WeaponSkill, 150f },
                    { SkillType.AmmunitionSkill, 150f },
                    { SkillType.MedBaySkill, 150f },
                    { SkillType.RepairSkill, 150f }
                }
            });

        // 인간형 (CrewRace.Human) : 80/120, 100/120, 100/120, 80/120, 80/120, 100/120, 80/120, 100/120
        ByRace.Add(CrewRace.Human,
            new RaceStat()
            {
                race = CrewRace.Human,
                maxHealth = 100f,
                attack = 8f,
                defense = 8f,
                learningSpeed = 1.5f,
                needsOxygen = true,
                initialPilotSkill = 80f,
                initialEngineSkill = 100f,
                initialPowerSkill = 100f,
                initialShieldSkill = 80f,
                initialWeaponSkill = 80f,
                initialAmmunitionSkill = 100f,
                initialMedBaySkill = 80f,
                initialRepairSkill = 100f,
                maxSkillValueArray = new Dictionary<SkillType, float>()
                {
                    { SkillType.PilotSkill, 120f },
                    { SkillType.EngineSkill, 120f },
                    { SkillType.PowerSkill, 120f },
                    { SkillType.ShieldSkill, 120f },
                    { SkillType.WeaponSkill, 120f },
                    { SkillType.AmmunitionSkill, 120f },
                    { SkillType.MedBaySkill, 120f },
                    { SkillType.RepairSkill, 120f }
                }
            });

        // 짐승형 (CrewRace.Beast) : 50/80, 40/80, 40/80, 50/80, 60/120, 100/120, 40/100, 70/120
        ByRace.Add(CrewRace.Beast,
            new RaceStat()
            {
                race = CrewRace.Beast,
                maxHealth = 120f,
                attack = 12f,
                defense = 12f,
                learningSpeed = 0.8f,
                needsOxygen = true,
                initialPilotSkill = 50f,
                initialEngineSkill = 40f,
                initialPowerSkill = 40f,
                initialShieldSkill = 50f,
                initialWeaponSkill = 60f,
                initialAmmunitionSkill = 100f,
                initialMedBaySkill = 40f,
                initialRepairSkill = 70f,
                maxSkillValueArray = new Dictionary<SkillType, float>()
                {
                    { SkillType.PilotSkill, 80f },
                    { SkillType.EngineSkill, 80f },
                    { SkillType.PowerSkill, 80f },
                    { SkillType.ShieldSkill, 80f },
                    { SkillType.WeaponSkill, 120f },
                    { SkillType.AmmunitionSkill, 120f },
                    { SkillType.MedBaySkill, 100f },
                    { SkillType.RepairSkill, 120f }
                }
            });

        // 곤충형 (CrewRace.Insect) : 100/150, 50/120, 50/120, 50/120, 100/150, 80/150, 80/120, 50/120
        ByRace.Add(CrewRace.Insect,
            new RaceStat()
            {
                race = CrewRace.Insect,
                maxHealth = 120f,
                attack = 10f,
                defense = 15f,
                learningSpeed = 1f,
                needsOxygen = true,
                initialPilotSkill = 100f,
                initialEngineSkill = 50f,
                initialPowerSkill = 50f,
                initialShieldSkill = 50f,
                initialWeaponSkill = 100f,
                initialAmmunitionSkill = 80f,
                initialMedBaySkill = 80f,
                initialRepairSkill = 50f,
                maxSkillValueArray = new Dictionary<SkillType, float>()
                {
                    { SkillType.PilotSkill, 150f },
                    { SkillType.EngineSkill, 120f },
                    { SkillType.PowerSkill, 120f },
                    { SkillType.ShieldSkill, 120f },
                    { SkillType.WeaponSkill, 150f },
                    { SkillType.AmmunitionSkill, 150f },
                    { SkillType.MedBaySkill, 120f },
                    { SkillType.RepairSkill, 120f }
                }
            });

        // 기계형(지원형) (CrewRace.MechanicSup) : 조종실 100/100, 엔진실 100/100, 전력실 100/100,
        // 배리어실, 조준석, 탄약고는 작업 불가 (0), 의무실 120/120, 수리 120/120
        ByRace.Add(CrewRace.MechanicSup,
            new RaceStat()
            {
                race = CrewRace.MechanicSup,
                maxHealth = 70f,
                attack = 5f,
                defense = 5f,
                learningSpeed = 0f,
                needsOxygen = false,
                initialPilotSkill = 100f,
                initialEngineSkill = 100f,
                initialPowerSkill = 100f,
                initialShieldSkill = 0f,
                initialWeaponSkill = 0f,
                initialAmmunitionSkill = 0f,
                initialMedBaySkill = 120f,
                initialRepairSkill = 120f,
                maxSkillValueArray = new Dictionary<SkillType, float>()
                {
                    { SkillType.PilotSkill, 100f },
                    { SkillType.EngineSkill, 100f },
                    { SkillType.PowerSkill, 100f },
                    { SkillType.ShieldSkill, 0f },
                    { SkillType.WeaponSkill, 0f },
                    { SkillType.AmmunitionSkill, 0f },
                    { SkillType.MedBaySkill, 120f },
                    { SkillType.RepairSkill, 120f }
                }
            });

        // 기계형(돌격형) (CrewRace.MechanicTank) : 조준석 120/120, 탄약고 120/120, 나머지는 작업 불가 (0)
        ByRace.Add(CrewRace.MechanicTank,
            new RaceStat()
            {
                race = CrewRace.MechanicTank,
                maxHealth = 120f,
                attack = 14f,
                defense = 10f,
                learningSpeed = 0f,
                needsOxygen = false,
                initialPilotSkill = 0f,
                initialEngineSkill = 0f,
                initialPowerSkill = 0f,
                initialShieldSkill = 0f,
                initialWeaponSkill = 120f,
                initialAmmunitionSkill = 120f,
                initialMedBaySkill = 0f,
                initialRepairSkill = 0f,
                maxSkillValueArray = new Dictionary<SkillType, float>()
                {
                    { SkillType.PilotSkill, 0f },
                    { SkillType.EngineSkill, 0f },
                    { SkillType.PowerSkill, 0f },
                    { SkillType.ShieldSkill, 0f },
                    { SkillType.WeaponSkill, 120f },
                    { SkillType.AmmunitionSkill, 120f },
                    { SkillType.MedBaySkill, 0f },
                    { SkillType.RepairSkill, 0f }
                }
            });
    }

    private void OnEnable()
    {
        InitializeDefaultStats();
    }
}
