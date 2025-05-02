using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 외갑판 업그레이드를 나타내는 클래스.
/// 방처럼 배치되지는 않지만, 전체 함선 스탯에 영향을 주는 구성 요소입니다.
/// </summary>
public class OuterHull : MonoBehaviour, IShipStatContributor
{
    /// <summary>
    /// 외갑판 데이터 (레벨별 스탯 포함).
    /// </summary>
    [SerializeField] private OuterHullData outerHullData;

    /// <summary>
    /// 현재 외갑판의 레벨입니다.
    /// </summary>
    [SerializeField] private int currentLevel = 0;

    /// <summary>
    /// 현재 레벨에 해당하는 외갑판 상세 데이터입니다.
    /// </summary>
    private OuterHullData.OuterHullLevel currentLevelData;

    /// <summary>
    /// 외갑판의 그리드 위치
    /// </summary>
    public Vector2Int gridPosition;

    /// <summary>
    /// 외갑판 타일의 방향 (0-3: 하좌상우, 4-7: 하좌/상좌/상우/하우 모서리, 8-11: 내부 모서리)
    /// </summary>
    public int direction = 0;

    /// <summary>
    /// 스프라이트 렌더러 컴포넌트
    /// </summary>
    private SpriteRenderer spriteRenderer;

    /// <summary>
    /// a소속된 함선 참조
    /// </summary>
    private Ship parentShip;

    /// <summary>
    /// 컴포넌트가 활성화될 때 외갑판을 초기화합니다.
    /// </summary>
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        Initialize();
    }

    /// <summary>
    /// 외갑판 레벨 데이터를 초기화합니다.
    /// </summary>
    public void Initialize()
    {
        if (outerHullData == null)
        {
            Debug.LogError("OuterHullData is not assigned.");
            return;
        }

        currentLevelData = outerHullData.GetOuterHullData(currentLevel);
        if (currentLevelData == null)
            Debug.LogWarning($"No outer hull data found for level {currentLevel}");
    }

    /// <summary>
    /// 외갑판 타일 초기화
    /// </summary>
    /// <param name="position">그리드 위치</param>
    /// <param name="level">외갑판 레벨</param>
    /// <param name="dir">방향 (0-3: 하좌상우, 4-7: 모서리, 8-11: 내부 모서리)</param>
    /// <param name="sprite">사용할 스프라이트</param>
    /// <param name="ship">소속 함선</param>
    public void Initialize(Vector2Int position, int level, int dir, Sprite sprite, Ship ship)
    {
        gridPosition = position;
        currentLevel = level;
        direction = dir;
        parentShip = ship;

        // 현재 레벨에 맞는 데이터 설정
        if (outerHullData != null) currentLevelData = outerHullData.GetOuterHullData(currentLevel);

        // 스프라이트 설정
        UpdateSprite(sprite);
    }

    /// <summary>
    /// 외갑판 데이터를 설정합니다.
    /// </summary>
    /// <param name="data">외갑판 데이터</param>
    public void SetOuterHullData(OuterHullData data)
    {
        outerHullData = data;

        // 현재 레벨에 맞는 데이터 업데이트
        if (outerHullData != null) currentLevelData = outerHullData.GetOuterHullData(currentLevel);
    }

    /// <summary>
    /// 외갑판 스프라이트 업데이트
    /// </summary>
    /// <param name="sprite">새 스프라이트</param>
    public void UpdateSprite(Sprite sprite)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        if (sprite != null) spriteRenderer.sprite = sprite;
    }

    /// <summary>
    /// 외갑판 레벨과 방향에 따른 스프라이트 업데이트
    /// </summary>
    /// <param name="level">레벨 (0-2)</param>
    /// <param name="directionIndex">방향 인덱스 (0-3: 하좌상우, 4-7: 모서리, 8-11: 내부 모서리)</param>
    public void UpdateSpriteByLevelAndDirection(int level, int directionIndex)
    {
        if (outerHullData == null)
        {
            Debug.LogWarning("OuterHullData가 설정되지 않았습니다.");
            return;
        }

        Sprite newSprite = outerHullData.GetSpecificHullSprite(level, directionIndex);
        UpdateSprite(newSprite);
    }

    /// <summary>
    /// 외갑판이 함선에 기여하는 스탯을 반환합니다.
    /// 현재는 피해 감소(DamageReduction) 스탯만 제공됩니다.
    /// </summary>
    /// <returns>스탯 종류와 수치가 담긴 딕셔너리.</returns>
    public Dictionary<ShipStat, float> GetStatContributions()
    {
        Dictionary<ShipStat, float> contributions = new();

        if (currentLevelData == null)
        {
            Debug.LogWarning("OuterHullLevel data is null. Returning empty stat contribution.");
            return contributions;
        }

        // 외갑판은 피해 감소 스탯만 기여
        contributions[ShipStat.DamageReduction] = currentLevelData.damageReduction;

        return contributions;
    }

    /// <summary>
    /// 외갑판을 한 단계 업그레이드합니다.
    /// 다음 레벨의 데이터가 존재하면 업그레이드에 성공합니다.
    /// </summary>
    /// <returns>업그레이드 성공 시 true, 실패 시 false.</returns>
    public bool Upgrade()
    {
        int nextLevel = currentLevel + 1;
        OuterHullData.OuterHullLevel nextData = outerHullData.GetOuterHullData(nextLevel);

        if (nextData == null)
            return false;

        currentLevel = nextLevel;
        currentLevelData = nextData;

        // 스프라이트 업데이트
        if (outerHullData != null)
        {
            Sprite newSprite = outerHullData.GetSpecificHullSprite(currentLevel, direction);
            UpdateSprite(newSprite);
        }

        return true;
    }

    /// <summary>
    /// 현재 외갑판의 레벨을 반환합니다.
    /// </summary>
    /// <returns>현재 레벨 값.</returns>
    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    /// <summary>
    /// 외갑판 레벨 설정
    /// </summary>
    /// <param name="level">새 레벨</param>
    public void SetLevel(int level)
    {
        if (level >= 0 && level < 3)
        {
            currentLevel = level;

            // 현재 레벨에 맞는 데이터 업데이트
            if (outerHullData != null)
            {
                currentLevelData = outerHullData.GetOuterHullData(currentLevel);

                // 스프라이트 업데이트
                Sprite newSprite = outerHullData.GetSpecificHullSprite(currentLevel, direction);
                UpdateSprite(newSprite);
            }
        }
    }
}
