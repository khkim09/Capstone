using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ResourceEffect 리스트를 받아 해당 자원을 증감시키는 유틸리티 핸들러입니다.
/// SpecialEffectType 처리와는 무관하게 EventOutcome의 resourceEffects에 직접 대응합니다.
/// </summary>
public class EventResourceEffectHandler
{
    /// <summary>
    /// 단일 자원 효과를 리스트로 감싸서 처리합니다.
    /// </summary>
    /// <param name="effect">적용할 단일 자원 효과</param>
    public void Handle(ResourceEffect effect)
    {
        if (effect == null)
        {
            Debug.LogWarning("[EventResourceEffectHandler] 전달된 ResourceEffect가 null입니다.");
            return;
        }

        ApplyEffects(new List<ResourceEffect> { effect });
    }
    /// <summary>
    /// 자원 효과 리스트를 받아 해당 자원을 증감시킵니다.
    /// </summary>
    /// <param name="resourceEffects">적용할 자원 효과 리스트</param>
    public void ApplyEffects(List<ResourceEffect> resourceEffects)
    {
        if (resourceEffects == null || resourceEffects.Count == 0)
        {
            Debug.LogWarning("[EventResourceEffectHandler] 적용할 자원 효과가 없습니다.");
            return;
        }

        foreach (var effect in resourceEffects)
        {
            ResourceManager.Instance.ChangeResource(effect.resourceType, effect.amount);
        }
    }
}
