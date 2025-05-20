using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

/// <summary>
/// PlanetEffect를 받아 아이템 카테고리별 가격 변동을 처리하는 유틸리티 핸들러입니다.
/// SpecialEffectType 처리와는 무관하게 EventOutcome의 planetEffects에 직접 대응합니다.
/// </summary>
public class EventPlanetEffectHandler
{
    /// <summary>
    /// 단일 행성 효과를 리스트로 감싸서 처리합니다.
    /// </summary>
    /// <param name="effect">적용할 단일 PlanetEffect</param>
    public void Handle(PlanetEffect effect)
    {
        if (effect == null)
        {
            Debug.LogWarning("[EventPlanetEffectHandler] 전달된 PlanetEffect가 null입니다.");
            return;
        }

        ApplyEffects(new List<PlanetEffect> { effect });
    }

    /// <summary>
    /// PlanetEffect 리스트를 받아 로그 출력 및 현재 행성에 적용합니다.
    /// </summary>
    /// <param name="planetEffects">적용할 행성 효과 리스트</param>
    /// <returns>행성의 이름</returns>
    public void ApplyEffects(List<PlanetEffect> planetEffects)
    {
        if (planetEffects == null || planetEffects.Count == 0)
            Debug.LogWarning("[EventPlanetEffectHandler] 적용할 PlanetEffect가 없습니다.");

        // 각 효과를 현재 행성에 등록
        for (int i = 0; i < planetEffects.Count; ++i)
        {
            PlanetEffect effect = planetEffects[i];

            Debug.Log($"[PlanetEffect] 카테고리: {effect.categoryType}, 변동률: {effect.changeAmount}%");
            effect.startYear = GameManager.Instance.CurrentYear;
            // 행성에 효과 등록
            List<PlanetData> planetDatas =
                GameManager.Instance.PlanetDataList.Where(d => d.activeEffects.Count == 0).ToList();
            int index = Random.Range(0, planetDatas.Count);
            planetDatas[index].RegisterPlanetEffect(effect);
        }
    }
}
