using UnityEngine;
using TMPro;
using System.Reflection;
using System.Text;

/// <summary>
/// 종족 별 설명에 나올 details 정보
/// </summary>
public class RaceTooltip : MonoBehaviour
{
    /// <summary>
    /// 종족 이름을 표시하는 TextMeshPro UI 텍스트입니다.
    /// </summary>
    [SerializeField] private TextMeshProUGUI raceNameText;

    /// <summary>
    /// 종족 능력치 상세 정보를 표시하는 TextMeshPro UI 텍스트입니다.
    /// </summary>
    [SerializeField] private TextMeshProUGUI raceInfoText;

    /// <summary>
    /// 전달된 종족 능력치 데이터를 기반으로 툴팁 UI를 구성합니다.
    /// 능력치 정보는 텍스트로 출력되며, maxSkillValueArray는 제외됩니다.
    /// </summary>
    /// <param name="raceStat">종족별 능력치 정보를 포함하는 데이터 객체.</param>
    /// <param name="race">표시할 대상 종족.</param>
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
