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

[CreateAssetMenu(fileName = "New Event", menuName = "Event/Random Event")]
public class RandomEvent : ScriptableObject
{
    public int eventId;
    public string debugName;
    public string eventTitle;
    public string eventDescription;
    public Sprite eventImage;
    public EventType eventType;
    public EventOutcomeType outcomeType;

    #region 퀘스트 출현 조건

    /// <summary>
    /// 퀘스트가 출현하는 최소 년도
    /// </summary>
    public int minimumYear = 0;

    /// <summary>
    /// 퀘스트가 출현하는 데 필요한 종족 종류
    /// </summary>
    public List<CrewRace> requiredCrewRace = new();

    /// <summary>
    /// 퀘스트가 출현하는 데 필요한 최소 COMA
    /// </summary>
    public int minimumCOMA = 0;

    #endregion

    public List<EventChoice> choices = new();
}
