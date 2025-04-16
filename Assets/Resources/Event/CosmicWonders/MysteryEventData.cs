using UnityEngine;

/// <summary>
/// 불가사의를 위한 Scriptable Object
/// </summary>
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

/// <summary>
/// 사기 효과 적용 범위, 대상, 값
/// </summary>
[System.Serializable]
public struct MoraleEffect
{
    public bool isGlobal;
    public CrewRace raceTarget; // 무시됨 if global == true
    public float value;
}
