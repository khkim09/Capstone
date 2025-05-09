using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이벤트 결과로 승무원 효과를 적용하는 유틸성 클래스입니다.
/// SpecialEffectType 시스템과는 무관하게 EventOutcome의 crewEffects를 직접 처리합니다.
/// </summary>
public class EventCrewEffectHandler
{
    /// <summary>
    /// 단일 승무원 효과를 리스트로 감싸서 처리합니다.
    /// </summary>
    /// <param name="effect">적용할 단일 승무원 효과</param>
    public void Handle(CrewEffect effect)
    {
        if (effect == null)
        {
            Debug.LogWarning("[EventCrewEffectHandler] 전달된 CrewEffect가 null입니다.");
            return;
        }

        ApplyEffects(new List<CrewEffect> { effect });
    }

    /// <summary>
    /// 이벤트로 전달된 CrewEffect 리스트를 전체 선원에게 적용합니다.
    /// </summary>
    /// <param name="crewEffects">적용할 효과 목록</param>
    public void ApplyEffects(List<CrewEffect> crewEffects)
    {
        if (crewEffects == null || crewEffects.Count == 0)
        {
            Debug.LogWarning("[EventCrewEffectHandler] 적용할 승무원 효과가 없습니다.");
            return;
        }

        List<CrewMember> crewList = GameManager.Instance.GetPlayerShip().GetAllCrew();
        foreach (CrewMember crew in crewList)
        {
            CrewBase baseCrew = crew; // 필요 시 형변환
        }

        foreach (CrewBase baseCrew in crewList)
        {
            if (baseCrew is not CrewMember crew)
                continue;

            foreach (CrewEffect effect in crewEffects)
            {
                // 해당 종족만 적용
                if (effect.raceType != CrewRace.None && crew.race != effect.raceType)
                    continue;

                // 체력 변화
                if (effect.changeAmount != 0)
                    crew.health += effect.changeAmount;

                // 스킬 변화
                if (effect.skill != SkillType.None && effect.changeAmount != 0)
                {
                    if (crew.skills.ContainsKey(effect.skill))
                        crew.skills[effect.skill] += effect.changeAmount;
                    else
                        crew.skills[effect.skill] = effect.changeAmount;
                }
            }
        }

        // // 사기 효과는 시스템 전체에 등록
        // foreach (CrewEffect effect in crewEffects)
        // {
        //     MoraleEffect morale = effect.moralChange;
        //
        //     if (morale.value == 0)
        //         continue;
        //
        //     MoraleEffectEvent moraleEvent = new MoraleEffectEvent
        //     {
        //         isGlobal = morale.isGlobal,
        //         raceTarget = morale.raceTarget,
        //         value = morale.value,
        //         durationYears = 0, // 즉시 적용
        //         startYear = GameManager.Instance.CurrentYear,
        //         sourceEventName = "EventCrewEffectHandler"
        //     };
        //
        //     EventMoraleEffectManager.Instance.RegisterMoraleEffect(moraleEvent);
        // }
    }
}
