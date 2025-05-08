using System.Collections.Generic;
using UnityEngine;

public class EventDatabase : ScriptableObject
{
    public List<RandomEvent> randomEvents;

    private Dictionary<int, RandomEvent> eventDictionary;

    public void InitializeDictionary()
    {
        eventDictionary = new Dictionary<int, RandomEvent>();
        // foreach (RandomEvent randomEvent in randomEvents) eventDictionary[randomEvent.eventId] = randomEvent;
    }
}
