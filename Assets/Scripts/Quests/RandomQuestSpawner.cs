using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 랜덤 퀘스트를 생성하고 QuestManager에 등록하는 매니저
/// </summary>
public class RandomQuestSpawner : MonoBehaviour
{
    [Header("사용할 랜덤 퀘스트 원본들 (4개)")]
    public List<RandomQuest> questTemplates;

    [Header("아이템 데이터베이스")]
    public TradingItemDataBase itemDatabase;

    [Header("행성 ID 목록 (랜덤 행성 선택용)")]
    public List<string> planetIds = new List<string> { "SIS", "CCK", "ICM", "RCE", "KTL" };

    /// <summary>
    /// 퀘스트를 무작위로 선택하고, 실행 시점에 목표와 값을 설정하여 등록
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

        QuestManager.Instance.AddQuest(newQuest.ToQuest());
        QuestUIManager ui = FindObjectOfType<QuestUIManager>();
        if (ui != null)
        {
            ui.ShowQuestOffer(newQuest);
        }
    }

    /// <summary>
    /// 퀘스트의 목표값들을 무작위로 설정하고 설명도 작성
    /// </summary>
    private void ConfigureQuest(RandomQuest quest)
    {
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
                    }
                    break;

                case RandomQuest.QuestObjectiveType.ItemProcurement:
                    var itemP = GetRandomItem();
                    if (itemP != null)
                    {
                        objective.targetId = itemP.id.ToString();
                        objective.requiredAmount = Random.Range(1, itemP.capacity + 1);
                        objective.destinationPlanetId = "UNKNOWN"; // 또는 추후 acceptingPlanetId
                        objective.description = $"Get '{itemP.itemName}' {objective.requiredAmount} and procurement to planet.";

                    }
                    break;

                case RandomQuest.QuestObjectiveType.CrewTransport:
                    objective.killCount = Random.Range(1, 5); // 1~4명
                    objective.destinationPlanetId = GetRandomPlanetId();
                    objective.description = $"Crew {objective.killCount} need to Transport to {objective.destinationPlanetId}.";
                    break;

                case RandomQuest.QuestObjectiveType.PirateHunt:
                    objective.killCount = Random.Range(5, 21); // 5~20명
                    objective.description = $"Kill pirate {objective.killCount}.";
                    break;
            }
        }
        if (quest.objectives.Count > 0)
        {
            quest.description = quest.objectives[0].description;
        }
    }

    /// <summary>
    /// 아이템 데이터베이스에서 무작위 아이템 반환
    /// </summary>
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
    /// planetIds 리스트에서 랜덤하게 행성 ID 선택
    /// </summary>
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
