using System.Collections.Generic;
using TMPro;
using UnityEngine;
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

        int currentPlanetId = GameManager.Instance.WhereIAm().planetId;

        // 행성 내 고용 창 최초 진입 - 랜덤 3장 생성 후 저장
        if (!GameManager.Instance.employCardsPerPlanet.ContainsKey(currentPlanetId))
            PopulateRandomCrewCardsAndSave(currentPlanetId);

        // 항상 현재 행성의 crewCard UI 표시
        PopulateCardsFromSavedData(currentPlanetId);
    }

    private void PopulateRandomCrewCardsAndSave(int planetId)
    {
        List<int> pool = new List<int> { 0, 1, 2, 3, 4, 5 };

        // 기계형 두 개 동시 생성 불가
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

        List<CrewCardSaveData> saveData = new();
        foreach (int index in chosen)
        {
            CrewRace race = (CrewRace)(index + 1);
            string randomName = GameObjectFactory.Instance.CrewFactory.GenerateRandomName(race);
            saveData.Add(new CrewCardSaveData(race, randomName));
        }

        GameManager.Instance.employCardsPerPlanet[planetId] = saveData;
    }

    private void PopulateCardsFromSavedData(int planetId)
    {
        foreach (Transform child in employPanel)
            Destroy(child.gameObject);

        List<CrewCardSaveData> savedCards = GameManager.Instance.employCardsPerPlanet[planetId];

        foreach (CrewCardSaveData data in savedCards)
        {
            if (data.isPurchased)
            {
                GameObject placeholder = new GameObject("Placeholder", typeof(RectTransform), typeof(LayoutElement));
                placeholder.transform.SetParent(employPanel, false);

                RectTransform rt = placeholder.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(500f, 900f);

                LayoutElement le = placeholder.GetComponent<LayoutElement>();
                le.preferredWidth = 500f;
                le.preferredHeight = 900f;

                continue;
            }

            GameObject card = Instantiate(crewCardPrefabs[(int)data.race - 1], employPanel);
            CrewCardUI crewCardUI = card.GetComponent<CrewCardUI>();
            crewCardUI.SetCrewName(data.name);

            // 로컬 복사
            GameObject cardCopy = card;
            CrewRace raceCopy = data.race;
            string nameCopy = data.name;

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

        if (playerCurrency < 1500 || GameManager.Instance.playerShip.allCrews.Count >= GameManager.Instance.playerShip.GetStat(ShipStat.CrewCapacity))
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

        // data 갱신
        int currentPlanetId = GameManager.Instance.WhereIAm().planetId;
        List<CrewCardSaveData> cards = GameManager.Instance.employCardsPerPlanet[currentPlanetId];
        CrewCardSaveData target = cards.Find(c => c.race == selectedRace && c.name == selectedRandomName);
        if (target != null)
            target.isPurchased = true;

        // 구매한 선원 카드 제거
        if (selectedCardInstance != null)
        {
            int siblingIndex = selectedCardInstance.transform.GetSiblingIndex();

            // LayoutElement layout = selectedCardInstance.GetComponent<LayoutElement>();
            // if (layout != null)
            //     layout.ignoreLayout = true;

            selectedCardInstance.SetActive(false);

            // 선원 카드 자리에 placeholder 추가
            GameObject placeholder = new GameObject("Placeholder", typeof(RectTransform), typeof(LayoutElement));
            placeholder.transform.SetParent(employPanel, false);

            RectTransform rt = placeholder.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500f, 900f);

            LayoutElement le = placeholder.GetComponent<LayoutElement>();
            le.preferredWidth = 500f;
            le.preferredHeight = 900f;

            // 기존 카드 위치에 삽입
            placeholder.transform.SetSiblingIndex(siblingIndex);

            Destroy(selectedCardInstance, 0.5f);
        }
    }

    private void OnClickCancel()
    {
        purchaseConfirmPanel.SetActive(false);
    }

    private void OnClickExit()
    {
        SceneChanger.Instance.LoadScene("Planet");

    }
}
