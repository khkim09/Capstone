using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

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

        foreach (CrewEffect crewEffect in crewEffects)
        {
            CrewRace targetRace = crewEffect.raceType;

            foreach (CrewMember crewMember in crewList)
            {
                if (targetRace != CrewRace.None)
                    if (targetRace != crewMember.race)
                        continue;

                switch (crewEffect.effectType)
                {
                    case CrewEffectType.ChangeHealth:
                        if (crewEffect.changeAmount >= 0)
                            crewMember.Heal(crewEffect.changeAmount);
                        else
                            crewMember.TakeDamage(crewEffect.changeAmount);
                        break;
                    case CrewEffectType.ChangeMorale:
                        MoraleManager.Instance.RegisterMoraleEffect(crewEffect);
                        break;
                    case CrewEffectType.ApplyStatusEffect:
                        // TODO : 상태 이상 적용하게 만들기
                        break;
                    case CrewEffectType.ChangeSkill:
                        // TODO : 숙련도 변화하는 이벤드도 구현
                        break;
                }
            }
        }
    }
}
