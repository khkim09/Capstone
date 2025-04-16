using UnityEngine;
using UnityEngine.UI;

public class RoomSelectionHandler : MonoBehaviour
{
    /// <summary>
    /// 싱글톤 인스턴스, 방 삭제 담당 handler
    /// </summary>
    public static RoomSelectionHandler Instance;

    /// <summary>
    /// 해당 위치에 방 확정, 삭제 물어보는 UI
    /// </summary>
    public GameObject selectionUI;

    /// <summary>
    /// 확정 버튼튼
    /// </summary>
    public Button confirmButton;

    /// <summary>
    /// 삭제 버튼튼
    /// </summary>
    public Button cancelButton;

    /// <summary>
    /// 그리드 타일 배치 작업을 위한 오브젝트
    /// </summary>
    public GridPlacer gridPlacer;

    /// <summary>
    /// 선택된 방
    /// </summary>
    private BlueprintRoom selectedRoom;

    private void Awake()
    {
        Instance = this;
        selectionUI.SetActive(false);
    }

    /// <summary>
    /// 선택된 방에 대해 확정, 삭제할 UI 활성화, 색상 변경
    /// </summary>
    /// <param name="room"></param>
    public void SelectRoom(BlueprintRoom room)
    {
        if (selectedRoom == room)
            return;

        Deselect();

        selectedRoom = room;
        selectionUI.SetActive(true);
        selectionUI.transform.position = selectedRoom.transform.position + Vector3.up * (selectedRoom.bpRoomSize.y * 0.5f + 0.5f);

        // 색상 변경
        room.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
    }

    /// <summary>
    /// 선택 해제
    /// </summary>
    public void Deselect()
    {
        if (selectedRoom != null)
            selectedRoom.GetComponent<SpriteRenderer>().color = Color.white;

        selectedRoom = null;
        selectionUI.SetActive(false);
    }

    /// <summary>
    /// 설계도 방 삭제
    /// </summary>
    public void ConfirmDelete()
    {
        if (selectedRoom != null)
        {
            gridPlacer.UnMarkRoomOccupied(selectedRoom);
            selectedRoom.GetBlueprintShip().RemoveRoom(selectedRoom);
            Destroy(selectedRoom.gameObject);
        }

        Deselect();
    }
}
