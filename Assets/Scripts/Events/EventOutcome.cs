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

    /// <summary>
    /// 특수 효과에 대한 값.
    /// (예 : 방이 파괴되어야하면 파괴되어야 하는 방 타입,
    /// 전투가 되어야하면 전투 하는 적 함선 템플릿 이름 등등)
    /// 방 타입의 경우 None 이면 랜덤
    /// </summary>
    public string specialEffectValue;

    /// <summary>
    /// 특수 효과 수치
    /// (예 : 방이 파괴되어야 한다면, 파괴되는 단계 (1단계 or 2단계)
    /// </summary>
    public float specialEffectAmount;

    /// <summary>
    /// 만약 특수 효과 타입이 NextEvent 일 경우
    /// </summary>
    public RandomEvent nextEvent;

    public RandomQuest questToAdd; // 연계 퀘스트가 있을 경우
}
