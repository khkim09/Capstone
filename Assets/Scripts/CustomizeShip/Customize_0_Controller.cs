using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Customize_0_Controller : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject customize0Panel;
    [SerializeField] private GameObject customize1Panel;
    [SerializeField] private GameObject customize2Panel;

    [Header("Fields")]
    [SerializeField] private Button exitButton;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button editButton;
    [SerializeField] private BPPreviewArea bpPreviewArea;

    [SerializeField] private Button slot1Button;
    [SerializeField] private Button slot2Button;
    [SerializeField] private Button slot3Button;
    [SerializeField] private Button slot4Button;

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
    }

    /// <summary>
    /// TODO : Idle scene으로 전환 구현 필요!!
    /// </summary>
    public void OnClickExit()
    {
        // scene 전환
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
        Customize_1_Controller controller = customize1Panel.GetComponent<Customize_1_Controller>();
        controller.ReloadBlueprintFromSlot(BlueprintSlotManager.Instance.currentSlotIndex);
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

        if (BlueprintSlotManager.Instance.GetBlueprintAt(index) == null)
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

        BlueprintSaveData selectedData = BlueprintSlotManager.Instance.GetBlueprintAt(slotIndex);
        if (selectedData != null)
        {
            Debug.LogError($"도안 {slotIndex}: 방 {selectedData.rooms.Count}, 무기 {selectedData.weapons.Count}");
            bpPreviewArea.Show(selectedData); // 도안 미리보기 표시

            CheckApplyAvailable();
        }
        else
        {
            bpPreviewArea.Clear(); // 미리보기 제거
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
