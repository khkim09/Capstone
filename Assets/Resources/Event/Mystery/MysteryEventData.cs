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
}
