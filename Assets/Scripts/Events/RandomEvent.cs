using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이벤트 종류
/// </summary>
public enum EventType
{
    ShipEvent,
    PlanetEvent,
    CosmicWonder
}

[CreateAssetMenu(fileName = "New Event", menuName = "Game/Random Event")]
public class RandomEvent : ScriptableObject
{
    public string eventId;
    public string eventTitle;
    public string eventDescription;
    public Sprite eventImage;
    public EventType eventType;

    public List<EventChoice> choices = new();
}
