using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 모든 이벤트 데이터를 관리하는 중앙 데이터베이스
/// </summary>
[CreateAssetMenu(fileName = "EventDatabase", menuName = "Event/Event Database")]
public class EventDatabase : ScriptableObject
{
    /// <summary>
    /// 모든 이벤트 데이터 목록
    /// </summary>
    [SerializeField] private List<RandomEvent> allEvents = new();

    // 이벤트 검색용 딕셔너리 (런타임에만 사용)
    private Dictionary<int, RandomEvent> eventDictionary;

    // 이벤트 타입별 목록 (런타임에만 사용)
    private Dictionary<EventType, List<RandomEvent>> eventsByType;

    // 이벤트 결과 타입별 목록 (런타임에만 사용)
    private Dictionary<EventOutcomeType, List<RandomEvent>> eventsByOutcomeType;

    /// <summary>
    /// 데이터베이스를 초기화합니다.
    /// </summary>
    public void Initialize()
    {
        InitializeEventDictionary();
        InitializeEventsByType();
        InitializeEventsByOutcomeType();
        Debug.Log($"EventDatabase 초기화 완료: {allEvents.Count}개 이벤트 로드");
    }

    /// <summary>
    /// 이벤트 ID 기반 딕셔너리를 초기화합니다.
    /// </summary>
    private void InitializeEventDictionary()
    {
        eventDictionary = new Dictionary<int, RandomEvent>();
        foreach (RandomEvent evt in allEvents)
            if (evt != null)
            {
                if (eventDictionary.ContainsKey(evt.eventId))
                {
                    Debug.LogWarning($"중복된 이벤트 ID: {evt.eventId}, 이벤트 이름: {evt.debugName}");
                    continue;
                }

                eventDictionary[evt.eventId] = evt;
            }
    }

    /// <summary>
    /// 이벤트 타입별 목록을 초기화합니다.
    /// </summary>
    private void InitializeEventsByType()
    {
        eventsByType = new Dictionary<EventType, List<RandomEvent>>();

        // 모든 EventType에 대해 빈 리스트 생성
        foreach (EventType type in System.Enum.GetValues(typeof(EventType)))
            eventsByType[type] = new List<RandomEvent>();

        // 이벤트 분류
        foreach (RandomEvent evt in allEvents)
            if (evt != null)
                eventsByType[evt.eventType].Add(evt);
    }

    /// <summary>
    /// 이벤트 결과 타입별 목록을 초기화합니다.
    /// </summary>
    private void InitializeEventsByOutcomeType()
    {
        eventsByOutcomeType = new Dictionary<EventOutcomeType, List<RandomEvent>>();

        // 모든 EventOutcomeType에 대해 빈 리스트 생성
        foreach (EventOutcomeType type in System.Enum.GetValues(typeof(EventOutcomeType)))
            eventsByOutcomeType[type] = new List<RandomEvent>();

        // 이벤트 분류
        foreach (RandomEvent evt in allEvents)
            if (evt != null)
                eventsByOutcomeType[evt.outcomeType].Add(evt);
    }

    /// <summary>
    /// Unity 이벤트: ScriptableObject가 로드될 때 호출됨
    /// </summary>
    private void OnEnable()
    {
        Initialize();
    }

    /// <summary>
    /// 모든 이벤트 목록을 반환합니다.
    /// </summary>
    public List<RandomEvent> GetAllEvents()
    {
        return new List<RandomEvent>(allEvents);
    }

    /// <summary>
    /// ID로 이벤트를 검색합니다.
    /// </summary>
    /// <param name="id">이벤트 ID</param>
    /// <returns>이벤트 또는 null</returns>
    public RandomEvent GetEvent(int id)
    {
        if (eventDictionary == null)
            Initialize();

        if (eventDictionary.TryGetValue(id, out RandomEvent evt))
            return evt;

        Debug.LogWarning($"ID가 {id}인 이벤트를 찾을 수 없습니다.");
        return null;
    }

    /// <summary>
    /// 디버그 이름으로 이벤트를 검색합니다.
    /// </summary>
    /// <param name="debugName">이벤트 디버그 이름</param>
    /// <returns>이벤트 또는 null</returns>
    public RandomEvent GetEventByDebugName(string debugName)
    {
        if (string.IsNullOrEmpty(debugName))
            return null;

        foreach (RandomEvent evt in allEvents)
            if (evt != null && evt.debugName == debugName)
                return evt;

        Debug.LogWarning($"디버그 이름이 '{debugName}'인 이벤트를 찾을 수 없습니다.");
        return null;
    }

    /// <summary>
    /// 특정 이벤트 타입의 모든 이벤트를 반환합니다.
    /// </summary>
    /// <param name="type">이벤트 타입</param>
    /// <returns>해당 타입의 이벤트 목록</returns>
    public List<RandomEvent> GetEventsByType(EventType type)
    {
        if (eventsByType == null)
            Initialize();

        if (eventsByType.TryGetValue(type, out List<RandomEvent> events))
            return new List<RandomEvent>(events);

        return new List<RandomEvent>();
    }

    /// <summary>
    /// 특정 결과 타입의 모든 이벤트를 반환합니다.
    /// </summary>
    /// <param name="outcomeType">이벤트 결과 타입</param>
    /// <returns>해당 결과 타입의 이벤트 목록</returns>
    public List<RandomEvent> GetEventsByOutcomeType(EventOutcomeType outcomeType)
    {
        if (eventsByOutcomeType == null)
            Initialize();

        if (eventsByOutcomeType.TryGetValue(outcomeType, out List<RandomEvent> events))
            return new List<RandomEvent>(events);

        return new List<RandomEvent>();
    }

    /// <summary>
    /// 최소 발생 년도 이상의 이벤트를 반환합니다.
    /// </summary>
    /// <param name="year">현재 게임 년도</param>
    /// <returns>발생 가능한 이벤트 목록</returns>
    public List<RandomEvent> GetEventsForYear(int year)
    {
        return allEvents.Where(evt => evt != null && evt.minimumYear <= year).ToList();
    }

    /// <summary>
    /// 최소 COMA 이상의 이벤트를 반환합니다.
    /// </summary>
    /// <param name="coma">현재 COMA 값</param>
    /// <returns>발생 가능한 이벤트 목록</returns>
    public List<RandomEvent> GetEventsForCOMA(int coma)
    {
        return allEvents.Where(evt => evt != null && evt.minimumCOMA <= coma).ToList();
    }

    /// <summary>
    /// 특정 선원 종족이 필요한 이벤트를 반환합니다.
    /// </summary>
    /// <param name="race">선원 종족</param>
    /// <returns>해당 종족이 필요한 이벤트 목록</returns>
    public List<RandomEvent> GetEventsRequiringRace(CrewRace race)
    {
        return allEvents.Where(evt => evt != null && evt.requiredCrewRace.Contains(race)).ToList();
    }

    /// <summary>
    /// 특정 조건에 맞는 이벤트를 필터링하여 반환합니다.
    /// </summary>
    /// <param name="type">이벤트 타입</param>
    /// <param name="year">현재 년도</param>
    /// <param name="coma">현재 COMA</param>
    /// <param name="availableRaces">보유 중인 선원 종족 목록</param>
    /// <returns>조건에 맞는 이벤트 목록</returns>
    public List<RandomEvent> GetFilteredEvents(EventType type, int year, int coma, float fuel,
        List<CrewRace> availableRaces)
    {
        // 기본 필터: 타입 + 년도 + COMA
        List<RandomEvent> filteredEvents = allEvents.Where(evt =>
            evt != null &&
            evt.eventType == type &&
            evt.minimumYear <= year &&
            evt.minimumCOMA <= coma
        ).ToList();

        // 선원 종족 필터 적용
        if (availableRaces != null && availableRaces.Count > 0)
            filteredEvents = filteredEvents.Where(evt =>
                // 필요한 종족이 없거나 (모든 종족 가능)
                evt.requiredCrewRace.Count == 0 ||
                // 필요한 모든 종족을 보유
                evt.requiredCrewRace.All(required => availableRaces.Contains(required))
            ).ToList();

        return filteredEvents;
    }

    /// <summary>
    /// 주어진 조건에서 발생 가능한 랜덤 이벤트를 선택합니다.
    /// </summary>
    /// <param name="type">이벤트 타입</param>
    /// <param name="year">현재 년도</param>
    /// <param name="coma">현재 COMA</param>
    /// <param name="availableRaces">보유 중인 선원 종족 목록</param>
    /// <returns>선택된 랜덤 이벤트 또는 null</returns>
    public RandomEvent GetRandomEvent(EventType type, int year, int coma, float fuel, List<CrewRace> availableRaces)
    {
        List<RandomEvent> filteredEvents = GetFilteredEvents(type, year, coma, fuel, availableRaces);

        if (filteredEvents.Count == 0)
            return null;

        // 랜덤하게 하나 선택
        int randomIndex = Random.Range(0, filteredEvents.Count);
        return filteredEvents[randomIndex];
    }


    /// <summary>
    /// 데이터베이스를 초기화하고 모든 이벤트를 다시 로드합니다.
    /// </summary>
    public void ReloadEvents()
    {
        Initialize();
    }
}
