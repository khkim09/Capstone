using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 랜덤 퀘스트를 생성하고 QuestManager에 등록하는 클래스입니다.
/// </summary>
public class RandomQuestSpawner : MonoBehaviour
{
    /// <summary>사용할 랜덤 퀘스트 템플릿 리스트 (4종류)</summary>
    [Header("사용할 랜덤 퀘스트 원본들 (4개)")]
    public List<RandomQuest> questTemplates;

    /// <summary>아이템 정보가 들어있는 데이터베이스</summary>
    [Header("아이템 데이터베이스")]
    public TradingItemDataBase itemDatabase;

    /// <summary>행성 ID 목록 (랜덤 목적지 선택 시 사용)</summary>
    [Header("행성 ID 목록 (랜덤 행성 선택용)")]
    public List<string> planetIds = new List<string> { "SIS", "CCK", "ICM", "RCE", "KTL" };

    /// <summary>
    /// 퀘스트를 무작위로 선택하고, 실행 시점에 목표 및 보상을 구성하여 UI에 표시합니다.
    /// </summary>
    public void SpawnRandomQuest()
    {
        if (questTemplates == null || questTemplates.Count == 0)
        {
            Debug.LogWarning("퀘스트 템플릿이 비어 있습니다.");
            return;
        }

        RandomQuest selectedQuest = questTemplates[Random.Range(0, questTemplates.Count)];
        RandomQuest newQuest = Instantiate(selectedQuest);
        ConfigureQuest(newQuest);

        QuestUIManager ui = FindObjectOfType<QuestUIManager>();
        if (ui != null)
        {
            ui.ShowQuestOffer(newQuest);
        }
    }

    /// <summary>
    /// 퀘스트의 목표와 설명, 보상 수치를 무작위로 구성합니다.
    /// </summary>
    /// <param name="quest">설정 대상 퀘스트</param>
    private void ConfigureQuest(RandomQuest quest)
    {
        quest.rewards.Clear();

        foreach (var objective in quest.objectives)
        {
            switch (objective.objectiveType)
            {
                case RandomQuest.QuestObjectiveType.ItemTransport:
                    var itemT = GetRandomItem();
                    if (itemT != null)
                    {
                        objective.targetId = itemT.id.ToString();
                        objective.requiredAmount = Random.Range(1, itemT.capacity + 1);
                        objective.destinationPlanetId = GetRandomPlanetId();
                        objective.description = $"'{itemT.itemName}' {objective.requiredAmount} transport to {objective.destinationPlanetId}.";

                        float reward = itemT.costMax * 1.1f * objective.requiredAmount;
                        quest.rewards.Add(new RandomQuest.QuestReward { amount = Mathf.RoundToInt(reward) });
                    }
                    break;

                case RandomQuest.QuestObjectiveType.ItemProcurement:
                    var itemP = GetRandomItem();
                    if (itemP != null)
                    {
                        objective.targetId = itemP.id.ToString();
                        objective.requiredAmount = Random.Range(1, itemP.capacity + 1);
                        objective.destinationPlanetId = "UNKNOWN";
                        objective.description = $"Get '{itemP.itemName}' {objective.requiredAmount} and procurement to planet.";

                        float reward =  itemP.costMax  * 1.1f * objective.requiredAmount;
                        quest.rewards.Add(new RandomQuest.QuestReward { amount = Mathf.RoundToInt(reward) });
                    }
                    break;

                case RandomQuest.QuestObjectiveType.CrewTransport:
                    objective.killCount = Random.Range(1, 5);
                    objective.destinationPlanetId = GetRandomPlanetId();
                    objective.description = $"Crew {objective.killCount} need to Transport to {objective.destinationPlanetId}.";

                    int crewReward = objective.killCount * 300;
                    quest.rewards.Add(new RandomQuest.QuestReward { amount = crewReward });
                    break;

                case RandomQuest.QuestObjectiveType.PirateHunt:
                    objective.killCount = Random.Range(5, 21);
                    objective.description = $"Kill pirate {objective.killCount}.";

                    int pirateReward = objective.killCount * 100;
                    quest.rewards.Add(new RandomQuest.QuestReward { amount = pirateReward });
                    break;
            }
        }

        /// <summary>
        /// 퀘스트의 대표 설명을 마지막 objective의 설명으로 설정합니다.
        /// </summary>
        if (quest.objectives.Count > 0)
        {
            quest.description = quest.objectives[^1].description;
        }
    }

    /// <summary>
    /// 아이템 데이터베이스에서 무작위 아이템을 하나 선택합니다.
    /// </summary>
    /// <returns>선택된 TradingItemData 객체</returns>
    private TradingItemData GetRandomItem()
    {
        if (itemDatabase == null || itemDatabase.allItems.Count == 0)
        {
            Debug.LogWarning("아이템 데이터베이스가 비어 있습니다.");
            return null;
        }

        return itemDatabase.allItems[Random.Range(0, itemDatabase.allItems.Count)];
    }

    /// <summary>
    /// 행성 ID 목록 중 무작위 행성 ID를 반환합니다.
    /// </summary>
    /// <returns>무작위로 선택된 행성 ID 문자열</returns>
    private string GetRandomPlanetId()
    {
        if (planetIds == null || planetIds.Count == 0)
        {
            Debug.LogWarning("행성 ID 목록이 비어 있습니다.");
            return "UNKNOWN";
        }

        return planetIds[Random.Range(0, planetIds.Count)];
    }
}
