using UnityEngine;

/// <summary>
/// 테스트용 버튼 입력 시 이벤트를 랜덤으로 발생시키는 트리거입니다.
/// </summary>
public class EventTestTrigger : MonoBehaviour
{
    /// <summary>
    /// ShipEvent 버튼 클릭 시: 항해 중 발생 가능한 이벤트 중 랜덤 1개 실행
    /// </summary>
    public void TriggerShipEvent()
    {
        var pool = EventManager.Instance.TimeEvents;
        if (pool.Count > 0)
        {
            var random = pool[Random.Range(0, pool.Count)];
            EventManager.Instance.TriggerEvent(random);
        }
    }

    /// <summary>
    /// PlanetEvent 버튼 클릭 시: 행성 도착 시 발생 가능한 이벤트 중 랜덤 1개 실행
    /// </summary>
    public void TriggerPlanetEvent()
    {
        var pool = EventManager.Instance.LocationEvents;
        if (pool.Count > 0)
        {
            var random = pool[Random.Range(0, pool.Count)];
            EventManager.Instance.TriggerEvent(random);
        }
    }

    /// <summary>
    /// CosmicEvent 버튼 클릭 시: 전체 이벤트 중 랜덤 1개 실행
    /// </summary>
    public void TriggerCosmicEvent()
    {
        var pool = EventManager.Instance.AllEvents;
        if (pool.Count > 0)
        {
            var random = pool[Random.Range(0, pool.Count)];
            EventManager.Instance.TriggerEvent(random);
        }
    }
}
