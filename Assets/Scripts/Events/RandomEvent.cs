using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이벤트 종류
/// </summary>
[Serializable]
public enum EventType
{
    Ship,
    Planet,
    Mystery
}

/// <summary>
/// 이벤트의 좋고 나쁨을 나타내는 열거형
/// </summary>
public enum EventOutcomeType
{
    Positive,
    Neutral,
    Negative
}

[CreateAssetMenu(fileName = "New Event", menuName = "Game/Random Event")]
public class RandomEvent : ScriptableObject
{
    public int eventId;
    public string debugName;
    public string eventTitle;
    public string eventDescription;
    public Sprite eventImage;
    public EventType eventType;
    public EventOutcomeType outcomeType;
    public int minimumYear;

    public List<EventChoice> choices = new();
}
