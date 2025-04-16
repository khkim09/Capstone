using System;
using System.Collections.Generic;

// 이벤트 결과
[Serializable]
public class EventOutcome
{
    public string outcomeText;
    public float probability; // 0-100 사이의 확률
    public List<PlanetEffect> planetEffects = new();
    public List<ResourceEffect> resourceEffects = new();
    public List<CrewEffect> crewEffects = new();

    // 특수 효과 (예: 다른 이벤트 트리거, 퀘스트 추가 등)
    public SpecialEffectType specialEffectType = SpecialEffectType.None;
    public RandomEvent nextEvent; // 연계 이벤트가 있을 경우
}
