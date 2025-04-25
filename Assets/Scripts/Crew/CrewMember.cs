using System;
using System.Collections.Generic;

/// <summary>
/// 선원의 데이터를 확장하여 실제 게임 내 선원(Crew)의 기능을 담당하는 클래스입니다.
/// </summary>
[Serializable]
public class CrewMember : CrewBase
{
    /// <summary>
    /// Unity 생명주기 메서드.
    /// 매 프레임마다 선원 상태를 갱신합니다. (현재는 구현 미완료)
    /// </summary>
    private void Update()
    {
    }

    /// <summary>
    /// 현재 선원의 모든 스킬 값을 계산하여 반환합니다.
    /// 기본 숙련도, 장비 보너스, 사기 보너스를 포함합니다.
    /// </summary>
    /// <returns>스킬 타입별 총 숙련도 값 딕셔너리.</returns>
    public virtual Dictionary<SkillType, float> GetCrewSkillValue()
    {
        Dictionary<SkillType, float> totalSkills = new();

        foreach (SkillType skill in Enum.GetValues(typeof(SkillType)))
        {
            if (skill == SkillType.None)
                continue;

            float baseSkill = skills.ContainsKey(skill) ? skills[skill] : 0f;
            float equipmentBonus = equipAdditionalSkills.ContainsKey(skill) ? equipAdditionalSkills[skill] : 0f;
            float moraleBonus = MoraleManager.Instance.GetTotalMoraleBonus(this); // 향후 보완 예정 -> morale manager 생성

            float total = baseSkill + equipmentBonus + moraleBonus;
            totalSkills[skill] = total;
        }

        return totalSkills;
    }
}
