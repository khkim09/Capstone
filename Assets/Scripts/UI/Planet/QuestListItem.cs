using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class QuestListItem : MonoBehaviour
{
    public RandomQuest questInfo;
    private Image backPanel;
    public QuestList questList;

    [SerializeField] private Button buttonAccpet;
    [SerializeField] private Button buttonComplete;
    [SerializeField] private Button buttonDecline;

    private PlanetData currentPlanet;

    private void Start()
    {
        currentPlanet = GameManager.Instance.WhereIAm();
    }


    public void OnAcceptButtonClicked()
    {
        RandomQuest quest = currentPlanet.questList
            .FirstOrDefault(q => q == questInfo);

        if (quest != null) currentPlanet.ActivateQuest(quest);
        else Debug.Log("퀘스트가 왜 널이지");

        GameManager.Instance.CheckItemQuests();


        UpdateItem();
    }

    public void OnCompleteButtonClicked()
    {
        RandomQuest quest = currentPlanet.questList
            .FirstOrDefault(q => q == questInfo);

        if (quest != null) currentPlanet.CompleteQuest(quest);
        else Debug.Log("퀘스트가 왜 널이지");
        Destroy(gameObject);
        StartCoroutine(CheckItemQuestsDelayed());

        GameManager.Instance.playerData.questCleared++;

        UpdateItem();
    }

    public void OnDeclineButtonClicked()
    {
        RandomQuest quest = currentPlanet.questList
            .FirstOrDefault(q => q == questInfo);

        if (quest != null) currentPlanet.FailQuest(quest);
        else Debug.Log("퀘스트가 왜 널이지");
        Destroy(gameObject);
    }

    private System.Collections.IEnumerator CheckItemQuestsDelayed()
    {
        yield return new WaitForSeconds(1.0f);

        GameManager.Instance.CheckItemQuests();
        UpdateItem();
    }

    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public TextMeshProUGUI destination;
    public TextMeshProUGUI reward;

    public void UpdateItem()
    {
        switch (questInfo.status)
        {
            case QuestStatus.NotStarted:
                GetComponent<Image>().sprite = questList.GetComponent<QuestList>().availableSprite;
                ButtonActivate(buttonAccpet);
                break;
            case QuestStatus.Active:
                if (questInfo.GetCanComplete())
                {
                    GetComponent<Image>().sprite = questList.GetComponent<QuestList>().completableSprite;
                    ButtonActivate(buttonComplete);
                }
                else
                {
                    GetComponent<Image>().sprite = questList.GetComponent<QuestList>().activeSprite;
                    ButtonActivate(buttonDecline);
                }

                break;
        }

        title.text = questInfo.title.Localize();
        QuestObjective questObjective = questInfo.objectives[0];
        string questDescription = "";
        GameObjectFactory.Instance.ItemFactory.itemDataBase.GetItemData(questObjective.targetId).itemName.Localize();
        string itemName = "";
        switch (questObjective.objectiveType)
        {
            case QuestObjectiveType.PirateHunt:
                questDescription = questObjective.description.Localize(questObjective.amount);
                break;
            case QuestObjectiveType.ItemTransport:
                itemName = GameObjectFactory.Instance.ItemFactory.itemDataBase.GetItemData(questObjective.targetId)
                    .itemName.Localize();
                questDescription = questObjective.description.Localize(itemName, questObjective.amount,
                    GameManager.Instance.PlanetDataList[questObjective.targetPlanetDataId].planetName);
                break;
            case QuestObjectiveType.ItemProcurement:
                itemName = GameObjectFactory.Instance.ItemFactory.itemDataBase.GetItemData(questObjective.targetId)
                    .itemName.Localize();
                questDescription = questObjective.description.Localize(itemName, questObjective.amount);
                break;
            case QuestObjectiveType.CrewTransport:
                questDescription = questObjective.description.Localize(questObjective.amount,
                    GameManager.Instance.PlanetDataList[questObjective.targetPlanetDataId].planetName);
                break;
        }

        description.text = questDescription;
        PlanetData planetData = GameManager.Instance.PlanetDataList[questInfo.objectives[0].targetPlanetDataId];
        destination.text = planetData.planetName;
        reward.text = "ui.planet.quest.reward".Localize() + ": " + questInfo.rewards[0].amount + " COMA";
        transform.Find("PlanetSprite").GetComponent<Image>().sprite =
            GameManager.Instance.PlanetDataList[questObjective.targetPlanetDataId].currentSprite;
    }

    private void ButtonActivate(Button whichButton)
    {
        buttonAccpet.gameObject.SetActive(false);
        buttonComplete.gameObject.SetActive(false);
        buttonDecline.gameObject.SetActive(false);

        whichButton.gameObject.SetActive(true);
    }
}
