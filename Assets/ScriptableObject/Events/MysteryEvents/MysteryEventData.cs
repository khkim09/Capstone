using UnityEngine;

[CreateAssetMenu(fileName = "MysteryEventData", menuName = "MysteryEvent/MysteryEventData")]
public class MysteryEventData : ScriptableObject
{
    public string eventName;
    [TextArea]
    public string eventDescription;

    public int durationYears;
    public MoraleEffect[] moraleEffects;
}

[System.Serializable]
public struct MoraleEffect
{
    public bool isGlobal;
    public CrewRace raceTarget; // 무시됨 if global == true
    public float value;
}
