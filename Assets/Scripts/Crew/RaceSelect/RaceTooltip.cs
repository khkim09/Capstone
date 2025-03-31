using UnityEngine;
using TMPro;
using System.Reflection;
using System.Text;

/// <summary>
/// 종족 별 설명에 나올 details 정보
/// </summary>
public class RaceTooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI raceNameText;
    [SerializeField] private TextMeshProUGUI raceInfoText;

    // 예시: 각 종족의 능력치를 설정하는 함수
    public void SetupTooltip(CrewRaceStat raceStat, CrewRace race)
    {
        if (raceStat == null)
            return;

        // Race Name 설정
        raceNameText.text = raceStat.ByRace[race].race.ToString();

        // Race Info 설정 (maxSkillValueArray는 제외)
        raceInfoText.text = $"Max Health : {raceStat.ByRace[race].maxHealth}\n" +
                            $"Attack : {raceStat.ByRace[race].attack}\n" +
                            $"Defense : {raceStat.ByRace[race].defense}\n" +
                            $"Learning Speed : {raceStat.ByRace[race].learningSpeed}\n" +
                            $"Needs Oxygen : {(raceStat.ByRace[race].needsOxygen ? "Yes" : "No")}\n" +
                            $"Pilot Skill : {raceStat.ByRace[race].initialPilotSkill}\n" +
                            $"Engine Skill : {raceStat.ByRace[race].initialEngineSkill}\n" +
                            $"Power Skill : {raceStat.ByRace[race].initialPowerSkill}\n" +
                            $"Shield Skill : {raceStat.ByRace[race].initialShieldSkill}\n" +
                            $"Weapon Skill : {raceStat.ByRace[race].initialWeaponSkill}\n" +
                            $"Ammunition Skill : {raceStat.ByRace[race].initialAmmunitionSkill}\n" +
                            $"MedBay Skill : {raceStat.ByRace[race].initialMedBaySkill}\n" +
                            $"Repair Skill : {raceStat.ByRace[race].initialRepairSkill}";
    }
}
