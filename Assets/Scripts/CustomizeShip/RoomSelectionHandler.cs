using UnityEngine;
using UnityEngine.UI;

public class RoomSelectionHandler : MonoBehaviour
{
    public static RoomSelectionHandler Instance;

    public GameObject selectionUI;
    public Button confirmButton;
    public Button cancelButton;
    public GridPlacer gridPlacer;

    private BlueprintRoom selectedRoom;

    private void Awake()
    {
        Instance = this;
        selectionUI.SetActive(false);
    }

    public void SelectRoom(BlueprintRoom room)
    {
        if (selectedRoom == room) return;

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
