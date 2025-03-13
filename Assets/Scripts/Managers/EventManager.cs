using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    [SerializeField] private List<RandomEvent> allEvents = new();
    [SerializeField] private List<RandomEvent> locationEvents = new();
    [SerializeField] private List<RandomEvent> timeEvents = new();
    private RandomEvent currentEvent;

    private SpecialEffectHandlerFactory effectHandlerFactory;
    public static EventManager Instance { get; private set; }

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

    private void Start()
    {
        // 모든 이벤트 로드
        LoadEvents();

        // 특수 효과 처리기 팩토리 초기화
        InitializeEffectHandlers();
    }

    private void LoadEvents()
    {
        // 에디터에서 설정한 이벤트들 또는 리소스에서 로드
        // 실제 구현에서는 Resources.LoadAll 또는 AddressableAssets 사용
    }

    private void InitializeEffectHandlers()
    {
        effectHandlerFactory = new SpecialEffectHandlerFactory();
    }

    // 위치 이동 시 이벤트 발생
    public void TriggerLocationEvent()
    {
        RandomEvent randomEvent = GetRandomEvent(true);
        if (randomEvent != null) TriggerEvent(randomEvent);
    }

    // 시간 경과 시 이벤트 발생
    public void TriggerDailyEvent()
    {
        RandomEvent randomEvent = GetRandomEvent(false);
        if (randomEvent != null) TriggerEvent(randomEvent);
    }

    // 이벤트 목록에서 랜덤 이벤트 선택
    public RandomEvent GetRandomEvent(bool isLocation)
    {
        List<RandomEvent> eventPool = isLocation ? locationEvents : timeEvents;

        if (eventPool.Count == 0)
            return null;

        int randomIndex = Random.Range(0, eventPool.Count);
        return eventPool[randomIndex];
    }

    // 이벤트 처리
    public void TriggerEvent(RandomEvent randomEvent)
    {
        currentEvent = randomEvent;

        // 게임 상태 변경
        GameManager.Instance.ChangeGameState(GameState.Event);

        // UI에 이벤트 표시
        EventUIManager.Instance.ShowEvent(randomEvent);
    }

    // 선택지 처리
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

            // 승무원 효과 적용
            foreach (CrewEffect effect in outcome.crewEffects) CrewManager.Instance.ApplyCrewEffect(effect);

            // 특수 효과 처리
            ProcessSpecialEffect(outcome);
        }
    }

    private void ProcessSpecialEffect(EventOutcome outcome)
    {
        ISpecialEffectHandler handler = effectHandlerFactory.GetHandler(outcome.specialEffectType);
        handler?.HandleEffect(outcome);
    }
}
