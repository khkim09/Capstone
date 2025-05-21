using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class QuestListItem : MonoBehaviour
{
    private RandomQuest questInfo;
    private Image backPanel;
    private void Start()
    {
        questInfo = GetComponent<RandomQuest>();
        backPanel=GetComponent<Image>();
        //todo:행성 나갈때 연결해제 필요
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
        UpdateItem();
    }

    public void UpdateItem()
    {
        switch (questInfo.status)
        {
            case QuestStatus.NotStarted:
                backPanel.sprite=transform.parent.GetComponent<QuestList>().availableSprite;
                ButtonActivate("Accept");
                break;
            case QuestStatus.Active:
                if (questInfo.GetCanComplete())
                {
                    backPanel.sprite=transform.parent.GetComponent<QuestList>().completableSprite;
                    ButtonActivate("Complete");
                }
                else
                {
                    backPanel.sprite = transform.parent.GetComponent<QuestList>().activeSprite;
                    ButtonActivate("Decline");
                }
                break;
        }
    }

    private void ButtonActivate(string whichButton)
    {
        transform.Find("Accept").gameObject.SetActive(false);
        transform.Find("Complete").gameObject.SetActive(false);
        transform.Find("Decline").gameObject.SetActive(false);

        transform.Find(whichButton).gameObject.SetActive(true);
    }
}
