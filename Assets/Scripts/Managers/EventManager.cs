using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 내 랜덤 이벤트를 관리하고 처리하는 매니저.
/// 위치 이동, 시간 경과에 따라 이벤트를 트리거하며 선택지 및 결과 처리까지 담당합니다.
/// </summary>
public class EventManager : MonoBehaviour
{
    /// <summary>
    /// 전체 이벤트 리스트입니다.
    /// </summary>
    [SerializeField] private List<RandomEvent> allEvents = new();

    /// <summary>
    /// 위치 이동 시 발생할 수 있는 이벤트 리스트입니다.
    /// </summary>
    [SerializeField] private List<RandomEvent> locationEvents = new();

    /// <summary>
    /// 시간 경과 시 발생할 수 있는 이벤트 리스트입니다.
    /// </summary>
    [SerializeField] private List<RandomEvent> timeEvents = new();

    /// <summary>
    /// 현재 발생 중인 이벤트입니다.
    /// </summary>
    private RandomEvent currentEvent;

    /// <summary>
    /// 특수 효과 처리 핸들러 팩토리입니다.
    /// </summary>
    private SpecialEffectHandlerFactory effectHandlerFactory;

    /// <summary>
    /// 싱글턴 인스턴스입니다.
    /// </summary>
    public static EventManager Instance { get; private set; }

    private bool isEventRunning = false;

    /// <summary>
    /// 프로퍼티를 추가해, 왜부에서 이벤트 목록을 받아옵니다.
    /// </summary>
    public IReadOnlyList<RandomEvent> TimeEvents => timeEvents;
    public IReadOnlyList<RandomEvent> LocationEvents => locationEvents;
    public IReadOnlyList<RandomEvent> AllEvents => allEvents;

    /// <summary>
    /// 인스턴스를 초기화합니다. 중복 객체는 제거됩니다.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 게임 시작 시 이벤트 로드 및 특수 효과 핸들러 초기화를 수행합니다.
    /// </summary>
    private void Start()
    {
        // 모든 이벤트 로드
        LoadEvents();

        // 특수 효과 처리기 팩토리 초기화
        InitializeEffectHandlers();
    }

    /// <summary>
    /// 에디터 또는 리소스에서 이벤트들을 불러옵니다.
    /// 실제 구현에서는 Resources나 AddressableAssets 사용 권장.
    /// </summary>
    private void LoadEvents()
    {
        // 에디터에서 설정한 이벤트들 또는 리소스에서 로드
        // 실제 구현에서는 Resources.LoadAll 또는 AddressableAssets 사용
    }

    /// <summary>
    /// 특수 효과 핸들러 팩토리를 초기화합니다.
    /// </summary>
    private void InitializeEffectHandlers()
    {
        effectHandlerFactory = new SpecialEffectHandlerFactory();
    }


    /// <summary>
    /// 위치 이동 시 랜덤 이벤트를 발생시킵니다.
    /// </summary>
    public void TriggerLocationEvent()
    {
        RandomEvent randomEvent = GetRandomEvent(true);
        if (randomEvent != null) TriggerEvent(randomEvent);
    }


    /// <summary>
    /// 시간 경과(일 단위) 시 랜덤 이벤트를 발생시킵니다.
    /// </summary>
    public void TriggerDailyEvent()
    {
        RandomEvent randomEvent = GetRandomEvent(false);
        if (randomEvent != null) TriggerEvent(randomEvent);
    }

    /// <summary>
    /// 이벤트 풀에서 랜덤 이벤트를 선택합니다.
    /// </summary>
    /// <param name="isLocation">위치 이벤트 여부 (true면 위치 이벤트).</param>
    /// <returns>선택된 랜덤 이벤트.</returns>
    public RandomEvent GetRandomEvent(bool isLocation)
    {
        List<RandomEvent> eventPool = isLocation ? locationEvents : timeEvents;

        if (eventPool.Count == 0)
            return null;

        int randomIndex = Random.Range(0, eventPool.Count);
        return eventPool[randomIndex];
    }

    /// <summary>
    /// 지정된 이벤트를 트리거하여 UI에 표시하고 게임 상태를 변경합니다.
    /// </summary>
    /// <param name="randomEvent">발생시킬 이벤트.</param>
    public void TriggerEvent(RandomEvent randomEvent)
    {
        if (isEventRunning)
        {
            Debug.Log("이벤트 중복 실행 방지됨");
            return;
        }

        isEventRunning = true;
        currentEvent = randomEvent;

        // 게임 상태 변경
        GameManager.Instance.ChangeGameState(GameState.Event);

        // UI에 이벤트 표시
        EventUIManager.Instance.ShowEvent(randomEvent);
    }

    /// <summary>
    /// 이벤트에서 선택지를 고르면 해당 선택지의 결과를 처리합니다.
    /// 자원 및 특수 효과가 적용됩니다.
    /// </summary>
    /// <param name="currentEvent">현재 이벤트.</param>
    /// <param name="choiceIndex">선택한 선택지 인덱스.</param>
    public void ProcessChoice(RandomEvent currentEvent, int choiceIndex)
    {
        if (choiceIndex < 0 || choiceIndex >= currentEvent.choices.Count)
            return;

        EventChoice choice = currentEvent.choices[choiceIndex];
        EventOutcome outcome = choice.GetRandomOutcome();

        if (outcome != null)
        {
            // 결과 텍스트 표시
            EventUIManager.Instance.ShowOutcome(outcome.outcomeText);

            // 자원 효과 적용
            foreach (ResourceEffect effect in outcome.resourceEffects)
                ResourceManager.Instance.ChangeResource(effect.resourceType, effect.amount);

            // 특수 효과 처리
            ProcessSpecialEffect(outcome);
        }
    }

    /// <summary>
    /// 선택지 결과에 포함된 특수 효과를 처리합니다.
    /// </summary>
    /// <param name="outcome">이벤트 결과 정보.</param>
    private void ProcessSpecialEffect(EventOutcome outcome)
    {
        ISpecialEffectHandler handler = effectHandlerFactory.GetHandler(outcome.specialEffectType);
        handler?.HandleEffect(outcome);
    }

    /// <summary>
    /// 이벤트 실행을 종료합니다.
    /// </summary>
    public void EndEvent()
    {
        isEventRunning = false;
        GameManager.Instance.OnEventCompleted();
    }
}
