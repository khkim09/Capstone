using UnityEngine;
using UnityEngine.UI;

public class RoomSelectionHandler : MonoBehaviour
{
    /// <summary>
    /// 싱글톤 인스턴스, 방/무기 삭제 담당 handler
    /// </summary>
    public static RoomSelectionHandler Instance;

    /// <summary>
    /// 해당 위치에 오브젝트 확정, 삭제 물어보는 UI
    /// </summary>
    public GameObject selectionUI;

    /// <summary>
    /// 확정 버튼
    /// </summary>
    public Button confirmButton;

    /// <summary>
    /// 삭제 버튼
    /// </summary>
    public Button cancelButton;

    /// <summary>
    /// 그리드 타일 배치 작업을 위한 오브젝트
    /// </summary>
    public GridPlacer gridPlacer;

    /// <summary>
    /// 선택된 배치 가능 오브젝트
    /// </summary>
    private IBlueprintPlaceable selectedPlaceable;

    private void Awake()
    {
        Instance = this;
        selectionUI.SetActive(false);
    }

    /// <summary>
    /// 선택된 방에 대해 확정, 삭제할 UI 활성화, 색상 변경 (이전 버전과의 호환성 유지)
    /// </summary>
    /// <param name="room"></param>
    public void SelectRoom(BlueprintRoom room)
    {
        SelectPlaceable(room);
    }

    /// <summary>
    /// 선택된 배치 가능 오브젝트에 대해 확정, 삭제할 UI 활성화, 색상 변경
    /// </summary>
    /// <param name="placeable">배치 가능 오브젝트</param>
    public void SelectPlaceable(IBlueprintPlaceable placeable)
    {
        if (selectedPlaceable == placeable)
            return;

        Deselect();

        selectedPlaceable = placeable;
        selectionUI.SetActive(true);

        // 선택된 오브젝트의 위치에 따라 선택 UI 위치 조정
        MonoBehaviour placeableObj = (MonoBehaviour)placeable;

        // 오브젝트 위치 + 높이에 따른 오프셋
        Vector3 position = placeableObj.transform.position;
        Vector2Int size = Vector2Int.zero;

        if (placeable is BlueprintRoom room)
            size = room.bpRoomSize;
        else if (placeable is BlueprintWeapon)
            size = new Vector2Int(2, 1); // 무기 기본 크기

        position += Vector3.up * (size.y * 0.5f + 0.5f);
        selectionUI.transform.position = position;

        // 색상 변경
        SpriteRenderer sr = placeableObj.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
    }

    /// <summary>
    /// 선택 해제
    /// </summary>
    public void Deselect()
    {
        if (selectedPlaceable != null)
        {
            SpriteRenderer sr = ((MonoBehaviour)selectedPlaceable).GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = Color.white;
        }

        selectedPlaceable = null;
        selectionUI.SetActive(false);
    }

    /// <summary>
    /// 설계도 오브젝트 삭제
    /// </summary>
    public void ConfirmDelete()
    {
        if (selectedPlaceable != null)
        {
            gridPlacer.UnMarkObjectOccupied(selectedPlaceable);
            selectedPlaceable.GetBlueprintShip().RemovePlaceable(selectedPlaceable);
            Destroy(((MonoBehaviour)selectedPlaceable).gameObject);
        }

        Deselect();
    }
}
