using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class QuestListItem : MonoBehaviour
{
    public string questID;
    public RandomQuest questInfo;
    private Image backPanel;
    public QuestList questList;
    private void Start()
    {
    }

    public void OnAcceptButtonClicked()
    {
        QuestManager.Instance.AddQuest(questInfo);
        UpdateItem();
    }

    public void OnCompleteButtonClicked()
    {
        QuestManager.Instance.CompleteQuest(questInfo);
        Destroy(gameObject);
    }

    public void OnDeclineButtonClicked()
    {
        QuestManager.Instance.RequestFailQuest(questInfo);
        Destroy(gameObject);
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
                GetComponent<Image>().sprite=questList.GetComponent<QuestList>().availableSprite;
                ButtonActivate("Accept");
                break;
            case QuestStatus.Active:
                if (questInfo.GetCanComplete())
                {
                    GetComponent<Image>().sprite=questList.GetComponent<QuestList>().completableSprite;
                    ButtonActivate("Complete");
                }
                else
                {
                    GetComponent<Image>().sprite =questList.GetComponent<QuestList>().activeSprite;
                    ButtonActivate("Decline");
                }
                break;
        }
        title.text=questInfo.title.Localize();
        QuestObjective questObjective=questInfo.objectives[0];
        string questDescription="";
        GameObjectFactory.Instance.ItemFactory.itemDataBase.GetItemData(questObjective.targetId).itemName.Localize();
        string itemName="";
        switch (questObjective.objectiveType)
        {
            case QuestObjectiveType.PirateHunt:
                questDescription=questObjective.description.Localize(questObjective.amount);
                break;
            case QuestObjectiveType.ItemTransport:
                itemName = GameObjectFactory.Instance.ItemFactory.itemDataBase.GetItemData(questObjective.targetId).itemName.Localize();
                questDescription=questObjective.description.Localize(itemName, questObjective.amount,GameManager.Instance.PlanetDataList[questObjective.targetPlanetDataId].planetName);
                break;
            case QuestObjectiveType.ItemProcurement:
                itemName = GameObjectFactory.Instance.ItemFactory.itemDataBase.GetItemData(questObjective.targetId).itemName.Localize();
                questDescription=questObjective.description.Localize(itemName,questObjective.amount);
                break;
            case QuestObjectiveType.CrewTransport:
                questDescription=questObjective.description.Localize(questObjective.amount,GameManager.Instance.PlanetDataList[questObjective.targetPlanetDataId].planetName);
                break;
        }
        description.text=questDescription;
        PlanetData planetData = GameManager.Instance.PlanetDataList[questInfo.objectives[0].targetPlanetDataId];
        destination.text = planetData.planetName;
        reward.text = "ui.planet.quest.reward".Localize() +": "+ questInfo.rewards[0].amount +" COMA";
        transform.Find("PlanetSprite").GetComponent<Image>().sprite = GameManager.Instance.PlanetDataList[questObjective.targetPlanetDataId].currentSprite;
    }

    private void ButtonActivate(string whichButton)
    {
        transform.Find("Accept").gameObject.SetActive(false);
        transform.Find("Complete").gameObject.SetActive(false);
        transform.Find("Decline").gameObject.SetActive(false);

        transform.Find(whichButton).gameObject.SetActive(true);
    }
}
