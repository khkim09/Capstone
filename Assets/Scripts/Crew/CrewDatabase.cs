using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

/// <summary>
/// 모든 선원 관련 데이터를 관리하는 중앙 데이터베이스
/// CrewRaceStat의 기능을 통합하여 데이터베이스 패턴을 일관성 있게 유지
/// </summary>
[CreateAssetMenu(fileName = "CrewDatabase", menuName = "Crew/CrewDatabase")]
public class CrewDatabase : ScriptableObject
{
    /// <summary>
    /// 종족별 기본 스탯 데이터 목록
    /// </summary>
    [SerializeField] private List<RaceStat> raceStats = new();

    /// <summary>
    /// 런타임용 종족별 스탯 딕셔너리
    /// </summary>
    private Dictionary<CrewRace, RaceStat> raceStatsByType;

    /// <summary>
    /// 종족 별 숙련도 저장 클래스
    /// </summary>
    [Serializable]
    public class SkillValue
    {
        public SkillType skillType;
        public float maxValue;
    }

    /// <summary>
    /// 종족 별 기본 정보 클래스
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
        /// 최대 숙련도 저장 리스트
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
        /// </summary>
        public Dictionary<SkillType, float> GetMaxSkillValueDictionary()
        {
            Dictionary<SkillType, float> dict = new();
            foreach (SkillValue skill in maxSkillValues) dict[skill.skillType] = skill.maxValue;
            return dict;
        }

        /// <summary>
        /// Dictionary에서 List로 값을 설정하는 메서드
        /// </summary>
        public void SetMaxSkillValuesFromDictionary(Dictionary<SkillType, float> dictionary)
        {
            maxSkillValues.Clear();
            foreach (KeyValuePair<SkillType, float> pair in dictionary)
                maxSkillValues.Add(new SkillValue { skillType = pair.Key, maxValue = pair.Value });
        }
    }

    /// <summary>
    /// 종족별 스탯 dictionary 제공
    /// </summary>
    public Dictionary<CrewRace, RaceStat> ByRace
    {
        get
        {
            if (raceStatsByType == null || raceStatsByType.Count == 0) InitializeDictionary();
            return raceStatsByType;
        }
    }

    /// <summary>
    /// 데이터베이스 초기화
    /// </summary>
    public void InitializeDictionary()
    {
        raceStatsByType = new Dictionary<CrewRace, RaceStat>();
        foreach (RaceStat stat in raceStats) raceStatsByType[stat.race] = stat;
    }

    /// <summary>
    /// Unity 이벤트: 값이 변경될 때마다 호출됨
    /// </summary>
    private void OnValidate()
    {
        if (raceStats.Count == 0) InitializeDefaultStats();
    }

    /// <summary>
    /// Unity 이벤트: ScriptableObject가 로드될 때 호출됨
    /// </summary>
    private void OnEnable()
    {
        if (raceStats.Count == 0) InitializeDefaultStats();
        InitializeDictionary();
    }

    /// <summary>
    /// 종족 별 기본값 초기화
    /// </summary>
    public void InitializeDefaultStats()
    {
        raceStats = new List<RaceStat>();
        raceStatsByType = new Dictionary<CrewRace, RaceStat>();

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
        raceStatsByType[CrewRace.Amorphous] = amorphousStat;

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
        raceStatsByType[CrewRace.Human] = humanStat;

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
        raceStatsByType[CrewRace.Beast] = beastStat;

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
        raceStatsByType[CrewRace.Insect] = insectStat;

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
        raceStatsByType[CrewRace.MechanicSup] = mechSupStat;

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
        raceStatsByType[CrewRace.MechanicTank] = mechTankStat;
    }

    /// <summary>
    /// 특정 종족의 스탯 정보 반환
    /// </summary>
    /// <param name="race">종족 타입</param>
    /// <returns>종족 스탯 정보</returns>
    public RaceStat GetRaceStat(CrewRace race)
    {
        if (raceStatsByType == null || raceStatsByType.Count == 0) InitializeDictionary();

        if (raceStatsByType.TryGetValue(race, out RaceStat stat)) return stat;

        Debug.LogWarning($"종족 {race}에 대한 스탯 정보를 찾을 수 없습니다.");
        return null;
    }

    /// <summary>
    /// 모든 종족 스탯 정보 반환
    /// </summary>
    /// <returns>종족별 스탯 정보 딕셔너리</returns>
    public Dictionary<CrewRace, RaceStat> GetAllRaceStats()
    {
        if (raceStatsByType == null || raceStatsByType.Count == 0) InitializeDictionary();

        return new Dictionary<CrewRace, RaceStat>(raceStatsByType);
    }

    /// <summary>
    /// 특정 스킬의 최대값 반환
    /// </summary>
    /// <param name="race">종족 타입</param>
    /// <param name="skillType">스킬 타입</param>
    /// <returns>최대 스킬값 (종족 정보가 없으면 0 반환)</returns>
    public float GetMaxSkillValue(CrewRace race, SkillType skillType)
    {
        RaceStat raceStat = GetRaceStat(race);
        if (raceStat == null)
            return 0f;

        Dictionary<SkillType, float> maxSkills = raceStat.GetMaxSkillValueDictionary();
        if (maxSkills.TryGetValue(skillType, out float value))
            return value;

        return 0f;
    }

    /// <summary>
    /// 특정 종족의 초기 스킬값 반환
    /// </summary>
    /// <param name="race">종족 타입</param>
    /// <param name="skillType">스킬 타입</param>
    /// <returns>초기 스킬값</returns>
    public float GetInitialSkillValue(CrewRace race, SkillType skillType)
    {
        RaceStat raceStat = GetRaceStat(race);
        if (raceStat == null)
            return 0f;

        switch (skillType)
        {
            case SkillType.PilotSkill:
                return raceStat.initialPilotSkill;
            case SkillType.EngineSkill:
                return raceStat.initialEngineSkill;
            case SkillType.PowerSkill:
                return raceStat.initialPowerSkill;
            case SkillType.ShieldSkill:
                return raceStat.initialShieldSkill;
            case SkillType.WeaponSkill:
                return raceStat.initialWeaponSkill;
            case SkillType.AmmunitionSkill:
                return raceStat.initialAmmunitionSkill;
            case SkillType.MedBaySkill:
                return raceStat.initialMedBaySkill;
            case SkillType.RepairSkill:
                return raceStat.initialRepairSkill;
            default:
                return 0f;
        }
    }

    /// <summary>
    /// 특정 종족이 산소가 필요한지 여부 반환
    /// </summary>
    /// <param name="race">종족 타입</param>
    /// <returns>산소 필요 여부</returns>
    public bool DoesRaceNeedOxygen(CrewRace race)
    {
        RaceStat raceStat = GetRaceStat(race);
        return raceStat != null && raceStat.needsOxygen;
    }

    /// <summary>
    /// 특정 종족의 학습 속도 반환
    /// </summary>
    /// <param name="race">종족 타입</param>
    /// <returns>학습 속도</returns>
    public float GetRaceLearningSpeed(CrewRace race)
    {
        RaceStat raceStat = GetRaceStat(race);
        return raceStat != null ? raceStat.learningSpeed : 1.0f;
    }

    /// <summary>
    /// 데이터베이스 전체 데이터를 JSON 형태로 직렬화
    /// </summary>
    /// <returns>JSON 문자열</returns>
    public string SerializeToJson()
    {
        // 직렬화할 데이터 구조체
        var serializableData = new { RaceStats = new List<object>() };

        // 각 종족별 데이터를 직렬화 가능한 형태로 변환
        foreach (KeyValuePair<CrewRace, RaceStat> kvp in ByRace)
        {
            RaceStat stat = kvp.Value;
            Dictionary<SkillType, float> maxSkills = stat.GetMaxSkillValueDictionary();

            var raceData = new
            {
                Race = stat.race.ToString(),
                MaxHealth = stat.maxHealth,
                Attack = stat.attack,
                Defense = stat.defense,
                LearningSpeed = stat.learningSpeed,
                NeedsOxygen = stat.needsOxygen,
                InitialSkills = new Dictionary<string, float>
                {
                    { "PilotSkill", stat.initialPilotSkill },
                    { "EngineSkill", stat.initialEngineSkill },
                    { "PowerSkill", stat.initialPowerSkill },
                    { "ShieldSkill", stat.initialShieldSkill },
                    { "WeaponSkill", stat.initialWeaponSkill },
                    { "AmmunitionSkill", stat.initialAmmunitionSkill },
                    { "MedBaySkill", stat.initialMedBaySkill },
                    { "RepairSkill", stat.initialRepairSkill }
                },
                MaxSkills = new Dictionary<string, float>()
            };

            // 최대 스킬값 변환
            foreach (KeyValuePair<SkillType, float> skillPair in maxSkills)
                raceData.MaxSkills[skillPair.Key.ToString()] = skillPair.Value;

            serializableData.RaceStats.Add(raceData);
        }

        // JSON으로 직렬화
        return JsonConvert.SerializeObject(serializableData, Formatting.Indented);
    }

    /// <summary>
    /// JSON 문자열에서 데이터베이스 데이터 복원
    /// </summary>
    /// <param name="json">JSON 문자열</param>
    /// <returns>성공 여부</returns>
    public bool DeserializeFromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return false;

        try
        {
            // JSON 파싱
            SerializableRaceStatData serializableData = JsonConvert.DeserializeObject<SerializableRaceStatData>(json);
            if (serializableData == null || serializableData.RaceStats == null)
                return false;

            // 각 종족별 데이터 복원
            foreach (RaceStatData raceData in serializableData.RaceStats)
            {
                if (!Enum.TryParse<CrewRace>(raceData.Race, out CrewRace race))
                {
                    Debug.LogWarning($"알 수 없는 종족 타입: {raceData.Race}");
                    continue;
                }

                // 기존 데이터가 있는지 확인
                if (ByRace.TryGetValue(race, out RaceStat stat))
                {
                    // 기존 종족 데이터 업데이트
                    stat.maxHealth = raceData.MaxHealth;
                    stat.attack = raceData.Attack;
                    stat.defense = raceData.Defense;
                    stat.learningSpeed = raceData.LearningSpeed;
                    stat.needsOxygen = raceData.NeedsOxygen;

                    // 초기 스킬값 업데이트
                    if (raceData.InitialSkills != null)
                    {
                        if (raceData.InitialSkills.TryGetValue("PilotSkill", out float value))
                            stat.initialPilotSkill = value;
                        if (raceData.InitialSkills.TryGetValue("EngineSkill", out value))
                            stat.initialEngineSkill = value;
                        if (raceData.InitialSkills.TryGetValue("PowerSkill", out value))
                            stat.initialPowerSkill = value;
                        if (raceData.InitialSkills.TryGetValue("ShieldSkill", out value))
                            stat.initialShieldSkill = value;
                        if (raceData.InitialSkills.TryGetValue("WeaponSkill", out value))
                            stat.initialWeaponSkill = value;
                        if (raceData.InitialSkills.TryGetValue("AmmunitionSkill", out value))
                            stat.initialAmmunitionSkill = value;
                        if (raceData.InitialSkills.TryGetValue("MedBaySkill", out value))
                            stat.initialMedBaySkill = value;
                        if (raceData.InitialSkills.TryGetValue("RepairSkill", out value))
                            stat.initialRepairSkill = value;
                    }

                    // 최대 스킬값 업데이트
                    if (raceData.MaxSkills != null)
                    {
                        Dictionary<SkillType, float> maxSkills = new();
                        foreach (KeyValuePair<string, float> skillPair in raceData.MaxSkills)
                            if (Enum.TryParse<SkillType>(skillPair.Key, out SkillType skillType))
                                maxSkills[skillType] = skillPair.Value;

                        stat.SetMaxSkillValuesFromDictionary(maxSkills);
                    }
                }
                else
                {
                    // 새로운 종족 데이터 생성
                    RaceStat newStat = new()
                    {
                        race = race,
                        maxHealth = raceData.MaxHealth,
                        attack = raceData.Attack,
                        defense = raceData.Defense,
                        learningSpeed = raceData.LearningSpeed,
                        needsOxygen = raceData.NeedsOxygen
                    };

                    // 초기 스킬값 설정
                    if (raceData.InitialSkills != null)
                    {
                        if (raceData.InitialSkills.TryGetValue("PilotSkill", out float value))
                            newStat.initialPilotSkill = value;
                        if (raceData.InitialSkills.TryGetValue("EngineSkill", out value))
                            newStat.initialEngineSkill = value;
                        if (raceData.InitialSkills.TryGetValue("PowerSkill", out value))
                            newStat.initialPowerSkill = value;
                        if (raceData.InitialSkills.TryGetValue("ShieldSkill", out value))
                            newStat.initialShieldSkill = value;
                        if (raceData.InitialSkills.TryGetValue("WeaponSkill", out value))
                            newStat.initialWeaponSkill = value;
                        if (raceData.InitialSkills.TryGetValue("AmmunitionSkill", out value))
                            newStat.initialAmmunitionSkill = value;
                        if (raceData.InitialSkills.TryGetValue("MedBaySkill", out value))
                            newStat.initialMedBaySkill = value;
                        if (raceData.InitialSkills.TryGetValue("RepairSkill", out value))
                            newStat.initialRepairSkill = value;
                    }

                    // 최대 스킬값 설정
                    if (raceData.MaxSkills != null)
                    {
                        Dictionary<SkillType, float> maxSkills = new();
                        foreach (KeyValuePair<string, float> skillPair in raceData.MaxSkills)
                            if (Enum.TryParse<SkillType>(skillPair.Key, out SkillType skillType))
                                maxSkills[skillType] = skillPair.Value;

                        newStat.SetMaxSkillValuesFromDictionary(maxSkills);
                    }

                    // 데이터베이스에 추가
                    raceStats.Add(newStat);
                    raceStatsByType[race] = newStat;
                }
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"CrewDatabase 역직렬화 오류: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 직렬화를 위한 데이터 구조체
    /// </summary>
    private class SerializableRaceStatData
    {
        public List<RaceStatData> RaceStats { get; set; }
    }

    /// <summary>
    /// 각 종족별 직렬화 데이터 구조체
    /// </summary>
    private class RaceStatData
    {
        public string Race { get; set; }
        public float MaxHealth { get; set; }
        public float Attack { get; set; }
        public float Defense { get; set; }
        public float LearningSpeed { get; set; }
        public bool NeedsOxygen { get; set; }
        public Dictionary<string, float> InitialSkills { get; set; }
        public Dictionary<string, float> MaxSkills { get; set; }
    }
}
