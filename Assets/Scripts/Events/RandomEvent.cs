using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Event", menuName = "Game/Random Event")]
public class RandomEvent : ScriptableObject
{
    public string eventId;
    public string eventTitle;
    public string eventDescription;
    public Sprite eventImage;

    public List<EventChoice> choices = new();
}
