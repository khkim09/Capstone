using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

/// <summary>
/// 행성에 가해지는 변화를 나타내는 클래스입니다.
/// 물품의 종류에 따라 가격이 오르거나 내릴 수 있습니다.
/// </summary>
[Serializable]
public class PlanetEffect
{
    /// <summary>
    /// 아이템 종류를 나타냅니다.
    /// 종류에 따라 변동량 만큼의 가격 변동이 일어납니다.
    /// </summary>
    public ItemCategory categoryType;

    /// <summary>
    /// 변동량을 나타냅니다.
    /// 양수면 가격이 증가, 음수면 가격이 갑소할 수 있습니다.
    /// </summary>
    public float changeAmount;

    /// <summary>
    /// 아이템 가격 변동의 지속 시간
    /// 상수값을 사용하지만 만약 이벤트 별로 다르게 설정하고 싶으면
    /// NonSerialized 필드를 제거하고 각 ScriptableObject에서 설정할 것
    /// </summary>
    [NonSerialized] public int duration = Constants.Events.EventDuration;
}
