using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Customize_0_Controller : MonoBehaviour
{
    [Header("Ship")] [SerializeField] private Ship playerShip;
    [SerializeField] private BlueprintShip bpShip;

    [Header("UI Panels")] [SerializeField] private GameObject customize0Panel;
    [SerializeField] private GameObject customize1Panel;
    [SerializeField] private GameObject customize2Panel;

    [Header("Fields")] [SerializeField] private Button exitButton;
    [SerializeField] public Button applyButton;
    [SerializeField] private Button editButton;
    [SerializeField] public BPPreviewArea bpPreviewArea;

    [SerializeField] private Button slot1Button;
    [SerializeField] private Button slot2Button;
    [SerializeField] private Button slot3Button;
    [SerializeField] private Button slot4Button;

    [Header("Applied Tags")] [SerializeField]
    private List<Image> appliedTags = new();

    public List<Button> slotButtons = new();

    /// <summary>
    /// button 클릭 함수 연결
    /// </summary>
    private void Start()
    {
        exitButton.onClick.AddListener(() => { OnClickExit(); });
        applyButton.onClick.AddListener(() => { OnClickApply(); });
        editButton.onClick.AddListener(() => { OnClickEdit(); });

        slot1Button.onClick.AddListener(() => { OnClickSlot(0); });
        slot2Button.onClick.AddListener(() => { OnClickSlot(1); });
        slot3Button.onClick.AddListener(() => { OnClickSlot(2); });
        slot4Button.onClick.AddListener(() => { OnClickSlot(3); });

        applyButton.interactable = false;
        editButton.interactable = false;

        playerShip = GameManager.Instance.playerShip;
        // RTSSelectionManager.Instance.playerShip = GameManager.Instance.playerShip;
        playerShip.SetShipContentsActive(false);

        if (BlueprintSlotManager.Instance != null)
            BlueprintSlotManager.Instance.RegisterCustomizePanel(gameObject);
    }

    /// <summary>
    /// 적용 중인 도안 표시
    /// </summary>
    private void OnEnable()
    {
        // playerShip 내 모든 오브젝트 비활성화
        if (playerShip == null) playerShip = GameManager.Instance.playerShip;
        // RTSSelectionManager.Instance.playerShip = GameManager.Instance.playerShip;
        playerShip.SetShipContentsActive(false);

        for (int i = 0; i < appliedTags.Count; i++)
        {
            appliedTags[i].enabled = false;

            TextMeshProUGUI tmp = appliedTags[i].GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
                tmp.enabled = false;
        }

        int index = 0;
        if (BlueprintSlotManager.Instance != null)
            index = BlueprintSlotManager.Instance.appliedSlotIndex;

        if (index >= 0 && index < appliedTags.Count)
        {
            appliedTags[index].enabled = true;
            TextMeshProUGUI tmp = appliedTags[index].GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
                tmp.enabled = true;
        }
    }

    /// <summary>
    /// TODO : Planet scene으로 전환 구현 필요!!
    /// </summary>
    public void OnClickExit()
    {
        BlueprintSlotManager.Instance.SaveAllBlueprints();

        // scene 전환
        Camera mainCam = Camera.main;
        mainCam.transform.position = new Vector3(-100f, -100f, mainCam.transform.position.z);
        playerShip.SetShipContentsActive(true);

        playerShip.RemoveAllCrews();
        playerShip.RemoveAllRooms();
        playerShip.RemoveAllWeapons();
        playerShip.RemoveAllItems();
        // GameManager.Instance.LoadPlayerData();

        // 로딩 코루틴 실행
        StartCoroutine(DelayedLoadShipAndScene());
    }

    private IEnumerator DelayedLoadShipAndScene()
    {
        // 1초 대기 (혹은 필요한 만큼 조정)
        yield return new WaitForSeconds(1f);

        if (ES3.FileExists("playerShip"))
        {
            Debug.Log("소환시도");

            ShipSerialization.LoadShip("playerShip");
        }

        RTSSelectionManager.Instance.RefreshMovementData();

        SceneChanger.Instance.LoadScene("Planet");
    }

    /// <summary>
    /// 함선 교체 버튼 (도안 실제 적용시키겠다)
    /// </summary>
    public void OnClickApply()
    {
        customize0Panel.SetActive(false);
        customize2Panel.SetActive(true);
    }

    /// <summary>
    /// 도안 편집
    /// </summary>
    public void OnClickEdit()
    {
        customize0Panel.SetActive(false);
        customize1Panel.SetActive(true);

        // Customize_1_Controller로 슬롯 정보 넘기기 (슬롯 번호는 BlueprintSlotManager.Instance.currentSlotIndex로 저장됨)
        Customize_1_Controller controller1 = customize1Panel.GetComponent<Customize_1_Controller>();
        controller1.ReloadBlueprintFromSlot(BlueprintSlotManager.Instance.currentSlotIndex);
    }

    /// <summary>
    /// 도안 유효성에 따른 slot button 갱신
    /// </summary>
    /// <param name="index"></param>
    /// <param name="isValid"></param>
    public void UpdateSlotButtonColor(int index, bool isValid)
    {
        if (index < 0 || index >= slotButtons.Count)
            return;

        Button btn = slotButtons[index];
        ColorBlock cb = btn.colors;

        BlueprintSaveData data = BlueprintSlotManager.Instance.GetBlueprintAt(index);
        if (data == null || (data.rooms.Count == 0 && data.weapons.Count == 0))
            cb.normalColor = Color.gray;
        else if (!isValid)
            cb.normalColor = Color.red;
        else
            cb.normalColor = Color.green;

        btn.colors = cb;
    }

    /// <summary>
    /// 슬롯 버튼 클릭
    /// </summary>
    /// <param name="slotIndex"></param>
    public void OnClickSlot(int slotIndex)
    {
        editButton.interactable = true;

        BlueprintSlotManager.Instance.currentSlotIndex = slotIndex;
        Customize_2_Controller controller2 = customize2Panel.GetComponent<Customize_2_Controller>();

        BlueprintSaveData selectedData = BlueprintSlotManager.Instance.GetBlueprintAt(slotIndex);

        // 해당 슬롯에 도안에 뭔가 있음
        if (selectedData != null && (selectedData.rooms.Count > 0 || selectedData.weapons.Count > 0))
        {
            // 외갑판 preview도 슬롯 별로
            if (selectedData.hullLevel >= 0)
            {
                Debug.Log($"controller0 도안에 뭐 있고 외갑판 레벨 0이상 : {selectedData.hullLevel}");
                bpShip = controller2.MakeBPShipWithSaveData();
                bpShip.SetBPHullLevel(selectedData.hullLevel, controller2.previewOuterHullPrefab);
            }
            else
            {
                Debug.Log($"cont0 도안에 뭐 있지만 외갑판 세팅한 적 없음 : {selectedData.hullLevel}");
                bpShip = controller2.MakeBPShipWithSaveData();
                bpShip.SetBPHullLevel(-1, controller2.previewOuterHullPrefab);
            }

            bpPreviewArea.UpdateAndShow(selectedData); // 도안 미리보기 표시

            // 실제 함선으로 교체 가능한지 체크
            CheckApplyAvailable();
        }
        else
        {
            // 외갑판 삭제
            bpShip.ClearRooms();
            bpShip.ClearPreviewOuterHulls();
            bpShip.SetBPHullLevel(0, controller2.previewOuterHullPrefab);

            // 도안 미리보기 제거
            bpPreviewArea.Clear();
            applyButton.interactable = false;
        }
    }

    /// <summary>
    /// 실제 함선으로 교체 버튼 활성화 체크
    /// </summary>
    private void CheckApplyAvailable()
    {
        int index = BlueprintSlotManager.Instance.currentSlotIndex;

        if (index < 0)
            return;

        if (!BlueprintSlotManager.Instance.isValidBP[index])
            applyButton.interactable = false;
        else
            applyButton.interactable = true;
    }
}
