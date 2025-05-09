using UnityEngine;

/// <summary>
/// 다음 이벤트를 트리거하는 특수 효과 핸들러입니다.
/// </summary>
public class EventNextEventEffectHandler : ISpecialEffectHandler
{
    /// <summary>
    /// outcome에 연결된 nextEvent가 존재하면 즉시 실행합니다.
    /// </summary>
    /// <param name="outcome">이벤트 결과 정보</param>
    public void HandleEffect(EventOutcome outcome)
    {
        if (outcome == null)
        {
            Debug.LogWarning("[NextEventHandler] outcome이 null입니다.");
            return;
        }

        if (outcome.nextEvent == null)
        {
            Debug.LogWarning("[NextEventHandler] nextEvent가 설정되어 있지 않습니다.");
            return;
        }

        Debug.Log($"[NextEventHandler] 다음 이벤트 실행: {outcome.nextEvent.eventTitle}");
        EventManager.Instance.TriggerEvent(outcome.nextEvent);
    }
}
