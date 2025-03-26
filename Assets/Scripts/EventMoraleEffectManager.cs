using System;
using System.Collections.Generic;
using UnityEngine;

public class EventMoraleEffectManager : MonoBehaviour
{
    public static EventMoraleEffectManager Instance { get; private set; }

    private List<MoraleEffectEvent> activeEvents = new List<MoraleEffectEvent>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        // GameManager.Instance.CurrentYear를 참조하도록 수정
        CheckEventExpirations(GameManager.Instance.CurrentYear);
    }

    // 새로운 불가사의/이벤트 사기 효과 등록
    public void RegisterMoraleEffect(MoraleEffectEvent moraleEvent)
    {
        activeEvents.Add(moraleEvent);
        ApplyMoraleEffect(moraleEvent);
    }

    // 현재 이벤트 상태 확인 후 만료된 효과 제거
    public void CheckEventExpirations(int currentYear)
    {
        for (int i = activeEvents.Count - 1; i >= 0; i--)
        {
            if (currentYear >= activeEvents[i].endYear)
            {
                RemoveMoraleEffect(activeEvents[i]);
                activeEvents.RemoveAt(i);
            }
        }
    }

    // 효과 적용 (종족 또는 전체)
    private void ApplyMoraleEffect(MoraleEffectEvent effect)
    {
        if (effect.isGlobal)
        {
            float newMorale = MoraleManager.Instance.globalMorale + effect.value;
            MoraleManager.Instance.SetAllCrewMorale(newMorale);
        }
        else
        {
            float current = MoraleManager.Instance.GetRaceMorale(effect.raceTarget);
            MoraleManager.Instance.SetRaceMorale(effect.raceTarget, current + effect.value);
        }
    }

    // 효과 제거 (종족 또는 전체)
    private void RemoveMoraleEffect(MoraleEffectEvent effect)
    {
        if (effect.isGlobal)
        {
            float newMorale = MoraleManager.Instance.globalMorale - effect.value;
            MoraleManager.Instance.SetAllCrewMorale(newMorale);
        }
        else
        {
            float current = MoraleManager.Instance.GetRaceMorale(effect.raceTarget);
            MoraleManager.Instance.SetRaceMorale(effect.raceTarget, current - effect.value);
        }
    }

    // 불가사의 적용
    public void RegisterMysteryEvent(MysteryEventData data)
    {
        int currentYear = GameManager.Instance.CurrentYear;

        foreach (MoraleEffect effect in data.moraleEffects)
        {
            MoraleEffectEvent evt = new MoraleEffectEvent
            {
                isGlobal = effect.isGlobal,
                raceTarget = effect.raceTarget,
                value = effect.value,
                durationYears = data.durationYears,
                startYear = currentYear,
                sourceEventName = data.eventName
            };
            RegisterMoraleEffect(evt);
        }
    }
}

[Serializable]
public class MoraleEffectEvent
{
    public bool isGlobal; // true면 전 종족, false면 특정 종족
    public CrewRace raceTarget; // 특정 종족 대상
    public float value; // 적용되는 사기 값 (+/- 가능)
    public int durationYears; // 지속 시간 (년 단위)
    public int startYear; // 시작 연도
    public int endYear => startYear + durationYears; // 종료 연도
    public string sourceEventName; // 이벤트 이름 (디버깅/식별용)
}
