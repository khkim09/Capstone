using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 외갑판의 업그레이드 정보를 담는 ScriptableObject.
/// 레벨별 이름, 비용, 피해 감소 수치, 스프라이트 등을 포함합니다.
/// </summary>
[CreateAssetMenu(fileName = "OuterHullData", menuName = "OuterHullData/OuterHull Data")]
public class OuterHullData : ScriptableObject
{
    /// <summary>
    /// 외갑판의 개별 레벨 정보를 담는 클래스.
    /// </summary>
    [Serializable]
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

        /// <summary>
        /// 외갑판 방향별 스프라이트 (상, 우, 하, 좌, 모서리-상우, 모서리-하우, 모서리-하좌, 모서리-상좌)
        /// </summary>
        public Sprite[] directionSprites;
    }

    /// <summary>
    /// 외갑판의 모든 레벨 데이터를 저장하는 리스트입니다.
    /// </summary>
    [SerializeField] protected List<OuterHullLevel> OuterHullLevels = new();

    /// <summary>
    /// 레벨 1 외갑판의 방향별 스프라이트 (인스펙터에서 설정)
    /// [0]: 상, [1]: 우, [2]: 하, [3]: 좌
    /// [4]: 모서리-상우, [5]: 모서리-하우, [6]: 모서리-하좌, [7]: 모서리-상좌
    /// [8]: 내부 모서리-상우, [9]: 내부 모소리-하우, [10]: 내부 모서리-하좌, [11]: 내부 모서리-상좌
    /// </summary>
    [Header("레벨 1 외갑판 스프라이트")]
    public Sprite[] level1Sprites = new Sprite[12];

    /// <summary>
    /// 레벨 2 외갑판의 방향별 스프라이트 (인스펙터에서 설정)
    /// </summary>
    [Header("레벨 2 외갑판 스프라이트")]
    public Sprite[] level2Sprites = new Sprite[8];

    /// <summary>
    /// 레벨 3 외갑판의 방향별 스프라이트 (인스펙터에서 설정)
    /// </summary>
    [Header("레벨 3 외갑판 스프라이트")]
    public Sprite[] level3Sprites = new Sprite[8];

    /// <summary>
    /// 지정한 레벨에 해당하는 외갑판 데이터를 반환합니다.
    /// </summary>
    /// <param name="level">가져올 외갑판 레벨 (0부터 시작하는 인덱스).</param>
    /// <returns>해당 레벨의 외갑판 데이터.</returns>
    public OuterHullLevel GetOuterHullData(int level)
    {
        if (level >= 0 && level < OuterHullLevels.Count)
        {
            return OuterHullLevels[level];
        }

        Debug.LogWarning($"외갑판 레벨 {level}에 대한 데이터가 없습니다.");
        return null;
    }

    /// <summary>
    /// 지정한 레벨에 해당하는 외갑판 스프라이트 배열을 반환합니다.
    /// </summary>
    /// <param name="level">외갑판 레벨 (0, 1, 2)</param>
    /// <returns>해당 레벨의 스프라이트 배열</returns>
    public Sprite[] GetHullSprites(int level)
    {
        switch (level)
        {
            case 0:
                return level1Sprites;
            case 1:
                return level2Sprites;
            case 2:
                return level3Sprites;
            default:
                Debug.LogWarning($"유효하지 않은 외갑판 레벨: {level}");
                return level1Sprites; // 기본값
        }
    }

    /// <summary>
    /// 특정 레벨과 방향에 해당하는 외갑판 스프라이트를 반환합니다.
    /// </summary>
    /// <param name="level">외갑판 레벨 (0-2)</param>
    /// <param name="direction">방향 (0: 상, 1: 우, 2: 하, 3: 좌, 4-7: 모서리)</param>
    /// <returns>해당 스프라이트</returns>
    public Sprite GetSpecificHullSprite(int level, int direction)
    {
        Sprite[] sprites = GetHullSprites(level);

        if (sprites != null && direction >= 0 && direction < sprites.Length)
        {
            return sprites[direction];
        }

        Debug.LogWarning($"외갑판 스프라이트를 찾을 수 없습니다: 레벨 {level}, 방향 {direction}");
        return null;
    }

    /// <summary>
    /// 기본 외갑판 레벨 데이터를 초기화합니다.
    /// 주로 에디터에서 ScriptableObject가 로드될 때 호출됩니다.
    /// </summary>
    private void InitializeDefaultLevels()
    {
        OuterHullLevels = new List<OuterHullLevel>
        {
            new() { outerHullName = "outerhull.level1", level = 1, costPerSurface = 5, damageReduction = 0, directionSprites = level1Sprites },
            new() { outerHullName = "outerhull.level2", level = 2, costPerSurface = 10, damageReduction = 5, directionSprites = level2Sprites },
            new() { outerHullName = "outerhull.level3", level = 3, costPerSurface = 20, damageReduction = 10, directionSprites = level3Sprites }
        };
    }

    /// <summary>
    /// ScriptableObject가 활성화될 때 자동으로 호출됩니다.
    /// 기본 레벨 데이터와 스프라이트를 설정합니다.
    /// </summary>
    protected virtual void OnEnable()
    {
        if (OuterHullLevels == null || OuterHullLevels.Count == 0)
        {
            InitializeDefaultLevels();
        }
        else
        {
            // 기존 레벨 데이터에 스프라이트 업데이트
            if (OuterHullLevels.Count >= 1 && OuterHullLevels[0] != null)
                OuterHullLevels[0].directionSprites = level1Sprites;

            if (OuterHullLevels.Count >= 2 && OuterHullLevels[1] != null)
                OuterHullLevels[1].directionSprites = level2Sprites;

            if (OuterHullLevels.Count >= 3 && OuterHullLevels[2] != null)
                OuterHullLevels[2].directionSprites = level3Sprites;
        }
    }
}
