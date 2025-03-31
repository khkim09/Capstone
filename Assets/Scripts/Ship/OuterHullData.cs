using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;


/// <summary>
/// 외갑판의 업그레이드 정보를 담는 ScriptableObject.
/// 레벨별 이름, 비용, 피해 감소 수치 등을 포함합니다.
/// </summary>
public class OuterHullData : ScriptableObject
{
    /// <summary>
    /// 외갑판의 개별 레벨 정보를 담는 클래스.
    /// </summary>
    public class OuterHullLevel
    {
        /// <summary>
        /// 외갑판 레벨의 내부 이름 또는 코드입니다.
        /// </summary>
        public string outerHullName;

        /// <summary>
        /// 외갑판의 레벨 번호입니다.
        /// </summary>
        public int level;

        /// <summary>
        /// 외곽면 하나당 업그레이드 비용입니다.
        /// </summary>
        public int costPerSurface;

        /// <summary>
        /// 이 레벨이 제공하는 피해 감소 수치입니다.
        /// </summary>
        public float damageReduction;
    }

    /// <summary>
    /// 외갑판의 모든 레벨 데이터를 저장하는 리스트입니다.
    /// </summary>
    protected List<OuterHullLevel> OuterHullLevels = new();

    /// <summary>
    /// 지정한 레벨에 해당하는 외갑판 데이터를 반환합니다.
    /// </summary>
    /// <param name="level">가져올 외갑판 레벨 (0부터 시작하는 인덱스).</param>
    /// <returns>해당 레벨의 외갑판 데이터.</returns>
    public OuterHullLevel GetOuterHullData(int level)
    {
        return OuterHullLevels[level];
    }

    /// <summary>
    /// 기본 외갑판 레벨 데이터를 초기화합니다.
    /// 주로 에디터에서 ScriptableObject가 로드될 때 호출됩니다.
    /// </summary>
    private void InitializeDefaultLevels()
    {
        OuterHullLevels = new List<OuterHullLevel>
        {
            new() { outerHullName = "outerhull.level1", level = 1, costPerSurface = 5, damageReduction = 0 },
            new() { outerHullName = "outerhull.level2", level = 1, costPerSurface = 10, damageReduction = 5 },
            new() { outerHullName = "outerhull.level3", level = 1, costPerSurface = 20, damageReduction = 10 }
        };
    }

    /// <summary>
    /// ScriptableObject가 활성화될 때 자동으로 호출됩니다.
    /// 기본 레벨 데이터를 설정합니다.
    /// </summary>
    protected virtual void OnEnable()
    {
        InitializeDefaultLevels();
    }
}
