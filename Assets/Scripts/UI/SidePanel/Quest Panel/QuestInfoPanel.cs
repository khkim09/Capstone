using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class QuestInfoPanel : TooltipPanelBase
{
    [SerializeField] private Image panelSprite;
    [SerializeField] private Sprite panelSpriteCompleted;
    [SerializeField] private Sprite panelSpriteOngoing;
    [SerializeField] private Image targetPlanetImage;

    [SerializeField] private TextMeshProUGUI targetPlanetName;

    [SerializeField] private TextMeshProUGUI questName;
    [SerializeField] private TextMeshProUGUI questDescription;
    [SerializeField] private TextMeshProUGUI questRewards;
    [SerializeField] private TextMeshProUGUI questDurationLeft;

    [SerializeField] private Button buttonCancel;
    public RandomQuest currentQuest;

    [SerializeField] private SlidePanelController slidePanelController;


    protected override void Start()
    {
        base.Start();

        slidePanelController = GameObject.FindWithTag("SlidePanel").GetComponent<SlidePanelController>();
    }

    public void Initialize(RandomQuest quest)
    {
        currentQuest = quest;
        questName.text = currentQuest.title.Localize();

        QuestObjective questObjective = currentQuest.objectives[0];
        string description = "";
        GameObjectFactory.Instance.ItemFactory.itemDataBase.GetItemData(questObjective.targetId).itemName.Localize();
        string itemName = "";
        switch (questObjective.objectiveType)
        {
            case QuestObjectiveType.PirateHunt:
                if (questObjective.amount - questObjective.currentAmount > 0)
                    description =
                        questObjective.description.Localize(questObjective.amount - questObjective.currentAmount);
                else
                    description = "ui.quest.objective.piratehuntcompleted".Localize(GameManager.Instance.PlanetDataList[questObjective.targetPlanetDataId].planetName);
                break;
            case QuestObjectiveType.ItemTransport:
                itemName = GameObjectFactory.Instance.ItemFactory.itemDataBase.GetItemData(questObjective.targetId)
                    .itemName.Localize();
                description = questObjective.description.Localize(itemName, questObjective.amount,
                    GameManager.Instance.PlanetDataList[questObjective.targetPlanetDataId].planetName);
                break;
            case QuestObjectiveType.ItemProcurement:
                itemName = GameObjectFactory.Instance.ItemFactory.itemDataBase.GetItemData(questObjective.targetId)
                    .itemName.Localize();
                description = questObjective.description.Localize(itemName, questObjective.amount);
                break;
            case QuestObjectiveType.CrewTransport:
                description = questObjective.description.Localize(questObjective.amount,
                    GameManager.Instance.PlanetDataList[questObjective.targetPlanetDataId].planetName);
                break;
        }

        questDescription.text = description;


        PlanetData targetPlanetData =
            GameManager.Instance.PlanetDataList[currentQuest.objectives[0].targetPlanetDataId];

        targetPlanetImage.sprite = targetPlanetData.currentSprite;
        targetPlanetName.text = targetPlanetData.planetName;

        if (quest.GetCanComplete())
            panelSprite.sprite = panelSpriteCompleted;
        else
            panelSprite.sprite = panelSpriteOngoing;

        // TODO : 나중가서 복수 보상 처리를 해야할 것. 일단은 인덱스 0 번을 사용
        QuestReward reward = currentQuest.rewards[0];
        questRewards.text = $"{reward.amount} {reward.questRewardType}";
        if (quest.status == QuestStatus.Active)
            questDurationLeft.text = $"{"ui.questinfo.duration".Localize()} : {quest.QuestDurationLeft}";
        else
            questDurationLeft.text = $"{"ui.questinfo.inactive".Localize()}";
    }

    public void OnQuestCancelButton()
    {
        PlanetData planetData = GameManager.Instance.PlanetDataList.Find(d => d.questList.Contains(currentQuest));
        if (planetData == null)
        {
            Debug.LogError("이 퀘스트를 가진 행성이 없습니다. 오류!");
            return;
        }

        slidePanelController.RequestQuestCancel(currentQuest);
    }


    // 부모 클래스의 추상 메서드 구현
    protected override void SetToolTipText()
    {
    }
}
