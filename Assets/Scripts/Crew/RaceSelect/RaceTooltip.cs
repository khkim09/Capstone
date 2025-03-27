using UnityEngine;
using TMPro;
using System.Reflection;
using System.Text;

public class RaceTooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI raceNameText;
    [SerializeField] private TextMeshProUGUI raceInfoText;

    // 예시: 각 종족의 능력치를 설정하는 함수
    public void SetupTooltip(CrewRaceStat raceStat)
    {
        if (raceStat == null)
            return;

        // Race Name 설정
        raceNameText.text = raceStat.race.ToString();
        raceNameText.text =

        // Race Info 설정 (maxSkillValueArray는 제외)
        raceInfoText.text = $"Max Health : {raceStat.maxHealth}\n" +
                            $"Attack : {raceStat.attack}\n" +
                            $"Defense : {raceStat.defense}\n" +
                            $"Learning Speed : {raceStat.learningSpeed}\n" +
                            $"Needs Oxygen : {(raceStat.needsOxygen ? "Yes" : "No")}\n" +
                            $"Pilot Skill : {raceStat.initialPilotSkill}\n" +
                            $"Engine Skill : {raceStat.initialEngineSkill}\n" +
                            $"Power Skill : {raceStat.initialPowerSkill}\n" +
                            $"Shield Skill : {raceStat.initialShieldSkill}\n" +
                            $"Weapon Skill : {raceStat.initialWeaponSkill}\n" +
                            $"Ammunition Skill : {raceStat.initialAmmunitionSkill}\n" +
                            $"MedBay Skill : {raceStat.initialMedBaySkill}\n" +
                            $"Repair Skill : {raceStat.initialRepairSkill}";
    }
}
