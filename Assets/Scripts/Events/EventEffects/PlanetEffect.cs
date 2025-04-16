using System;
using System.Collections.Generic;

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
    public float fluctuation;
}
