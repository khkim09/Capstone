using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EmployController : MonoBehaviour
{
    [Header("Crew Card Prefabs")]
    public GameObject[] crewCardPrefabs;

    [Header("UI Panels")]
    public Transform employPanel;
    public GameObject purchaseConfirmPanel;
    public Transform crewCardPlace;
    public TMP_InputField nameInputField;
    public TMP_Text currencyText;

    [Header("Button")]
    [SerializeField] private Button exitButton;
    public Button buyButton;
    public Button cancelButton;

    [SerializeField] int playerCurrency;

    private GameObject selectedCardInstance;
    private CrewRace selectedRace;
    private string selectedRandomName;

    private void Start()
    {
        exitButton.onClick.AddListener(() => { OnClickExit(); });
        buyButton.onClick.AddListener(() => { OnClickBuy(); });
        cancelButton.onClick.AddListener(() => { OnClickCancel(); });

        PopulateRandomCrewCards();
    }

    /// <summary>
    /// crewcard 랜덤 3개 배치
    /// </summary>
    public void PopulateRandomCrewCards()
    {
        foreach (Transform child in employPanel)
            Destroy(child.gameObject);

        List<int> pool = new List<int> { 0, 1, 2, 3, 4, 5 }; // prefab 인덱스

        // Tank/Sup 동시에 못 나오게 제한
        int mechTankIndex = 2, mechSupIndex = 3;
        if (Random.value < 0.5f)
            pool.Remove(mechSupIndex);
        else
            pool.Remove(mechTankIndex);

        List<int> chosen = new();
        while (chosen.Count < 3)
        {
            int rand = pool[Random.Range(0, pool.Count)];
            if (!chosen.Contains(rand)) chosen.Add(rand);
        }

        foreach (int index in chosen)
        {
            GameObject card = Instantiate(crewCardPrefabs[index], employPanel);
            CrewRace race = (CrewRace)(index + 1);

            string randomName = GameObjectFactory.Instance.CrewFactory.GenerateRandomName(race);
            CrewCardUI crewCardUI = card.GetComponent<CrewCardUI>();
            crewCardUI.SetCrewName(randomName);

            // 로컬 복사
            GameObject cardCopy = card;
            CrewRace raceCopy = race;
            string nameCopy = randomName;

            Button cardBtn = cardCopy.GetComponent<Button>();
            cardBtn.onClick.AddListener(() => OnClickCrewCard(cardCopy, raceCopy, nameCopy));
        }
    }

    private void OnClickCrewCard(GameObject card, CrewRace race, string name)
    {
        selectedCardInstance = card;
        selectedRace = race;
        selectedRandomName = name;
        playerCurrency = ResourceManager.Instance.COMA;

        // UI 채우기
        foreach (Transform child in crewCardPlace)
            Destroy(child.gameObject);

        // crew card place에 띄우기 (단일 구매 창)
        GameObject copied = Instantiate(card, crewCardPlace);
        RectTransform rt = copied.GetComponent<RectTransform>();

        rt.SetParent(crewCardPlace, false);
        rt.anchoredPosition = new Vector2(250f, 450f);
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one;

        nameInputField.text = "";
        currencyText.text = $"Cur : {playerCurrency} COMA";

        purchaseConfirmPanel.SetActive(true);

        if (playerCurrency < 1500)
            buyButton.interactable = false;
        else
            buyButton.interactable = true;
    }

    private void OnClickBuy()
    {
        string finalName = string.IsNullOrWhiteSpace(nameInputField.text) ? selectedRandomName : nameInputField.text;

        CrewBase newCrew = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(selectedRace, finalName);
        GameManager.Instance.playerShip.AddCrew(newCrew);

        playerCurrency -= 1500;
        purchaseConfirmPanel.SetActive(false);

        // 구매한 선원 카드 제거
        if (selectedCardInstance != null)
        {
            LayoutElement layout = selectedCardInstance.GetComponent<LayoutElement>();
            if (layout != null)
                layout.ignoreLayout = true;

            selectedCardInstance.SetActive(false);

            Canvas.ForceUpdateCanvases();

            Destroy(selectedCardInstance, 0.5f);
        }

        // 다시 새 카드 뽑기
        // PopulateRandomCrewCards();
    }

    private void OnClickCancel()
    {
        purchaseConfirmPanel.SetActive(false);
    }

    private void OnClickExit()
    {
        SceneChanger.Instance.LoadScene("Trade");
    }
}
