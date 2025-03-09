using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

// 이벤트 선택지
[Serializable]
public class EventChoice
{
    public string choiceText;
    public List<EventOutcome> possibleOutcomes = new();

    // 확률의 합은 100이 되어야 함
    public EventOutcome GetRandomOutcome()
    {
        var random = Random.Range(0f, 100f);
        var cumulativeProbability = 0f;

        foreach (var outcome in possibleOutcomes)
        {
            cumulativeProbability += outcome.probability;
            if (random <= cumulativeProbability) return outcome;
        }

        // 기본값으로 첫 번째 결과 반환
        return possibleOutcomes.Count > 0 ? possibleOutcomes[0] : null;
    }
}
