using System;
using UnityEngine;

public class MoraleEffect : MonoBehaviour
{
    /// <summary>
    /// 대상 종족 (None 이면 전체)
    /// </summary>
    public CrewRace targetRace;

    /// <summary>
    /// 적용되는 사기 값
    /// </summary>
    /// <returns></returns>
    [Range(-20, 20)] public int value;

    /// <summary>
    /// 지속 년도
    /// </summary>
    public int duration;

    /// <summary>
    /// 시작 연도
    /// </summary>
    public int startYear;

    /// <summary>
    /// 끝나는 년도 프로퍼티
    /// </summary>
    public int EndYear => startYear + duration;

    /// <summary>
    /// 스프라이트 렌더러.
    /// </summary>
    public SpriteRenderer spriteRenderer;

    /// <summary>
    /// 호버했을 때의 툴팁
    /// </summary>
    public GameObject toolTip;


    public void Initialize()
    {
    }
}
