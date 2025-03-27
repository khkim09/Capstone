using UnityEngine;

[CreateAssetMenu(fileName = "MysteryEventData", menuName = "MysteryEvent/MysteryEventData")]
public class MysteryEventData : ScriptableObject
{
    public string eventName;
    [TextArea] public string eventDescription;

    public Sprite icon;
    public AudioClip sfx;
    public int durationYears;
    public bool isStackable = false;
    public MoraleEffect[] moraleEffects;
}

[System.Serializable]
public struct MoraleEffect
{
    public bool isGlobal;
    public CrewRace raceTarget; // 무시됨 if global == true
    public float value;
}
