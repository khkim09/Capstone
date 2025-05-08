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
    private List<MoraleEffectEvent> activeEvents = new();

    /// <summary>
    /// 이 클래스의 인스턴스를 초기화하고 싱글톤을 설정하는 메서드입니다.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// 매 프레임마다 현재 연도를 기반으로 불가사의가 만료되었는지 확인합니다.
    /// </summary>
    private void Update()
    {
        // GameManager.Instance.CurrentYear를 참조하도록 수정
        CheckEventExpirations(GameManager.Instance.CurrentYear);
    }

    /// <summary>
    /// 새로운 불가사의/이벤트 사기 효과를 등록하는 메서드입니다.
    /// </summary>
    /// <param name="moraleEvent">사기 효과 이벤트</param>
    public void RegisterMoraleEffect(MoraleEffectEvent moraleEvent)
    {
        activeEvents.Add(moraleEvent);
        ApplyMoraleEffect(moraleEvent);
    }

    /// <summary>
    /// 현재 이벤트 상태를 확인하고 만료된 효과를 제거합니다.
    /// </summary>
    /// <param name="currentYear">현재 연도</param>
    public void CheckEventExpirations(int currentYear)
    {
        for (int i = activeEvents.Count - 1; i >= 0; i--)
            if (currentYear >= activeEvents[i].endYear)
            {
                RemoveMoraleEffect(activeEvents[i]);
                activeEvents.RemoveAt(i);
            }
    }

    /// <summary>
    /// 사기 증감 효과를 적용하는 메서드입니다.
    /// 종족 또는 전체에 대한 영향을 적용합니다.
    /// </summary>
    /// <param name="effect">적용될 사기 효과</param>
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
    /// 사기 증감 효과를 제거하는 메서드입니다.
    /// 종족 또는 전체에 대한 영향을 제거합니다.
    /// </summary>
    /// <param name="effect">제거할 사기 효과</param>
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
