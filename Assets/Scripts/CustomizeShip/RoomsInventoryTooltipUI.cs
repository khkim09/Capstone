using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// RoomInventoryTooltip 의 Scroll View 관리하며,
/// Ship.cs의 allRooms에 저장된 보유 방들을 기반으로 UI를 동적으로 생성하고 Content 크기를 조정합니다.
/// </summary>
public class RoomsInventoryTooltipUI : MonoBehaviour
{
    /// <summary>
    /// 싱글턴 인스턴스입니다.
    /// </summary>
    public static RoomsInventoryTooltipUI Instance;

    /// <summary>
    /// 드래그 가능한 방 UI 프리팹입니다.
    /// </summary>
    [Header("Inventory References")] public GameObject roomButtonPrefab;

    /// <summary>
    /// ScrollView 내부 Content 트랜스폼입니다.
    /// </summary>
    public Transform contentRoot;               // ScrollView > Viewport > Content

    /// <summary>
    /// 현재 작업 중인 플레이어의 Ship 객체입니다.
    /// </summary>
    public Ship playerShip;                     // 현재 플레이어가 작업 중인 Ship

    /// <summary>
    /// 구매한 모든 방 데이터를 저장하는 리스트입니다.
    /// </summary>
    public List<RoomData> allOwnedRoomData = new List<RoomData>(); // 구매한 전체 방 data list

    /// <summary>
    /// 현재 설치된 방 데이터 리스트입니다.
    /// </summary>
    public List<RoomData> installedRoomData = new List<RoomData>(); // 유저가 설치한 방 data list

    /// <summary>
    /// 방 하나당 버튼의 높이(px)입니다.
    /// </summary>
    private const float unitSize = 40f;         // 타일 1칸에 해당하는 버튼 높이(px)

    /// <summary>
    /// 버튼 간 세로 간격(px)입니다.
    /// </summary>
    private const float verticalSpacing = 20f;  // VerticalLayoutGroup의 spacing

    /// <summary>
    /// 시작 시 RoomChanged 이벤트에 인벤토리 갱신 함수를 연결합니다.
    /// </summary>
    private void Start()
    {
        if (playerShip != null)
            playerShip.OnRoomChanged += RefreshInventory;
    }

    /// <summary>
    /// 인벤토리를 갱신하여 현재 보유한 방들을 UI로 표시합니다.
    /// 방 설치, 해제 완료 시 갱신 추가 필요 (구현 필요)
    /// </summary>
    public void RefreshInventory()
    {
        if (playerShip == null || playerShip.allRooms == null)
        {
            Debug.LogWarning("Ship or room list is null");
            return;
        }

        // 현재 설치된 항목 불러오기
        installedRoomData = playerShip.GetInstalledRoomDataList(); // room data는 crewquartersroomdata 와 같은 scriptable object를 가져오는 작업
        Debug.Log($"설치된 방 개수 : {installedRoomData.Count}");

        // 디버깅 (scriptable object 가져오는 것 확인 완료)
        foreach (RoomData data in installedRoomData)
        {
            Debug.Log($"{data}");
        }

        // 무슨 말이냐면 getinstalledroomdatalist()하면 방 타입에 따른 그 방의 initialize 정보만 가져오기 때문에, (scriptable object)
        // 타입은 같지만, 다른 위치에 설치된 방 2개에 대해서 동일하다고 판단함. (-> room으로 가져와야 될 듯)
        // ui 개편해서 하면 roomdata로 가져오는게 오히려 이득일듯 ? -> 인벤토리 UI확장 idea.txt 확인

        // 기존 항목 제거
        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        float totalHeight = 0f;

        /* 수정 전
                foreach (Room room in playerShip.allRooms)
                {
                    Debug.Log($"{room.roomType}");
                    RoomData data = room.roomData;
                    var roomLevelData = data.GetRoomData(room.GetCurrentLevel());
                    if (roomLevelData == null)
                        continue;

                    GameObject btn = Instantiate(roomButtonPrefab, contentRoot); // button prefab이라 draghandler가 붙질 않는듯

                    // 크기 조정
                    RectTransform rt = btn.GetComponent<RectTransform>();
                    rt.sizeDelta = new Vector2(roomLevelData.size.x * unitSize, roomLevelData.size.y * unitSize); // room 크기 만큼

                    // 이미지 설정
                    Image img = btn.GetComponent<Image>();
                    img.sprite = roomLevelData.roomSprite;

                    // 드래그 설정
                    RoomDragHandler dragHandler = btn.GetComponent<RoomDragHandler>();
                    dragHandler.InitializeFromRoomData(data, room.GetCurrentLevel());

                    totalHeight += rt.sizeDelta.y + verticalSpacing;
                }
        */

        // 수정 후
        foreach (RoomData roomData in allOwnedRoomData)
        {
            // 설치된 방이면 스킵
            //if (installedRoomData.Contains(roomData))
            //    continue;

            var levelData = roomData.GetRoomData(1);
            GameObject btn = Instantiate(roomButtonPrefab, contentRoot);

            // 크기, 이미지 설정
            RectTransform rt = btn.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(levelData.size.x * unitSize, levelData.size.y * unitSize);
            btn.GetComponent<Image>().sprite = levelData.roomSprite;

            RoomDragHandler dragHandler = btn.GetComponent<RoomDragHandler>();
            dragHandler.InitializeFromRoomData(roomData, 1);

            totalHeight += rt.sizeDelta.y + verticalSpacing;
        }

        // Content의 최소 높이 보장
        float finalHeight = Mathf.Max(500f, totalHeight);
        contentRoot.GetComponent<RectTransform>().sizeDelta = new Vector2(contentRoot.GetComponent<RectTransform>().sizeDelta.x, finalHeight);
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot.GetComponent<RectTransform>());
    }

    /// <summary>
    /// 새롭게 획득한 방을 인벤토리에 추가하고 UI를 갱신합니다.
    /// </summary>
    /// <param name="data">추가할 방의 RoomData.</param>
    public void AddRoom(RoomData data)
    {
        if (!allOwnedRoomData.Contains(data))
            allOwnedRoomData.Add(data);
        RefreshInventory();
    }
}
