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
    [SerializeField] Button applyButton;
    [SerializeField] Button editButton;
    [SerializeField] BPPreviewArea bpPreviewArea;

    [SerializeField] Button slot1Button;
    [SerializeField] Button slot2Button;
    [SerializeField] Button slot3Button;
    [SerializeField] Button slot4Button;

    public List<Button> slotButtons = new();

    private void Start()
    {
        applyButton.onClick.AddListener(() => { OnClickApply(); });
        editButton.onClick.AddListener(() => { OnClickEdit(); });

        slot1Button.onClick.AddListener(() => { OnClickSlot(0); });
        slot2Button.onClick.AddListener(() => { OnClickSlot(1); });
        slot3Button.onClick.AddListener(() => { OnClickSlot(2); });
        slot4Button.onClick.AddListener(() => { OnClickSlot(3); });
    }

    public void OnClickApply()
    {
        customize0Panel.SetActive(false);
        customize2Panel.SetActive(true);
    }

    public void OnClickEdit()
    {
        customize0Panel.SetActive(false);
        customize1Panel.SetActive(true);

        // Customize_1_Controller로 슬롯 정보 넘기기 (슬롯 번호는 BlueprintSlotManager.Instance.currentSlotIndex로 저장됨)
        Customize_1_Controller controller = customize1Panel.GetComponent<Customize_1_Controller>();
        controller.ReloadBlueprintFromSlot(BlueprintSlotManager.Instance.currentSlotIndex);
    }

    public void UpdateSlotButtonColor(int index, bool isValid)
    {
        if (index < 0 || index >= slotButtons.Count)
            return;

        var btn = slotButtons[index];
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
        BlueprintSlotManager.Instance.currentSlotIndex = slotIndex;

        BlueprintSaveData selectedData = BlueprintSlotManager.Instance.GetBlueprintAt(slotIndex);
        if (selectedData != null)
        {
            Debug.LogError($"도안 {slotIndex}: 방 {selectedData.rooms.Count}, 무기 {selectedData.weapons.Count}");
            bpPreviewArea.Show(selectedData); // 도안 미리보기 표시
            applyButton.interactable = true;
            // editButton.interactable = true;
        }
        else
        {
            bpPreviewArea.Clear(); // 미리보기 제거
            applyButton.interactable = false;
            // editButton.interactable = false;
        }
    }
}
