using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestList : MonoBehaviour
{
    public GameObject questListItem;
    private PlanetData planetData;

    private List<RandomQuest> quests;

    public Sprite availableSprite;
    public Sprite activeSprite;
    public Sprite completableSprite;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        planetData = GameManager.Instance.WhereIAm();
        quests = planetData.questList;
        foreach (RandomQuest quest in quests)
        {
            AddQuestToListUI(quest);
        }
    }

    private void AddQuestToListUI(RandomQuest quest)
    {
        GameObject questItem = Instantiate(questListItem, transform,false);
        RandomQuest questInfo = questItem.GetComponent<RandomQuest>();
        questInfo = quest;

        switch (questInfo.status)
        {
            case QuestStatus.NotStarted:
                questItem.GetComponent<Image>().sprite = availableSprite;
                break;
            case QuestStatus.Active:
                if(questInfo.GetCanComplete())
                    questItem.GetComponent<Image>().sprite = completableSprite;
                else
                    questItem.GetComponent<Image>().sprite = activeSprite;
                break;
        }
    }

    public void UpdateList()
    {
        foreach (RandomQuest quest in quests)
        {
            AddQuestToListUI(quest);
        }
    }

    private void SetQuestStatus(RandomQuest quest, QuestStatus status)
    {
        quest.status = status;
        UpdateList();
    }

    public void AcceptQuest(string questId)
    {
        foreach (RandomQuest quest in quests)
        {
            if (quest.questId == questId)
            {
                SetQuestStatus(quest, QuestStatus.Active);
                return;
            }
        }
    }

}
