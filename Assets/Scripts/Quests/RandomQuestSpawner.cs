using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 랜덤 퀘스트를 생성하고 QuestManager에 등록하는 클래스입니다.
/// </summary>
public class RandomQuestSpawner : MonoBehaviour
{
    /// <summary>사용할 랜덤 퀘스트 템플릿 리스트 (4종류)</summary>
    [Header("사용할 랜덤 퀘스트 원본들 (4개)")] public List<RandomQuest> questTemplates;


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
    }

    /// <summary>
    /// 퀘스트의 목표와 설명, 보상 수치를 무작위로 구성합니다.
    /// </summary>
    /// <param name="quest">설정 대상 퀘스트</param>
    private void ConfigureQuest(RandomQuest quest)
    {
        quest.rewards.Clear();

        foreach (QuestObjective objective in quest.objectives)
            switch (objective.objectiveType)
            {
                case QuestObjectiveType.ItemTransport:
                    TradingItemData itemT = GetRandomItem();
                    if (itemT != null)
                    {
                        objective.targetId = itemT.id;
                        objective.amount = Random.Range(1, itemT.capacity + 1);
                        objective.targetPlanetData = GetRandomPlanetData();
                        objective.description =
                            "ui.quest.objective.itemtransport".Localize(itemT.itemName.Localize(), objective.amount,
                                objective.targetPlanetData.planetName);
                        //$"'{itemT.itemName}'을/를 {objective.amount}개 확보하여 {objective.targetPlanetData.planetName}로 수송하세요.";

                        float reward = itemT.costMax * 1.1f * objective.amount;
                        quest.rewards.Add(new QuestReward { amount = Mathf.RoundToInt(reward) });
                    }

                    break;

                case QuestObjectiveType.ItemProcurement:
                    TradingItemData itemP = GetRandomItem();
                    if (itemP != null)
                    {
                        objective.targetId = itemP.id;
                        objective.amount = Random.Range(1, itemP.capacity + 1);
                        objective.targetPlanetData =
                            GameManager.Instance.PlanetDataList[GameManager.Instance.CurrentWarpTargetPlanetId];
                        objective.description =
                            "ui.quest.objective.itemprocurement".Localize(itemP.itemName.Localize(), objective.amount);
                        //$"'{itemP.itemName}'을/를 {objective.amount}개 확보하여 행성으로 조달하세요.";

                        float reward = itemP.costMax * 1.1f * objective.amount;
                        quest.rewards.Add(new QuestReward { amount = Mathf.RoundToInt(reward) });
                    }

                    break;

                case QuestObjectiveType.CrewTransport:
                    objective.amount = Random.Range(1, 5);
                    objective.targetPlanetData = GetRandomPlanetData();
                    objective.description =
                        "ui.quest.objective.crewtransport".Localize(objective.amount, objective.targetPlanetData);
                    //$"선원 {objective.amount}명을 {objective.targetPlanetData.planetName}로 수송하세요.";

                    int crewReward = objective.amount * 300;
                    quest.rewards.Add(new QuestReward { amount = crewReward });
                    break;

                case QuestObjectiveType.PirateHunt:
                    objective.amount = Random.Range(5, 21);
                    objective.targetPlanetData = GetRandomPlanetData(); // 추후 수락한 행성 넣을 예정
                    objective.description =
                        "ui.quest.objective.piratehunt".Localize(objective.amount);
                    //$"해적 {objective.amount}명을 죽이세요.";

                    int pirateReward = objective.amount * 100;
                    quest.rewards.Add(new QuestReward { amount = pirateReward });
                    break;
            }

        if (quest.objectives.Count > 0) quest.description = quest.objectives[^1].description;
    }

    /// <summary>
    /// 아이템 데이터베이스에서 무작위 아이템을 하나 선택합니다.
    /// </summary>
    /// <returns>선택된 TradingItemData 객체</returns>
    private TradingItemData GetRandomItem()
    {
        return null;
    }

    /// <summary>
    /// 행성 ID 목록 중 무작위 행성 ID를 반환합니다.
    /// </summary>
    /// <returns>무작위로 선택된 행성 ID 문자열</returns>
    private PlanetData GetRandomPlanetData()
    {
        if (GameManager.Instance.PlanetDataList == null || GameManager.Instance.PlanetDataList.Count == 0)
        {
            Debug.LogError("행성 목록이 비어있음");
            return null;
        }

        int index = Random.Range(0, GameManager.Instance.PlanetDataList.Count);
        return GameManager.Instance.PlanetDataList[index];
    }
}
