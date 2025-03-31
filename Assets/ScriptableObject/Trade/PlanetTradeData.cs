using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// PlanetTradeData는 특정 행성의 무역 데이터를 보관하는 ScriptableObject입니다.
/// 이 클래스는 행성 코드와 해당 행성에서 판매하는 무역 아이템 목록을 포함합니다.
/// </summary>
[CreateAssetMenu(fileName = "PlanetTradeData", menuName = "Trade/Planet Trade Data")]
public class PlanetTradeData : ScriptableObject
{
    /// <summary>
    /// 행성 코드입니다. 예를 들어, SIS, CCK 등이 있습니다.
    /// </summary>
    [Tooltip("행성 코드 (예: SIS, CCK 등)")]
    public string planetCode;

    /// <summary>
    /// 해당 행성에서 판매하는 무역 아이템 목록입니다.
    /// </summary>
    [Tooltip("해당 행성에서 판매하는 물품 목록")]
    public List<TradableItem> items;
}
