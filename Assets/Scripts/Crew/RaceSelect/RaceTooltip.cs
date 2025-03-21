using UnityEngine;
using TMPro;
using System.Reflection;
using System.Text;

public class RaceTooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI raceNameText;
    [SerializeField] private TextMeshProUGUI raceInfoText;

    // 예시: 각 종족의 능력치를 설정하는 함수
    public void SetupTooltip(CrewRaceSettings raceSettings)
    {
        if (raceSettings == null)
            return;

        // Race Name 설정
        raceNameText.text = raceSettings.race.ToString();

        // Race Info 설정 (maxSkillValueArray는 제외)
        raceInfoText.text = $"Max Health : {raceSettings.maxHealth}\n" +
                            $"Attack : {raceSettings.attack}\n" +
                            $"Defense : {raceSettings.defense}\n" +
                            $"Learning Speed : {raceSettings.learningSpeed}\n" +
                            $"Needs Oxygen : {(raceSettings.needsOxygen ? "Yes" : "No")}\n" +
                            $"Pilot Skill : {raceSettings.initialPilotSkill}\n" +
                            $"Engine Skill : {raceSettings.initialEngineSkill}\n" +
                            $"Power Skill : {raceSettings.initialPowerSkill}\n" +
                            $"Shield Skill : {raceSettings.initialShieldSkill}\n" +
                            $"Weapon Skill : {raceSettings.initialWeaponSkill}\n" +
                            $"Ammunition Skill : {raceSettings.initialAmmunitionSkill}\n" +
                            $"MedBay Skill : {raceSettings.initialMedBaySkill}\n" +
                            $"Repair Skill : {raceSettings.initialRepairSkill}";
    }
}
