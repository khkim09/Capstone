using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUIManager : MonoBehaviour
{
    public GameObject offerPanel;
    public TextMeshProUGUI questText;
    public Button acceptBtn;
    public Button declineBtn;

    public GameObject completePanel;
    public TextMeshProUGUI completeText;
    public Button completeBtn;

    private RandomQuest currentQuest;

    private void Start()
    {
        acceptBtn.onClick.AddListener(OnAccept);
        declineBtn.onClick.AddListener(OnDecline);
        completeBtn.onClick.AddListener(OnCompleteConfirmed);
    }

    public void ShowQuestOffer(RandomQuest quest)
    {
        currentQuest = quest;
        offerPanel.SetActive(true);
        questText.text = ""; // 초기화
        StopAllCoroutines();
        StartCoroutine(TypeText(quest.title + "\n\n" + quest.description));
    }

    private System.Collections.IEnumerator TypeText(string text)
    {
        questText.text = "";
        foreach (char c in text)
        {
            questText.text += c;
            yield return new WaitForSeconds(0.015f);
        }
    }

    private void OnAccept()
    {
        if (currentQuest != null)
        {
            currentQuest.Accept();
            QuestManager.Instance.AddQuest(currentQuest.ToQuest());
        }

        offerPanel.SetActive(false);
        currentQuest = null;
    }

    private void OnDecline()
    {
        if (currentQuest != null)
        {
            currentQuest.Decline();
        }

        offerPanel.SetActive(false);
        currentQuest = null;
    }

    public void ShowCompletion(RandomQuest quest)
    {
        completePanel.SetActive(true);
        completeText.text = $"퀘스트 완료!\n보상: {quest.rewards[0].amount} COMA";
    }

    private void OnCompleteConfirmed()
    {
        completePanel.SetActive(false);
    }
}
