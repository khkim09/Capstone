﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 행성 데이터
/// </summary>
public class PlanetData
{
    /// <summary>
    /// 행성 고유 ID
    /// </summary>
    public int planetId;

    /// <summary>
    /// 행성 이름
    /// </summary>
    public string planetName;

    /// <summary>
    /// 행성 종족
    /// </summary>
    public PlanetRace planetRace;

    /// <summary>
    /// 무역 아이템 매칭에 필요한 행성 이름 정보
    /// </summary>
    public ItemPlanet itemPlanet;

    /// <summary>
    /// 행성맵에 표시될 상대적인 좌표.
    /// </summary>
    public Vector2 normalizedPosition;

    /// <summary>
    /// 행성 sprite
    /// </summary>
    public Sprite currentSprite;

    /// <summary>
    /// 판매하는 티어 1 아이템 리스트
    /// </summary>
    public List<TradingItemData> itemsTier1 = new();

    /// <summary>
    /// 판매하는 티어 2 아이템 리스트
    /// </summary>
    public List<TradingItemData> itemsTier2 = new();

    /// <summary>
    /// 판매하는 티어 3 아이템 리스트
    /// </summary>
    public List<TradingItemData> itemsTier3 = new();

    /// <summary>
    /// 현재까지 수익을 낸 총 판매액
    /// </summary>
    public int currentRevenue = 0;

    /// <summary>
    /// 현재 해금되어있는 티어
    /// </summary>
    public ItemTierLevel CurrentTier => currentRevenue > Constants.Planets.Tier3RevenueRequirement
        ? ItemTierLevel.T3
        : currentRevenue > Constants.Planets.Tier2RevenueRequirement
            ? ItemTierLevel.T2
            : ItemTierLevel.T1;

    /// <summary>
    /// 현재 연료 가격
    /// </summary>
    [Range(50, 150)] public int currentFuelPrice = 100;

    /// <summary>
    /// 현재 미사일 가격
    /// </summary>
    public int currentMissilePrice = 300;

    /// <summary>
    /// 현재 초음속탄 가격
    /// </summary>
    public int currentHypersonicPrice = 500;

    /// <summary>
    /// 현재 행성의 퀘스트 리스트.
    /// </summary>
    public List<RandomQuest> questList = new();

    /// <summary>
    /// 현재 적용 중인 행성 효과 데이터
    /// </summary>
    public List<PlanetEffect> activeEffects = new();

    /// <summary>
    /// 이벤트로 변동되는 카테고리별 현재 가격 변동율 (%)
    /// </summary>
    public Dictionary<ItemCategory, float> categoryPriceModifiers = new();

    /// <summary>
    /// 아이템의 현재 가격 딕셔너리
    /// </summary>
    public Dictionary<int, int> itemPriceDictionary = new();

    /// <summary>
    /// 현재 판매하고 있는 장비 아이템 (Trade 의 장비 거래 패널의 세 번째 슬롯).
    /// </summary>
    public EquipmentItem currentRandomEquipmentItem;

    public PlanetRace PlanetRace => planetRace;

    public bool isHome = false;

    /// <summary>
    /// 행성의 데이터를 랜덤으로 생성합니다. 게임 시작할 때 호출되며, 게임의 여러 요소가 한 쪽으로 편향되지 않게 적절한 설정이 적용.
    /// </summary>
    public void CreateRandomData()
    {
        PlanetRace[] allRaces = (PlanetRace[])Enum.GetValues(typeof(PlanetRace));
        int totalPlanets = Constants.Planets.PlanetTotalCount;

        if (GameManager.Instance.PlanetDataList.Count < allRaces.Length)
        {
            int index = GameManager.Instance.PlanetDataList.Count;
            planetRace = allRaces[index];
            itemPlanet = (ItemPlanet)index;
            planetId = index;
        }
        else
        {
            int index = Random.Range(0, allRaces.Length);
            planetRace = allRaces[index];
            itemPlanet = (ItemPlanet)index;
            planetId = GameManager.Instance.PlanetDataList.Count;
        }

        SetRandomName();

        SetRandomPosition();

        SetItems();

        RefreshEquipmentSelling();

        currentRevenue = 0;
        currentFuelPrice = Random.Range(50, 151);

        // 효과 관련 데이터 초기화
        activeEffects.Clear();
        InitializeCategoryModifiers();
    }

    public void LoadSprites()
    {
        switch (itemPlanet)
        {
            case ItemPlanet.CCK:
                currentSprite = Resources.Load<Sprite>("Sprites/Planet/CCK");
                break;
            case ItemPlanet.ICM:
                currentSprite = Resources.Load<Sprite>("Sprites/Planet/ICM");
                break;
            case ItemPlanet.RCE:
                currentSprite = Resources.Load<Sprite>("Sprites/Planet/RCE");
                break;
            case ItemPlanet.KTL:
                currentSprite = Resources.Load<Sprite>("Sprites/Planet/KTL");
                break;
            case ItemPlanet.SIS:
                currentSprite = Resources.Load<Sprite>("Sprites/Planet/SIS");
                break;
        }
    }

    /// <summary>
    /// 카테고리별 가격 변동율 초기화
    /// </summary>
    private void InitializeCategoryModifiers()
    {
        categoryPriceModifiers.Clear();

        // 모든 아이템 카테고리에 대해 초기값 0%로 설정
        foreach (ItemCategory category in Enum.GetValues(typeof(ItemCategory))) categoryPriceModifiers[category] = 0f;
    }

    #region 이름

    /// <summary>
    /// 행성 이름을 중복되지 않게 랜덤하게 생성합니다. (종족 접두어 + 숫자)
    /// </summary>
    private void SetRandomName()
    {
        List<string> allNames = GameManager.Instance.PlanetDataList
            .Select(planet => planet.planetName)
            .ToList();

        string prefix = GetSpeciesPrefix(planetRace);

        string newName;

        // 중복되지 않은 이름 나올 때까지 반복
        do
        {
            int randomNumber = Random.Range(100, 1000); // 3자리 숫자 (100~999)
            newName = $"{prefix}-{randomNumber}";
        } while (allNames.Contains(newName));

        planetName = newName;
    }


    /// <summary>
    /// 종족 유형에 따라 행성 이름 접두어를 반환합니다.
    /// </summary>
    /// <param name="species">종족 타입</param>
    /// <returns>종족 접두어 문자열</returns>
    public string GetSpeciesPrefix(PlanetRace species)
    {
        switch (species)
        {
            case PlanetRace.Aquatic: // 어인
                return "SIS";
            case PlanetRace.Avian: // 조류
                return "CCK";
            case PlanetRace.IceMan: // 선인
                return "ICM";
            case PlanetRace.Human: // 인간형
                return "RCE";
            case PlanetRace.Amorphous: // 부정형
                return "KTL";
            default:
                return "UNK"; // Unknown, fallback
        }
    }

    #endregion

    #region 위치

    /// <summary>
    /// 행성이 맵에 그려지는 상대적인 위치를 랜덤으로 생성.
    /// 다른 행성들과의 거리가 PlanetSpacingMin 이상 유지되도록 함.
    /// </summary>
    private void SetRandomPosition()
    {
        const int maxAttempts = 100; // 무한 루프 방지를 위한 최대 시도 횟수

        Vector2 candidatePosition;
        bool validPosition = false;
        int attempts = 0;

        do
        {
            // 0.0f ~ 1.0f 범위에서 랜덤 위치 생성
            candidatePosition = new Vector2(
                Random.Range(Constants.Planets.PlanetSpacingEdge, 1f - Constants.Planets.PlanetSpacingEdge),
                Random.Range(Constants.Planets.PlanetSpacingEdge, 1f - Constants.Planets.PlanetSpacingEdge)
            );

            validPosition = true;

            float distancePlayer = Vector2.Distance(candidatePosition, GameManager.Instance.normalizedPlayerPosition);

            if (distancePlayer <= Constants.Planets.PlanetSpacingMin)
                validPosition = false;

            // 기존의 모든 행성과의 거리 체크
            foreach (PlanetData existingPlanet in GameManager.Instance.PlanetDataList)
            {
                // 자기 자신은 제외
                if (existingPlanet == this)
                    continue;

                float distance = Vector2.Distance(candidatePosition, existingPlanet.normalizedPosition);

                // 거리가 최소 거리보다 작으면 무효한 위치
                if (distance < Constants.Planets.PlanetSpacingMin)
                {
                    validPosition = false;
                    break;
                }
            }

            attempts++;

            // 최대 시도 횟수에 도달했지만 유효한 위치를 찾지 못한 경우
            if (attempts >= maxAttempts && !validPosition)
            {
                Debug.LogWarning($"Planet {planetName}: 최대 시도 횟수({maxAttempts})에 도달했습니다. 마지막 위치를 사용합니다.");
                validPosition = true; // 강제로 루프 종료
            }
        } while (!validPosition);

        normalizedPosition = candidatePosition;
    }

    #endregion

    #region 아이템

    /// <summary>
    /// 행성에서 판매하는 아이템들을 세팅합니다.
    /// </summary>
    public void SetItems()
    {
        itemsTier1 = GameObjectFactory.Instance.ItemFactory.itemDataBase
            .GetItemsByPlanet(itemPlanet)
            .Where(item => item.tier == ItemTierLevel.T1)
            .ToList();

        itemsTier2 = GameObjectFactory.Instance.ItemFactory.itemDataBase
            .GetItemsByPlanet(itemPlanet)
            .Where(item => item.tier == ItemTierLevel.T2)
            .ToList();

        itemsTier3 = GameObjectFactory.Instance.ItemFactory.itemDataBase
            .GetItemsByPlanet(itemPlanet)
            .Where(item => item.tier == ItemTierLevel.T3)
            .ToList();

        foreach (TradingItemData item in GameObjectFactory.Instance.ItemFactory.itemDataBase.allItems)
            itemPriceDictionary.Add(item.id, Random.Range(item.costMin, item.costMax));
    }

    public void ChangeItemPrice()
    {
        List<int> keys = new(itemPriceDictionary.Keys); // 키 복사

        foreach (int key in keys)
        {
            TradingItemData item = GameObjectFactory.Instance.ItemFactory.itemDataBase.allItems[key];
            itemPriceDictionary[item.id] = Random.Range(item.costMin, item.costMax);
        }
    }

    public int GetItemPrice(int itemId)
    {
        TradingItemData item = GameObjectFactory.Instance.ItemFactory.itemDataBase.GetItemData(itemId);
        float multiplier = categoryPriceModifiers[item.type];
        int planetPrice = itemPriceDictionary[item.id];

        return (int)(planetPrice * (100 + multiplier) / 100);
    }

    public int GetItemPrice(TradingItemData item)
    {
        float multiplier = categoryPriceModifiers[item.type];
        int planetPrice = itemPriceDictionary[item.id];

        float stateMulitplier = 100;
        switch (item.itemState)
        {
            case ItemState.Normal:
                stateMulitplier = 0;
                break;
            case ItemState.SlightlyDamaged:
                stateMulitplier = -25;
                break;
            case ItemState.Damaged:
                stateMulitplier = -50;
                break;
            case ItemState.Unsellable:
                stateMulitplier = -100;
                break;
        }

        return (int)(planetPrice * (100 + multiplier) / 100 * (100 + stateMulitplier) / 100);
    }

    #endregion

    #region 행성 이벤트 효과

    /// <summary>
    /// 새로운 행성 효과 등록
    /// </summary>
    /// <param name="effectData">등록할 효과 데이터</param>
    public void RegisterPlanetEffect(PlanetEffect effectData)
    {
        // 기존에 같은 카테고리에 대한 효과가 있는지 확인
        PlanetEffect existingEffect = activeEffects.Find(e => e.categoryType == effectData.categoryType);

        if (existingEffect != null)
        {
            // 기존 효과 제거
            RemovePlanetEffectValue(existingEffect);
            activeEffects.Remove(existingEffect);

            Debug.Log($"[PlanetEffect] 행성 {planetName}의 기존 {existingEffect.categoryType} 효과를 새 효과로 대체합니다.");
        }

        // 효과 리스트에 추가
        activeEffects.Add(effectData);

        // 효과 값 적용
        ApplyPlanetEffectValue(effectData);

        Debug.Log(
            $"[PlanetEffect] 행성 {planetName}에 {effectData.categoryType} 효과가 적용되었습니다. 변동률: {effectData.changeAmount}%, 종료 년도: {effectData.EndYear}, 시작 년도 : {GameManager.Instance.CurrentYear}");
    }

    /// <summary>
    /// 행성 효과 값 적용
    /// </summary>
    /// <param name="effect">적용할 효과</param>
    private void ApplyPlanetEffectValue(PlanetEffect effect)
    {
        // 현재 카테고리의 가격 변동율 업데이트
        categoryPriceModifiers[effect.categoryType] += effect.changeAmount;

        Debug.Log(
            $"[PlanetEffect] {planetName} 행성의 {effect.categoryType} 가격 변동률: {categoryPriceModifiers[effect.categoryType]}%");
    }

    /// <summary>
    /// 행성 효과 값 제거
    /// </summary>
    /// <param name="effect">제거할 효과</param>
    private void RemovePlanetEffectValue(PlanetEffect effect)
    {
        // 현재 카테고리의 가격 변동율 업데이트
        categoryPriceModifiers[effect.categoryType] -= effect.changeAmount;

        Debug.Log(
            $"[PlanetEffect] {planetName} 행성의 {effect.categoryType} 가격 변동률 효과 제거: 현재 {categoryPriceModifiers[effect.categoryType]}%");
    }

    public void CheckPirateQuests()
    {
        foreach (RandomQuest quest in questList.Where(q =>
                     q.objectives[0].objectiveType == QuestObjectiveType.PirateHunt && q.status == QuestStatus.Active))
        {
            quest.objectives[0].currentAmount++;

            if (quest.objectives[0].currentAmount > quest.objectives[0].amount) quest.SetCanComplete(true);
        }
    }

    public void CheckItemQuests(int itemId)
    {
        foreach (RandomQuest quest in questList.Where(q =>
                     q.objectives[0].objectiveType == QuestObjectiveType.ItemTransport ||
                     q.objectives[0].objectiveType == QuestObjectiveType.ItemProcurement ||
                     q.status == QuestStatus.Active))
        {
            int targetId = quest.objectives[0].targetId;
            int targetAmount = quest.objectives[0].amount;

            bool hasMatchingItem = GameManager.Instance.playerShip.GetAllItems()
                .Any(i => i.GetItemId() == targetId && i.GetItemData().amount == targetAmount);

            if (hasMatchingItem)
                quest.SetCanComplete(true);
            else
                quest.SetCanComplete(false);
        }
    }

    /// <summary>
    /// 현재 이벤트 상태를 확인하고 만료된 효과를 제거합니다
    /// </summary>
    /// <param name="currentYear">현재 게임 년도</param>
    public void CheckEventExpirations(int currentYear)
    {
        if (activeEffects.Count == 0) return;

        // 만료된 효과 찾기
        List<ItemCategory> categoriesToUpdate = new();

        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            PlanetEffect effect = activeEffects[i];

            if (currentYear >= effect.EndYear)
            {
                // 만료된 효과의 카테고리 기록
                if (!categoriesToUpdate.Contains(effect.categoryType))
                    categoriesToUpdate.Add(effect.categoryType);

                // 효과 값 제거
                RemovePlanetEffectValue(effect);

                // 효과 목록에서 제거
                activeEffects.RemoveAt(i);

                Debug.Log($"[PlanetEffect] 행성 {planetName}의 {effect.categoryType} 효과가 만료되었습니다.");
            }
        }

        // 변경된 카테고리별로 가격 업데이트 필요
        foreach (ItemCategory category in categoriesToUpdate)
        {
            // 여기서 실제 가격에 변동률을 적용하는 로직 추가 (필요시)
            // UpdateCategoryPrices(category);
        }
    }


    /// <summary>
    /// 특정 카테고리의 현재 가격 변동률 반환
    /// </summary>
    /// <param name="category">확인할 아이템 카테고리</param>
    /// <returns>현재 가격 변동률 (%)</returns>
    public float GetCategoryPriceModifier(ItemCategory category)
    {
        if (categoryPriceModifiers.TryGetValue(category, out float modifier)) return modifier;
        return 0f;
    }

    /// <summary>
    /// 모든 행성 효과 초기화
    /// </summary>
    public void ResetAllEffects()
    {
        activeEffects.Clear();
        InitializeCategoryModifiers();
        Debug.Log($"[PlanetEffect] 행성 {planetName}의 모든 효과가 초기화되었습니다.");
    }

    /// <summary>
    /// 현재 가격 변동률을 고려하여 아이템 가격 계산
    /// </summary>
    /// <param name="basePrice">기본 가격</param>
    /// <param name="category">아이템 카테고리</param>
    /// <returns>조정된 가격</returns>
    public int CalculateAdjustedPrice(int basePrice, ItemCategory category)
    {
        float modifier = GetCategoryPriceModifier(category);
        float multiplier = 1f + modifier / 100f; // 퍼센트를 곱셈 계수로 변환

        int adjustedPrice = Mathf.RoundToInt(basePrice * multiplier);
        return Mathf.Max(1, adjustedPrice); // 최소 가격 1 보장
    }

    /// <summary>
    /// 현재 행성 효과 상태 출력 (디버깅용)
    /// </summary>
    public void PrintAllEffects()
    {
        Debug.Log($"=== 행성 {planetName} 효과 상태 ===");

        Debug.Log("=== 카테고리별 가격 변동률 ===");
        foreach (KeyValuePair<ItemCategory, float> kvp in categoryPriceModifiers) Debug.Log($"{kvp.Key}: {kvp.Value}%");

        Debug.Log("=== 적용 중인 효과 ===");
        foreach (PlanetEffect effect in activeEffects)
            Debug.Log($"카테고리: {effect.categoryType}, 변동률: {effect.changeAmount}%, 종료 년도: {effect.EndYear}");
    }

    #endregion

    #region 퀘스트

    public void CheckQuestExpirations(int currentYear)
    {
        List<RandomQuest> activeQuestList = questList.Where(q => q.status == QuestStatus.Active).ToList();
        for (int i = activeQuestList.Count - 1; i >= 0; i--)
            if (activeQuestList[i].QuestExpiredYear >= currentYear)
                FailQuest(activeQuestList[i]);
    }

    public void ActivateQuest(RandomQuest quest)
    {
        quest.status = QuestStatus.Active;
        quest.questAcceptedYear = GameManager.Instance.CurrentYear;
    }

    public void CompleteQuest(RandomQuest quest)
    {
        quest.status = QuestStatus.Completed;
        int reward = 0;
        if (quest.rewards[0].questRewardType == QuestRewardType.COMA)
            reward = quest.rewards[0].amount;


        switch (quest.objectives[0].objectiveType)
        {
            case QuestObjectiveType.ItemTransport:
            case QuestObjectiveType.ItemProcurement:
                int targetItemId = quest.objectives[0].targetId;
                int targetAmount = quest.objectives[0].amount;
                foreach (Room room in GameManager.Instance.playerShip.GetAllRooms())
                    if (room is StorageRoomBase storageRoom)
                    {
                        TradingItem targetItem = storageRoom.storedItems
                            .FirstOrDefault(t =>
                                t.GetItemData().id == targetItemId && t.GetItemData().amount == targetAmount);

                        if (targetItem != null) storageRoom.DestroyItem(targetItem);
                    }

                break;
            case QuestObjectiveType.PirateHunt:

                break;
            case QuestObjectiveType.CrewTransport:
                break;
        }

        ResourceManager.Instance.ChangeResource(ResourceType.COMA, reward);
        questList.Remove(quest);
    }


    /// <summary>
    /// 퀘스트를 실패 처리합니다.
    /// </summary>
    /// <param name="quest">실패할 퀘스트</param>
    public void FailQuest(RandomQuest quest)
    {
        quest.status = QuestStatus.Failed;
        questList.Remove(quest);
    }

    public void TrySpawnQuest()
    {
        if (Random.value <= Constants.Quest.QuestCreateRate) CreateRandomQuest();
    }

    private void CreateRandomQuest()
    {
        RandomQuest quest = new();
        QuestObjective objective = new();
        int reward = 0;
        Array values = Enum.GetValues(typeof(QuestObjectiveType));
        QuestObjectiveType randomType = (QuestObjectiveType)values.GetValue(Random.Range(0, values.Length - 1));
        PlanetData randomPlanetData = null;
        do
        {
            randomPlanetData = GameManager.Instance.GetRandomPlanetData();
        } while (randomPlanetData == this);


        TradingItemDataBase itemDatabase = GameObjectFactory.Instance.ItemFactory.itemDataBase;
        TradingItemData item = null;
        PlanetData targetPlanetData = null;

        switch (randomType)
        {
            case QuestObjectiveType.PirateHunt:
                objective.objectiveType = QuestObjectiveType.PirateHunt;
                quest.title = "ui.quest.title.piratehunt";
                objective.amount = Random.Range(5, 21);
                objective.targetPlanetDataId = planetId;
                objective.description = "ui.quest.objective.piratehunt";

                reward = objective.amount * 100;
                break;
            case QuestObjectiveType.ItemTransport:
                objective.objectiveType = QuestObjectiveType.ItemTransport;
                quest.title = "ui.quest.title.itemtransport";
                item = itemDatabase.GetRandomItem();
                objective.targetId = item.id;
                objective.amount = item.capacity;
                objective.targetPlanetDataId = randomPlanetData.planetId;
                targetPlanetData = GameManager.Instance.PlanetDataList[randomPlanetData.planetId];
                objective.targetPlanetDataId = targetPlanetData.planetId;
                objective.description = "ui.quest.objective.itemtransport";

                reward = (int)(item.costMax * 1.1f * objective.amount);
                break;
            case QuestObjectiveType.ItemProcurement:
                objective.objectiveType = QuestObjectiveType.ItemProcurement;
                quest.title = "ui.quest.title.itemprocurement";
                item = itemDatabase.GetRandomItem();
                objective.targetId = item.id;

                objective.amount = item.capacity;
                objective.targetPlanetDataId = planetId;
                objective.description =
                    "ui.quest.objective.itemprocurement";
                reward = (int)(item.costMax * 1.1f * objective.amount);
                break;
            case QuestObjectiveType.CrewTransport:
                objective.objectiveType = QuestObjectiveType.CrewTransport;
                quest.title = "ui.quest.title.crewtransport";
                objective.amount = 1;
                objective.targetPlanetDataId = randomPlanetData.planetId;
                targetPlanetData = GameManager.Instance.PlanetDataList[randomPlanetData.planetId];
                objective.targetPlanetDataId = targetPlanetData.planetId;

                objective.description = objective.description =
                    "ui.quest.objective.crewtransport";

                reward = objective.amount * 500;
                break;
        }


        quest.rewards.Add(new QuestReward() { questRewardType = QuestRewardType.COMA, amount = reward });
        quest.status = QuestStatus.NotStarted;
        quest.objectives.Add(objective);
        questList.Add(quest);
    }

    #endregion

    #region 고용

    #endregion

    #region 장비 판매

    public void RefreshEquipmentSelling()
    {
        List<EquipmentItem> allRandomEquipmentList =
            EquipmentManager.Instance.equipmentDatabase.GetEquipmentByPlanet(itemPlanet);

        currentRandomEquipmentItem = allRandomEquipmentList[Random.Range(0, allRandomEquipmentList.Count)];

        GameManager.Instance.isBoughtEquipment = false;
    }

    #endregion

    #region 자원

    public void ChangeFuelPrice()
    {
        currentFuelPrice = Random.Range(50, 151);
    }

    #endregion

    public void SetPlanetRace(PlanetRace planetRace)
    {
        this.planetRace = planetRace;
    }
}
