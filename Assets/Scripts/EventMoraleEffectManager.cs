using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이벤트로 발생한 사기 증감 관리 매니저
/// </summary>
public class EventMoraleEffectManager : MonoBehaviour
{
    public static EventMoraleEffectManager Instance { get; private set; }

    /// <summary>
    /// 현재 작용중인 불가사의
    /// </summary>
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

    /// <summary>
    /// 새로운 불가사의/이벤트 사기 효과 등록
    /// </summary>
    /// <param name="moraleEvent"></param>
    public void RegisterMoraleEffect(MoraleEffectEvent moraleEvent)
    {
        activeEvents.Add(moraleEvent);
        ApplyMoraleEffect(moraleEvent);
    }

    /// <summary>
    /// 현재 이벤트 상태 확인 후 만료된 효과 제거
    /// </summary>
    /// <param name="currentYear"></param>
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

    /// <summary>
    /// 사기 증감 효과 적용 (종족 또는 전체)
    /// </summary>
    /// <param name="effect"></param>
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

    /// <summary>
    /// 효과 제거 (종족 또는 전체)
    /// </summary>
    /// <param name="effect"></param>
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

    /// <summary>
    /// 불가사의 적용
    /// </summary>
    /// <param name="data"></param>
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

/// <summary>
/// 사기 효과
/// </summary>
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
