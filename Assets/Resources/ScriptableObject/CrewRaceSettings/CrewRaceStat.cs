using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 종족 별 기본 값
/// </summary>
[CreateAssetMenu(fileName = "CrewRaceStat", menuName = "Crew/CrewRaceStat")]
public class CrewRaceStat : ScriptableObject
{
    /// <summary>
    /// 종족의 숙련도
    /// </summary>
    [Serializable]
    public class SkillValue
    {
        public SkillType skillType;
        public float maxValue;
    }

    /// <summary>
    /// 종족 별 기본 정보
    /// 최대 체력, 공격력, 방어력, 학습속도, 산소 호흡 여부
    /// </summary>
    [Serializable]
    public class RaceStat
    {
        public CrewRace race;
        public float maxHealth;
        public float attack;
        public float defense;
        public float learningSpeed;
        public bool needsOxygen;

        /// <summary>
        /// 최대 숙련도 저장 리스트트
        /// </summary>
        [SerializeField] private List<SkillValue> maxSkillValues = new();

        /// <summary>
        /// 기관별 최초 숙련도
        /// 직렬화 가능한 개별 필드
        /// </summary>
        public float initialPilotSkill;
        public float initialEngineSkill;
        public float initialPowerSkill;
        public float initialShieldSkill;
        public float initialWeaponSkill;
        public float initialAmmunitionSkill;
        public float initialMedBaySkill;
        public float initialRepairSkill;

        /// <summary>
        /// 숙련도 기관별 최대 숙련도를 호출합니다.
        /// 런타임에 Dictionary로 변환하는 메서드
        /// </summary>
        /// <returns></returns>
        public Dictionary<SkillType, float> GetMaxSkillValueDictionary()
        {
            Dictionary<SkillType, float> dict = new();
            foreach (SkillValue skill in maxSkillValues) dict[skill.skillType] = skill.maxValue;
            return dict;
        }

        /// <summary>
        /// Dictionary에서 List로 값을 설정하는 메서드
        /// </summary>
        /// <param name="dictionary"></param>
        public void SetMaxSkillValuesFromDictionary(Dictionary<SkillType, float> dictionary)
        {
            maxSkillValues.Clear();
            foreach (KeyValuePair<SkillType, float> pair in dictionary)
                maxSkillValues.Add(new SkillValue { skillType = pair.Key, maxValue = pair.Value });
        }
    }

    /// <summary>
    /// 인스펙터에서 보이도록 직렬화 가능한 리스트 사용
    /// </summary>
    [SerializeField] private List<RaceStat> raceStats = new();

    /// <summary>
    /// 런타임에만 사용할 Dictionary
    /// </summary>
    private Dictionary<CrewRace, RaceStat> byRace;

    public Dictionary<CrewRace, RaceStat> ByRace
    {
        get
        {
            if (byRace == null || byRace.Count == 0) InitializeDictionaryFromList();
            return byRace;
        }
    }

    /// <summary>
    /// 숙련도 초기화
    /// </summary>
    private void InitializeDictionaryFromList()
    {
        byRace = new Dictionary<CrewRace, RaceStat>();
        foreach (RaceStat stat in raceStats) byRace[stat.race] = stat;
    }

    private void OnValidate()
    {
        // 에디터에서 값이 변경될 때마다 호출됨
        if (raceStats.Count == 0) InitializeDefaultStats();
    }

    private void OnEnable()
    {
        // ScriptableObject가 로드될 때 호출됨
        if (raceStats.Count == 0) InitializeDefaultStats();
        InitializeDictionaryFromList();
    }

    /// <summary>
    /// 종족 별 기본 값 세팅
    /// </summary>
    private void InitializeDefaultStats()
    {
        raceStats = new List<RaceStat>();
        byRace = new Dictionary<CrewRace, RaceStat>();

        // 부정형 (CrewRace.Amorphous)
        RaceStat amorphousStat = new()
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
            initialRepairSkill = 80f
        };

        Dictionary<SkillType, float> amorphousSkills = new()
        {
            { SkillType.PilotSkill, 150f },
            { SkillType.EngineSkill, 150f },
            { SkillType.PowerSkill, 150f },
            { SkillType.ShieldSkill, 150f },
            { SkillType.WeaponSkill, 150f },
            { SkillType.AmmunitionSkill, 150f },
            { SkillType.MedBaySkill, 150f },
            { SkillType.RepairSkill, 150f }
        };
        amorphousStat.SetMaxSkillValuesFromDictionary(amorphousSkills);
        raceStats.Add(amorphousStat);
        byRace[CrewRace.Amorphous] = amorphousStat;

        // 인간형 (CrewRace.Human)
        RaceStat humanStat = new()
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
            initialRepairSkill = 100f
        };

        Dictionary<SkillType, float> humanSkills = new()
        {
            { SkillType.PilotSkill, 120f },
            { SkillType.EngineSkill, 120f },
            { SkillType.PowerSkill, 120f },
            { SkillType.ShieldSkill, 120f },
            { SkillType.WeaponSkill, 120f },
            { SkillType.AmmunitionSkill, 120f },
            { SkillType.MedBaySkill, 120f },
            { SkillType.RepairSkill, 120f }
        };
        humanStat.SetMaxSkillValuesFromDictionary(humanSkills);
        raceStats.Add(humanStat);
        byRace[CrewRace.Human] = humanStat;

        // 짐승형 (CrewRace.Beast)
        RaceStat beastStat = new()
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
            initialRepairSkill = 70f
        };

        Dictionary<SkillType, float> beastSkills = new()
        {
            { SkillType.PilotSkill, 80f },
            { SkillType.EngineSkill, 80f },
            { SkillType.PowerSkill, 80f },
            { SkillType.ShieldSkill, 80f },
            { SkillType.WeaponSkill, 120f },
            { SkillType.AmmunitionSkill, 120f },
            { SkillType.MedBaySkill, 100f },
            { SkillType.RepairSkill, 120f }
        };
        beastStat.SetMaxSkillValuesFromDictionary(beastSkills);
        raceStats.Add(beastStat);
        byRace[CrewRace.Beast] = beastStat;

        // 곤충형 (CrewRace.Insect)
        RaceStat insectStat = new()
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
            initialRepairSkill = 50f
        };

        Dictionary<SkillType, float> insectSkills = new()
        {
            { SkillType.PilotSkill, 150f },
            { SkillType.EngineSkill, 120f },
            { SkillType.PowerSkill, 120f },
            { SkillType.ShieldSkill, 120f },
            { SkillType.WeaponSkill, 150f },
            { SkillType.AmmunitionSkill, 150f },
            { SkillType.MedBaySkill, 120f },
            { SkillType.RepairSkill, 120f }
        };
        insectStat.SetMaxSkillValuesFromDictionary(insectSkills);
        raceStats.Add(insectStat);
        byRace[CrewRace.Insect] = insectStat;

        // 기계형(지원형) (CrewRace.MechanicSup)
        RaceStat mechSupStat = new()
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
            initialRepairSkill = 120f
        };

        Dictionary<SkillType, float> mechSupSkills = new()
        {
            { SkillType.PilotSkill, 100f },
            { SkillType.EngineSkill, 100f },
            { SkillType.PowerSkill, 100f },
            { SkillType.ShieldSkill, 0f },
            { SkillType.WeaponSkill, 0f },
            { SkillType.AmmunitionSkill, 0f },
            { SkillType.MedBaySkill, 120f },
            { SkillType.RepairSkill, 120f }
        };
        mechSupStat.SetMaxSkillValuesFromDictionary(mechSupSkills);
        raceStats.Add(mechSupStat);
        byRace[CrewRace.MechanicSup] = mechSupStat;

        // 기계형(돌격형) (CrewRace.MechanicTank)
        RaceStat mechTankStat = new()
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
            initialRepairSkill = 0f
        };

        Dictionary<SkillType, float> mechTankSkills = new()
        {
            { SkillType.PilotSkill, 0f },
            { SkillType.EngineSkill, 0f },
            { SkillType.PowerSkill, 0f },
            { SkillType.ShieldSkill, 0f },
            { SkillType.WeaponSkill, 120f },
            { SkillType.AmmunitionSkill, 120f },
            { SkillType.MedBaySkill, 0f },
            { SkillType.RepairSkill, 0f }
        };
        mechTankStat.SetMaxSkillValuesFromDictionary(mechTankSkills);
        raceStats.Add(mechTankStat);
        byRace[CrewRace.MechanicTank] = mechTankStat;
    }
}
