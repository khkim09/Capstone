using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.IO;

/// <summary>
/// 테스트용 버튼 입력 시 이벤트를 랜덤으로 발생시키는 트리거입니다.
/// </summary>
public class EventTestTrigger : MonoBehaviour
{
    [Header("이벤트 매니저 참조")]
    [SerializeField] private EventManager eventManager;

    [Header("게임 상태 변수 (테스트용)")]
    [SerializeField] private int testYear = 2150;
    [SerializeField] private int testCOMA = 50;
    [SerializeField] private float testFuel = 100f;
    [SerializeField] private bool useRealGameState = true;

    [Header("테스트 옵션")]
    [SerializeField] private bool logEventDetails = true;
    [Tooltip("특정 ID 이벤트만 테스트하려면 값 입력 (0 = 랜덤)")]
    [SerializeField] private int specificEventId = 0;

    [Header("텍스트 검사 설정")]
    [SerializeField] private string exportFilePath = "EventTexts";
    [SerializeField] private bool appendDateToFileName = true;
    [SerializeField] private bool exportAsCSV = true;

    private void Start()
    {
        // EventManager 자동 참조
        if (eventManager == null)
            eventManager = FindObjectOfType<EventManager>();

        if (eventManager == null)
            Debug.LogError("EventManager가 씬에 존재하지 않습니다!");
    }

    /// <summary>
    /// ShipEvent 버튼 클릭 시: 항해 중 발생 가능한 이벤트 중 랜덤 1개 실행
    /// </summary>
    public void TriggerShipEvent()
    {
        if (specificEventId > 0)
        {
            // 특정 ID 이벤트 실행
            RandomEvent specificEvent = eventManager.GetEventById(specificEventId);
            if (specificEvent != null && specificEvent.eventType == EventType.Ship)
            {
                eventManager.TriggerEvent(specificEvent);
                LogEventDetails(specificEvent);
            }
            else
            {
                Debug.LogWarning($"ID {specificEventId}의 Ship 이벤트를 찾을 수 없습니다.");
            }
        }
        else
        {
            // 게임 매니저에서 조건에 맞는 Ship 이벤트 실행
            RandomEvent randomEvent = SelectTestEvent(EventType.Ship);
            if (randomEvent != null)
            {
                eventManager.TriggerEvent(randomEvent);
                LogEventDetails(randomEvent);
            }
            else
            {
                Debug.LogWarning("현재 조건에 맞는 Ship 이벤트가 없습니다.");
            }
        }
    }

    /// <summary>
    /// PlanetEvent 버튼 클릭 시: 행성 도착 시 발생 가능한 이벤트 중 랜덤 1개 실행
    /// </summary>
    public void TriggerPlanetEvent()
    {
        if (specificEventId > 0)
        {
            // 특정 ID 이벤트 실행
            RandomEvent specificEvent = eventManager.GetEventById(specificEventId);
            if (specificEvent != null && specificEvent.eventType == EventType.Planet)
            {
                eventManager.TriggerEvent(specificEvent);
                LogEventDetails(specificEvent);
            }
            else
            {
                Debug.LogWarning($"ID {specificEventId}의 Planet 이벤트를 찾을 수 없습니다.");
            }
        }
        else
        {
            // 게임 매니저에서 조건에 맞는 Planet 이벤트 실행
            RandomEvent randomEvent = SelectTestEvent(EventType.Planet);
            if (randomEvent != null)
            {
                eventManager.TriggerEvent(randomEvent);
                LogEventDetails(randomEvent);
            }
            else
            {
                Debug.LogWarning("현재 조건에 맞는 Planet 이벤트가 없습니다.");
            }
        }
    }

    /// <summary>
    /// CosmicEvent 버튼 클릭 시: 미스터리 이벤트 중 랜덤 1개 실행
    /// </summary>
    public void TriggerCosmicEvent()
    {
        if (specificEventId > 0)
        {
            // 특정 ID 이벤트 실행
            RandomEvent specificEvent = eventManager.GetEventById(specificEventId);
            if (specificEvent != null && specificEvent.eventType == EventType.Mystery)
            {
                eventManager.TriggerEvent(specificEvent);
                LogEventDetails(specificEvent);
            }
            else
            {
                Debug.LogWarning($"ID {specificEventId}의 Mystery 이벤트를 찾을 수 없습니다.");
            }
        }
        else
        {
            // 게임 매니저에서 조건에 맞는 Mystery 이벤트 실행
            RandomEvent randomEvent = SelectTestEvent(EventType.Mystery);
            if (randomEvent != null)
            {
                eventManager.TriggerEvent(randomEvent);
                LogEventDetails(randomEvent);
            }
            else
            {
                Debug.LogWarning("현재 조건에 맞는 Mystery 이벤트가 없습니다.");
            }
        }
    }

    /// <summary>
    /// 모든 이벤트 중 랜덤으로 하나 선택하여 실행합니다.
    /// </summary>
    public void TriggerRandomEvent()
    {
        if (specificEventId > 0)
        {
            // 특정 ID 이벤트 실행
            RandomEvent specificEvent = eventManager.GetEventById(specificEventId);
            if (specificEvent != null)
            {
                eventManager.TriggerEvent(specificEvent);
                LogEventDetails(specificEvent);
            }
            else
            {
                Debug.LogWarning($"ID {specificEventId}의 이벤트를 찾을 수 없습니다.");
            }
            return;
        }

        // 이벤트 타입 랜덤 선택
        EventType[] eventTypes = { EventType.Ship, EventType.Planet, EventType.Mystery };
        EventType randomType = eventTypes[Random.Range(0, eventTypes.Length)];

        // 선택된 타입의 이벤트 실행
        RandomEvent randomEvent = SelectTestEvent(randomType);
        if (randomEvent != null)
        {
            eventManager.TriggerEvent(randomEvent);
            LogEventDetails(randomEvent);
        }
        else
        {
            Debug.LogWarning("현재 조건에 맞는 이벤트가 없습니다.");
        }
    }

    /// <summary>
    /// 테스트를 위한 이벤트 선택 도우미 메서드
    /// </summary>
    private RandomEvent SelectTestEvent(EventType type)
    {
        // 실제 게임 상태 사용 또는 테스트 값 사용
        int year = useRealGameState ? GameManager.Instance.CurrentYear : testYear;
        int coma = useRealGameState ? ResourceManager.Instance.COMA : testCOMA;
        float fuel = useRealGameState ? ResourceManager.Instance.Fuel : testFuel;

        // 사용 가능한 크루 종족 가져오기
        System.Collections.Generic.List<CrewRace> availableRaces;

        if (useRealGameState)
        {
            availableRaces = new System.Collections.Generic.List<CrewRace>();
            var allCrew = GameManager.Instance.GetPlayerShip().GetAllCrew();

            foreach (var crewMember in allCrew)
            {
                if (crewMember.race != CrewRace.None && !availableRaces.Contains(crewMember.race))
                {
                    availableRaces.Add(crewMember.race);
                }
            }
        }
        else
        {
            // 테스트용 기본 종족 설정
            availableRaces = new System.Collections.Generic.List<CrewRace>
            {
                CrewRace.Human,
                CrewRace.Amorphous,
                CrewRace.MechanicSup
            };
        }


            // EventDatabase를 통해 조건에 맞는 이벤트 찾기
            System.Collections.Generic.List<RandomEvent> filteredEvents = EventManager.Instance.ShipEvents;

        if (filteredEvents.Count == 0)
            return null;

        // 랜덤하게 하나 선택
        return filteredEvents[Random.Range(0, filteredEvents.Count)];
    }

    /// <summary>
    /// 이벤트 세부 정보 로깅
    /// </summary>
    private void LogEventDetails(RandomEvent evt)
    {
        if (!logEventDetails)
            return;

        Debug.Log($"===== 테스트 이벤트 발생 =====");
        Debug.Log($"ID: {evt.eventId}, 이름: {evt.debugName}");
        Debug.Log($"타입: {evt.eventType}, 결과 타입: {evt.outcomeType}");
        Debug.Log($"선택지 수: {evt.choices.Count}");
        Debug.Log($"요구 조건: 최소 년도 {evt.minimumYear}, 최소 COMA {evt.minimumCOMA}, 최소 연료 {evt.minimumFuel}");

        if (evt.requiredCrewRace.Count > 0)
        {
            string races = string.Join(", ", evt.requiredCrewRace);
            Debug.Log($"필요 선원 종족: {races}");
        }
        else
        {
            Debug.Log("필요 선원 종족: 없음 (모든 종족 가능)");
        }
    }

    /// <summary>
    /// 모든 이벤트를 콘솔에 출력합니다.
    /// </summary>
    public void PrintAllEvents()
    {
        Debug.Log($"===== 모든 이벤트 목록 ({eventManager.AllEvents.Count}개) =====");

        foreach (var evt in eventManager.AllEvents)
        {
            Debug.Log($"ID: {evt.eventId}, 이름: {evt.debugName}, 타입: {evt.eventType}");
        }
    }

    /// <summary>
    /// 현재 조건에서 발생 가능한 이벤트를 콘솔에 출력합니다.
    /// </summary>
    public void PrintAvailableEvents()
    {
        int year = useRealGameState ? GameManager.Instance.CurrentYear : testYear;
        int coma = useRealGameState ? ResourceManager.Instance.COMA : testCOMA;
        float fuel = useRealGameState ? ResourceManager.Instance.Fuel : testFuel;

        Debug.Log($"===== 현재 조건에서 발생 가능한 이벤트 목록 =====");
        Debug.Log($"기준: 년도 {year}, COMA {coma}, 연료 {fuel}");

        int count = 0;
        foreach (var evt in eventManager.AllEvents)
        {
            if (evt.minimumYear <= year && evt.minimumCOMA <= coma && evt.minimumFuel <= fuel)
            {
                Debug.Log($"ID: {evt.eventId}, 이름: {evt.debugName}, 타입: {evt.eventType}");
                count++;
            }
        }

        Debug.Log($"총 {count}개 이벤트 발생 가능");
    }

     /// <summary>
    /// 모든 이벤트의 텍스트를 검사하고 파일로 내보냅니다.
    /// </summary>
    public void ExportAllEventTexts()
    {
        if (eventManager == null)
        {
            Debug.LogError("EventManager가 null입니다!");
            return;
        }

        var allEvents = eventManager.AllEvents;
        if (allEvents == null || allEvents.Count == 0)
        {
            Debug.LogWarning("이벤트가 없거나 로드되지 않았습니다!");
            return;
        }

        string filePath = exportFilePath;
        if (appendDateToFileName)
        {
            filePath += "_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        }

        filePath = Path.Combine(Application.persistentDataPath, filePath);

        if (exportAsCSV)
        {
            filePath += ".csv";
            ExportEventTextsAsCSV(allEvents, filePath);
        }
        else
        {
            filePath += ".txt";
            ExportEventTextsAsTXT(allEvents, filePath);
        }

        Debug.Log($"이벤트 텍스트 내보내기 완료: {filePath}");

        // 파일 경로를 클립보드에 복사 (선택 사항)
        #if UNITY_EDITOR
        UnityEditor.EditorGUIUtility.systemCopyBuffer = filePath;
        Debug.Log("파일 경로가 클립보드에 복사되었습니다.");
        #endif

        // 파일 탐색기로 열기 (Windows만 해당)
        #if UNITY_EDITOR && UNITY_STANDALONE_WIN
        System.Diagnostics.Process.Start("explorer.exe", "/select," + filePath.Replace('/', '\\'));
        #endif
    }

    /// <summary>
    /// 모든 이벤트 텍스트를 CSV 형식으로 내보냅니다.
    /// </summary>
    private void ExportEventTextsAsCSV(List<RandomEvent> events, string filePath)
    {
        StringBuilder sb = new StringBuilder();

        // CSV 헤더
        sb.AppendLine("EventID,EventType,DebugName,EventTitle,EventDescription,ChoiceIndex,ChoiceText,OutcomeIndex,OutcomeText,Probability");

        foreach (var evt in events)
        {
            if (evt == null) continue;

            string escapedTitle = EscapeCSVField(evt.eventTitle.Localize());
            string escapedDesc = EscapeCSVField(evt.eventDescription.Localize());

            for (int choiceIndex = 0; choiceIndex < evt.choices.Count; choiceIndex++)
            {
                var choice = evt.choices[choiceIndex];
                if (choice == null) continue;

                string escapedChoiceText = EscapeCSVField(choice.choiceText.Localize());

                for (int outcomeIndex = 0; outcomeIndex < choice.possibleOutcomes.Count; outcomeIndex++)
                {
                    var outcome = choice.possibleOutcomes[outcomeIndex];
                    if (outcome == null) continue;

                    string escapedOutcomeText = EscapeCSVField(outcome.outcomeText.Localize());

                    // 한 행 추가
                    sb.AppendLine($"{evt.eventId},{evt.eventType},{evt.debugName},{escapedTitle},{escapedDesc},{choiceIndex+1},{escapedChoiceText},{outcomeIndex+1},{escapedOutcomeText},{outcome.probability}");
                }
            }
        }

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }

    /// <summary>
    /// CSV 필드를 위한 문자열 이스케이프 처리
    /// </summary>
    private string EscapeCSVField(string field)
    {
        if (string.IsNullOrEmpty(field)) return "";

        // 따옴표 내의 따옴표는 두 번 사용하여 이스케이프
        field = field.Replace("\"", "\"\"");

        // 쉼표, 줄바꿈 또는 따옴표가 포함된 경우 전체를 따옴표로 묶음
        if (field.Contains(",") || field.Contains("\n") || field.Contains("\"") || field.Contains("\r"))
        {
            field = $"\"{field}\"";
        }

        return field;
    }

    /// <summary>
    /// 모든 이벤트 텍스트를 일반 텍스트 형식으로 내보냅니다.
    /// </summary>
    private void ExportEventTextsAsTXT(List<RandomEvent> events, string filePath)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("===== 이벤트 텍스트 내보내기 =====");
        sb.AppendLine($"전체 이벤트 수: {events.Count}");
        sb.AppendLine($"내보내기 날짜: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine("================================\n");

        foreach (var evt in events)
        {
            if (evt == null) continue;

            sb.AppendLine($"[이벤트 ID: {evt.eventId}] {evt.debugName} (타입: {evt.eventType})");
            sb.AppendLine($"제목: {evt.eventTitle}");
            sb.AppendLine($"설명: {evt.eventDescription}");

            for (int choiceIndex = 0; choiceIndex < evt.choices.Count; choiceIndex++)
            {
                var choice = evt.choices[choiceIndex];
                if (choice == null) continue;

                sb.AppendLine($"  선택지 {choiceIndex+1}: {choice.choiceText}");

                for (int outcomeIndex = 0; outcomeIndex < choice.possibleOutcomes.Count; outcomeIndex++)
                {
                    var outcome = choice.possibleOutcomes[outcomeIndex];
                    if (outcome == null) continue;

                    sb.AppendLine($"    결과 {outcomeIndex+1} (확률: {outcome.probability}%): {outcome.outcomeText}");
                }
            }

            sb.AppendLine("\n--------------------------------\n");
        }

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }

    /// <summary>
    /// 텍스트 키로 시작하는 이벤트들을 출력합니다.
    /// </summary>
    public void FindEventsByTextKeyPrefix(string prefix)
    {
        if (eventManager == null || eventManager.AllEvents == null) return;

        Debug.Log($"'{prefix}'로 시작하는 텍스트 키를 가진 이벤트 검색 중...");

        int count = 0;
        foreach (var evt in eventManager.AllEvents)
        {
            if (evt == null) continue;

            bool matched = false;

            // 이벤트 제목 확인
            if (evt.eventTitle.StartsWith(prefix))
            {
                Debug.Log($"이벤트 ID {evt.eventId} ({evt.debugName}) - 제목: {evt.eventTitle}");
                matched = true;
            }

            // 이벤트 설명 확인
            if (evt.eventDescription.StartsWith(prefix))
            {
                Debug.Log($"이벤트 ID {evt.eventId} ({evt.debugName}) - 설명: {evt.eventDescription}");
                matched = true;
            }

            // 선택지와 결과 확인
            foreach (var choice in evt.choices)
            {
                if (choice == null) continue;

                if (choice.choiceText.StartsWith(prefix))
                {
                    Debug.Log($"이벤트 ID {evt.eventId} ({evt.debugName}) - 선택지: {choice.choiceText}");
                    matched = true;
                }

                foreach (var outcome in choice.possibleOutcomes)
                {
                    if (outcome == null) continue;

                    if (outcome.outcomeText.StartsWith(prefix))
                    {
                        Debug.Log($"이벤트 ID {evt.eventId} ({evt.debugName}) - 결과: {outcome.outcomeText}");
                        matched = true;
                    }
                }
            }

            if (matched) count++;
        }

        Debug.Log($"검색 완료: {count}개 이벤트 발견");
    }

    /// <summary>
    /// 모든 이벤트의 텍스트 키가 유효한지 검사합니다.
    /// </summary>
    public void ValidateAllEventTextKeys()
    {
        if (eventManager == null || eventManager.AllEvents == null) return;

        Debug.Log("모든 이벤트 텍스트 키 유효성 검사 중...");

        int errorCount = 0;
        foreach (var evt in eventManager.AllEvents)
        {
            if (evt == null) continue;

            int eventId = evt.eventId;

            // 이벤트 제목 검사
            string expectedTitleKey = $"event.name.{eventId}";
            if (evt.eventTitle != expectedTitleKey)
            {
                Debug.LogError($"이벤트 ID {eventId} ({evt.debugName}) 제목 키 오류: 예상 '{expectedTitleKey}', 실제 '{evt.eventTitle}'");
                errorCount++;
            }

            // 이벤트 설명 검사
            string expectedDescKey = $"event.description.{eventId}";
            if (evt.eventDescription != expectedDescKey)
            {
                Debug.LogError($"이벤트 ID {eventId} ({evt.debugName}) 설명 키 오류: 예상 '{expectedDescKey}', 실제 '{evt.eventDescription}'");
                errorCount++;
            }

            // 선택지와 결과 검사
            for (int choiceIndex = 0; choiceIndex < evt.choices.Count; choiceIndex++)
            {
                var choice = evt.choices[choiceIndex];
                if (choice == null) continue;

                string expectedChoiceKey = $"event.choice.{eventId}.{choiceIndex + 1}";
                if (choice.choiceText != expectedChoiceKey)
                {
                    Debug.LogError($"이벤트 ID {eventId} ({evt.debugName}) 선택지 {choiceIndex + 1} 키 오류: 예상 '{expectedChoiceKey}', 실제 '{choice.choiceText}'");
                    errorCount++;
                }

                for (int outcomeIndex = 0; outcomeIndex < choice.possibleOutcomes.Count; outcomeIndex++)
                {
                    var outcome = choice.possibleOutcomes[outcomeIndex];
                    if (outcome == null) continue;

                    string expectedOutcomeKey = $"event.result.{eventId}.{choiceIndex + 1}.{outcomeIndex + 1}";
                    if (outcome.outcomeText != expectedOutcomeKey)
                    {
                        Debug.LogError($"이벤트 ID {eventId} ({evt.debugName}) 선택지 {choiceIndex + 1} 결과 {outcomeIndex + 1} 키 오류: 예상 '{expectedOutcomeKey}', 실제 '{outcome.outcomeText}'");
                        errorCount++;
                    }
                }
            }
        }

        if (errorCount == 0)
        {
            Debug.Log("모든 이벤트 텍스트 키가 유효합니다!");
        }
        else
        {
            Debug.LogWarning($"총 {errorCount}개의 텍스트 키 오류가 발견되었습니다.");
        }
    }
}
