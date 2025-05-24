using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 내 이벤트 시스템을 관리하는 매니저 클래스
/// </summary>
public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    [Header("데이터베이스 참조")] [SerializeField] private EventDatabase eventDatabase;

    public EventPanelController EventPanelController { get; set; }


    // 현재 진행 중인 이벤트
    private RandomEvent currentEvent;

    // 효과 핸들러들
    private EventResourceEffectHandler resourceEffectHandler;
    private EventCrewEffectHandler crewEffectHandler;
    private EventPlanetEffectHandler planetEffectHandler;
    private SpecialEffectHandlerFactory specialEffectHandlerFactory;

    // 이벤트 히스토리
    private List<int> recentEventIds = new();

    private Queue<RandomEvent> pendingEvents = new();
    private bool isProcessingEvent = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        // 핸들러 초기화
        resourceEffectHandler = new EventResourceEffectHandler();
        crewEffectHandler = new EventCrewEffectHandler();
        planetEffectHandler = new EventPlanetEffectHandler();
        specialEffectHandlerFactory = new SpecialEffectHandlerFactory();

        GameManager.Instance.OnYearChanged += TryTriggerPlanetEvent;
        GameManager.Instance.OnYearChanged += TryTriggerShipEvent;
    }


    private void Start()
    {
        if (eventDatabase == null)
        {
            Debug.LogError("EventDatabase가 할당되지 않았습니다!");
            return;
        }

        eventDatabase.Initialize();
    }

    /// <summary>
    /// 모든 이벤트 목록을 반환합니다.
    /// </summary>
    public List<RandomEvent> AllEvents => eventDatabase.GetAllEvents();

    /// <summary>
    /// 함선 이벤트 목록을 반환합니다.
    /// </summary>
    public List<RandomEvent> ShipEvents => eventDatabase.GetEventsByType(EventType.Ship);

    /// <summary>
    /// 행성 이벤트 목록을 반환합니다.
    /// </summary>
    public List<RandomEvent> PlanetEvents => eventDatabase.GetEventsByType(EventType.Planet);

    /// <summary>
    /// 미스터리 이벤트 목록을 반환합니다.
    /// </summary>
    public List<RandomEvent> MysteryEvents => eventDatabase.GetEventsByType(EventType.Mystery);

    /// <summary>
    /// 이벤트 ID로 이벤트를 검색합니다.
    /// </summary>
    public RandomEvent GetEventById(int id)
    {
        return eventDatabase.GetEvent(id);
    }

    public void TryTriggerShipEvent(int year)
    {
        StartCoroutine(TryTriggerShipEventCoroutine());
    }

    private IEnumerator TryTriggerShipEventCoroutine()
    {
        yield return new WaitForSeconds(Constants.WarpNodes.WarpingDuration);


        if (GameManager.Instance.WarpNodeDataList[GameManager.Instance.CurrentWarpNodeId].nodeType !=
            WarpNodeType.Event)
            yield break;

        RandomEvent evt;
        if (Random.value <= Constants.Events.ShipEventChance)
            evt = SelectAppropriateEvent(EventType.Ship);
        else
            evt = SelectAppropriateEvent(EventType.Mystery);

        if (evt != null) TriggerEvent(evt);
    }


    /// <summary>
    /// 워프 시 이벤트 발생 여부를 체크하고 이벤트를 실행합니다.
    /// </summary>
    /// <returns>이벤트가 발생했는지 여부</returns>
    public void TryTriggerPlanetEvent(int year)
    {
        StartCoroutine(TryTriggerPlanetEventCoroutine());
    }

    private IEnumerator TryTriggerPlanetEventCoroutine()
    {
        yield return new WaitForSeconds(Constants.WarpNodes.WarpingDuration);

        if (GameManager.Instance.PlanetDataList.Where(d => d.activeEffects.Count == 0).ToList().Count == 0) yield break;
        if (CurrentWarpNode.NodeData.nodeType != WarpNodeType.Event)
            yield break;

        if (Random.value <= Constants.Events.PlanetEventChance)
        {
            RandomEvent evt = SelectAppropriateEvent(EventType.Planet);
            if (evt != null) TriggerEvent(evt);
        }
    }

    public WarpNode CurrentWarpNode;

    public void CombatOccur()
    {
        if (CurrentWarpNode.NodeData.nodeType == WarpNodeType.Combat)
        {
            GameManager.Instance.ChangeGameState(GameState.Combat);
            SceneChanger.Instance.LoadScene("Combat");
        }
    }

    /// <summary>
    /// 이벤트를 트리거합니다. 이미 진행 중인 이벤트가 있다면 큐에 추가합니다.
    /// </summary>
    public void TriggerEvent(RandomEvent randomEvent)
    {
        if (randomEvent == null)
            return;

        // 최근 이벤트 목록에 추가
        AddToRecentEvents(randomEvent.eventId);

        // 큐에 이벤트 추가
        pendingEvents.Enqueue(randomEvent);
        Debug.Log($"이벤트 큐에 추가: {randomEvent.debugName} (ID: {randomEvent.eventId})");

        // 이벤트 처리가 진행 중이 아니라면 처리 시작
        if (!isProcessingEvent) ProcessNextEvent();
    }

    /// <summary>
    /// 큐에서 다음 이벤트를 처리합니다.
    /// </summary>
    private void ProcessNextEvent()
    {
        if (pendingEvents.Count == 0)
        {
            isProcessingEvent = false;
            return;
        }

        isProcessingEvent = true;
        currentEvent = pendingEvents.Dequeue();
        EventPanelController.ShowEvent(currentEvent);

        Debug.Log(
            "현재 노드 타입 : " + GameManager.Instance.WarpNodeDataList[GameManager.Instance.CurrentWarpNodeId].nodeType);
        Debug.Log("현재 노드 Index : " + GameManager.Instance.CurrentWarpNodeId);
        Debug.Log($"이벤트 실행: {currentEvent.debugName} (ID: {currentEvent.eventId})");
    }


    /// <summary>
    /// 특정 ID의 이벤트를 강제로 실행합니다.
    /// </summary>
    /// <param name="eventId">실행할 이벤트 ID</param>
    public void TriggerEventById(int eventId)
    {
        RandomEvent evt = eventDatabase.GetEvent(eventId);
        if (evt != null)
            TriggerEvent(evt);
        else
            Debug.LogWarning($"ID가 {eventId}인 이벤트를 찾을 수 없습니다.");
    }

    /// <summary>
    /// 선택지 선택 시 처리합니다.
    /// </summary>
    /// <param name="randomEvent">현재 이벤트</param>
    /// <param name="choiceIndex">선택한 선택지 인덱스</param>
    public void ProcessChoice(RandomEvent randomEvent, int choiceIndex)
    {
        if (randomEvent == null || choiceIndex < 0 || choiceIndex >= randomEvent.choices.Count)
            return;

        // 선택지와 결과 가져오기
        EventChoice choice = randomEvent.choices[choiceIndex];
        EventOutcome outcome = choice.GetRandomOutcome();

        // 각종 효과 적용
        ApplyOutcomeEffects(outcome);

        // 미스터리 이벤트 완료 기록
        if (randomEvent.eventType == EventType.Mystery)
        {
            GameManager.Instance.AddCompletedMysteryEvent(randomEvent.eventId);
            Debug.Log($"미스터리 이벤트 완료 기록: {randomEvent.debugName} (ID: {randomEvent.eventId})");
        }


        if (outcome == null)
            return;

        if (randomEvent.eventType == EventType.Planet)
        {
            string planetName = GameManager.Instance.PlanetDataList[randomEvent.planetId].planetName;

            EventPanelController
                .ShowOutcome(outcome.outcomeText.Localize(planetName));
        }
        else
        {
            EventPanelController.ShowOutcome(outcome.outcomeText.Localize());
        }


        Debug.Log(
            $"이벤트 선택: {randomEvent.debugName}, 선택지: {choiceIndex + 1}, 결과: {outcome.outcomeText.Substring(0, Mathf.Min(30, outcome.outcomeText.Length))}...");
    }

    /// <summary>
    /// 결과 효과를 적용합니다.
    /// </summary>
    /// <param name="outcome">적용할 결과</param>
    private void ApplyOutcomeEffects(EventOutcome outcome)
    {
        // 1. 자원 효과 적용
        if (outcome.resourceEffects.Count > 0)
        {
            resourceEffectHandler.ApplyEffects(outcome.resourceEffects);
            Debug.Log($"자원 효과 적용: {outcome.resourceEffects.Count}개");
        }

        // 2. 승무원 효과 적용
        if (outcome.crewEffects.Count > 0)
        {
            crewEffectHandler.ApplyEffects(outcome.crewEffects);
            Debug.Log($"승무원 효과 적용: {outcome.crewEffects.Count}개");
        }

        // 3. 행성 효과 적용
        if (outcome.planetEffects.Count > 0)
        {
            planetEffectHandler.ApplyEffects(outcome.planetEffects, currentEvent.eventTitle);
            Debug.Log($"행성 효과 적용: {outcome.planetEffects.Count}개");
        }

        // 4. 특수 효과 적용
        if (outcome.specialEffectType != SpecialEffectType.None)
        {
            ISpecialEffectHandler handler = specialEffectHandlerFactory.GetHandler(outcome.specialEffectType);
            if (handler != null)
            {
                handler.HandleEffect(outcome);
                Debug.Log($"특수 효과 적용: {outcome.specialEffectType}");
            }
            else
            {
                Debug.LogWarning($"특수 효과 핸들러를 찾을 수 없음: {outcome.specialEffectType}");
            }
        }
    }

    /// <summary>
    /// 이벤트 종료 처리를 수행합니다.
    /// </summary>
    public void EndEvent()
    {
        currentEvent = null;
        Debug.Log("이벤트 종료");

        // 다음 대기 중인 이벤트 처리
        ProcessNextEvent();
    }

    /// <summary>
    /// 대기 중인 모든 이벤트를 취소합니다.
    /// </summary>
    public void ClearPendingEvents()
    {
        pendingEvents.Clear();
        if (!isProcessingEvent)
            isProcessingEvent = false;
    }

    /// <summary>
    /// 현재 게임 상태에 적합한 이벤트를 선택합니다.
    /// </summary>
    /// <param name="type">이벤트 타입</param>
    /// <returns>선택된 이벤트 또는 null</returns>
    private RandomEvent SelectAppropriateEvent(EventType type)
    {
        // 현재 게임 상태 정보 가져오기
        int currentYear = GameManager.Instance.CurrentYear;
        int currentCOMA = ResourceManager.Instance.COMA;
        float currentFuel = ResourceManager.Instance.Fuel;
        List<CrewRace> availableRaces = GetAvailableCrewRaces();

        // 기계형 선원 처리 - 한 종류만 있어도 두 종류 모두 있는 것으로 취급
        bool hasMechanicType = availableRaces.Contains(CrewRace.MechanicTank) ||
                               availableRaces.Contains(CrewRace.MechanicSup);
        if (hasMechanicType)
        {
            // 둘 다 없는 경우에만 추가
            if (!availableRaces.Contains(CrewRace.MechanicTank))
                availableRaces.Add(CrewRace.MechanicTank);

            if (!availableRaces.Contains(CrewRace.MechanicSup))
                availableRaces.Add(CrewRace.MechanicSup);
        }

        // 조건에 맞는 이벤트 필터링
        List<RandomEvent> filteredEvents =
            eventDatabase.GetFilteredEvents(type, currentYear, currentCOMA, currentFuel, availableRaces);

        // 최근에 발생한 이벤트 제외
        filteredEvents = filteredEvents.Where(evt => !recentEventIds.Contains(evt.eventId)).ToList();

        // 미스터리 이벤트의 경우 이미 완료된 이벤트 제외
        if (type == EventType.Mystery)
        {
            List<int> completedMysteryIds = GameManager.Instance.CompletedMysteryEventIds;
            filteredEvents = filteredEvents.Where(evt => !completedMysteryIds.Contains(evt.eventId)).ToList();
            Debug.Log(
                $"미스터리 이벤트 필터링: 전체 {eventDatabase.GetEventsByType(EventType.Mystery).Count}개 중 완료된 {completedMysteryIds.Count}개 제외, 사용 가능한 이벤트 {filteredEvents.Count}개");
        }

        if (filteredEvents.Count == 0)
        {
            Debug.LogWarning($"현재 조건에 맞는 {type} 이벤트가 없습니다.");
            return null;
        }

        // 랜덤하게 하나 선택
        int randomIndex = Random.Range(0, filteredEvents.Count);
        int planetId = 0;
        if (type == EventType.Planet)
        {
            List<PlanetData> planetDatas =
                GameManager.Instance.PlanetDataList.Where(p => p.activeEffects.Count == 0).ToList();
            int index = Random.Range(0, planetDatas.Count);
            planetId = planetDatas[index].planetId;
        }

        filteredEvents[randomIndex].planetId = planetId;

        return filteredEvents[randomIndex];
    }

    /// <summary>
    /// 현재 보유 중인 선원 종족 목록을 가져옵니다.
    /// </summary>
    /// <returns>보유 중인 선원 종족 목록</returns>
    private List<CrewRace> GetAvailableCrewRaces()
    {
        List<CrewMember> crew = GameManager.Instance.GetPlayerShip().GetAllCrew();

        // 중복 제거한 종족 목록
        HashSet<CrewRace> races = new();
        foreach (CrewMember member in crew)
            // None은 제외
            if (member.race != CrewRace.None)
                races.Add(member.race);

        return races.ToList();
    }

    /// <summary>
    /// 최근 발생한 이벤트 목록에 이벤트를 추가합니다.
    /// </summary>
    /// <param name="eventId">추가할 이벤트 ID</param>
    private void AddToRecentEvents(int eventId)
    {
        // 이미 목록에 있으면 제거 (최신 기록으로 갱신하기 위해)
        recentEventIds.Remove(eventId);

        // 목록 앞에 추가
        recentEventIds.Insert(0, eventId);

        // 최대 기록 크기 유지
        if (recentEventIds.Count > Constants.Events.RecentEventHistoryCount)
            recentEventIds.RemoveAt(recentEventIds.Count - 1);
    }

    /// <summary>
    /// 최근 발생한 이벤트 목록을 초기화합니다.
    /// </summary>
    public void ClearRecentEvents()
    {
        recentEventIds.Clear();
    }
}
