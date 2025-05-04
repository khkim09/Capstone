using UnityEngine;

public class EventTestTrigger : MonoBehaviour
{
    public RandomEvent shipEvent;
    public RandomEvent planetEvent;
    public RandomEvent cosmicWonder;

    public void TriggerShipEvent() => EventManager.Instance.TriggerEvent(shipEvent);
    public void TriggerPlanetEvent() => EventManager.Instance.TriggerEvent(planetEvent);
    public void TriggerCosmicEvent() => EventManager.Instance.TriggerEvent(cosmicWonder);
}
