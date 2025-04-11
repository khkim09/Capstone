using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 창고 타입의 기본 클래스.
/// 저장 가능한 아이템의 타입, 보관/제거/저하 로직 등을 공통으로 정의합니다.
/// </summary>
public abstract class StorageRoomBase : Room<StorageRoomBaseData, StorageRoomBaseData.StorageRoomBaseLevel>
{
    /// <summary>저장된 아이템 목록.</summary>
    protected List<TradingItem> storedItems = new();

    private int[,] storageGrid;


    /// <summary>
    /// 초기화 시 방 타입을 Storage로 설정합니다.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        roomType = RoomType.Storage;

        storageGrid = new int[GetSize().x, GetSize().y];

        // TODO: 테스트용 코드
        TradingItem tradingItem = ItemManager.Instance.CreateItemInstance(0, 1, new Vector2Int(1, 1));

        AddItem(tradingItem, new Vector2Int(1, 1), 0);
    }

    /// <summary>
    /// 이 방이 함선 스탯에 기여하는 값을 계산합니다.
    /// 작동 여부에 따라 전력 사용량을 기여합니다.
    /// </summary>
    public override Dictionary<ShipStat, float> GetStatContributions()
    {
        // 기본 기여도 가져오기 (작동 상태 체크 등)
        Dictionary<ShipStat, float> contributions = base.GetStatContributions();

        // 작동 상태가 아니면 기여도 없음
        if (!IsOperational() || currentRoomLevelData == null)
            return contributions;

        // 창고 레벨 데이터에서 기여도
        contributions[ShipStat.PowerUsing] = currentRoomLevelData.powerRequirement;

        return contributions;
    }

    /// <summary>
    /// 현재 창고의 저장 타입을 반환합니다.
    /// </summary>
    public StorageType GetStorageType()
    {
        RoomData data = GetRoomData();
        if (data is StorageRoomBaseData storageData)
        {
            StorageType storageType = storageData.storageType;
            return storageType;
        }

        return StorageType.Regular;
    }

    public void SetStorageType(StorageType storageType)
    {
        RoomData data = GetRoomData();
        if (data is StorageRoomBaseData storageData) storageData.storageType = storageType;
    }

    /// <summary>
    /// 해당 아이템을 저장할 수 있는지 여부를 반환합니다.
    /// 파생 클래스에서 구현합니다.
    /// </summary>
    public abstract bool CanStoreItemType(ItemCategory itemType);

    /// <summary>
    /// 아이템을 창고에 추가합니다.
    /// </summary>
    public virtual bool AddItem(TradingItem item, Vector2Int position, int rotation)
    {
        if (!CanPlaceItem(item, position, rotation))
            return false;

        // TODO: 자식으로 안 붙는 이유 수정, 방 안의 그리드 좌표에 생성되게 수정
        item.transform.SetParent(transform, false);

        return true;
    }

    /// <summary>
    /// 아이템을 창고에서 제거합니다.
    /// </summary>
    public virtual bool RemoveItem(TradingItem item, int quantity)
    {
        return true;
    }

    /// <summary>
    /// 워프 후 아이템 상태를 확인 및 갱신합니다.
    /// </summary>
    public virtual void CheckItemsAfterWarp()
    {
    }

    /// <summary>
    /// 아이템의 상태를 저하시킵니다.
    /// 아이템은 점점 더 손상된 상태로 변경됩니다.
    /// </summary>
    protected virtual void DegradeItemState(TradingItem item)
    {
        switch (item.itemState)
        {
            case ItemState.Normal:
                item.itemState = ItemState.SlightlyDamaged;
                break;
            case ItemState.SlightlyDamaged:
                item.itemState = ItemState.ModeratelyDamaged;
                break;
            case ItemState.ModeratelyDamaged:
                item.itemState = ItemState.Unsellable;
                break;
        }
    }

    /// <summary>
    /// 아이템이 점유하는 모든 타일의 좌표를 반환합니다.
    /// </summary>
    public List<Vector2Int> GetOccupiedTiles(TradingItem item, Vector2Int position, int rotation)
    {
        List<Vector2Int> occupiedTiles = new();

        // 아이템의 블록 형태 가져오기 (이미 회전된 형태)
        bool[][] blockShape = ItemShape.Instance.GetItemShape(item.GetItemData().id, rotation);

        // 각 점유된 블록에 대해 창고 좌표 계산
        for (int y = 0; y < 5; y++)
        for (int x = 0; x < 5; x++)
            if (blockShape[y][x])
            {
                // 아이템 블록 좌표를 창고 좌표로 변환 (중심점 2,2 기준)
                int storageX = position.x + (x - 2);
                int storageY = position.y + (y - 2);

                occupiedTiles.Add(new Vector2Int(storageX, storageY));
            }

        return occupiedTiles;
    }

    /// <summary>
    /// 아이템을 지정된 위치에 배치할 수 있는지 확인합니다.
    /// </summary>
    public bool CanPlaceItem(TradingItem item, Vector2Int position, int rotation)
    {
        // 아이템이 점유할 타일 계산
        List<Vector2Int> occupiedTiles = GetOccupiedTiles(item, position, rotation);

        // 창고 크기 (4x4)
        int storageWidth = GetSize().x;
        int storageHeight = GetSize().y;

        // 각 타일에 대해 유효성 검사
        foreach (Vector2Int tile in occupiedTiles)
        {
            // 1. 창고 경계를 벗어나는지 확인
            if (tile.x < 0 || tile.x >= storageWidth || tile.y < 0 || tile.y >= storageHeight)
                return false;

            // 2. 이미 다른 아이템이 점유하고 있는지 확인
            if (storageGrid[tile.y, tile.x] != 0)
                return false;
        }

        // 아이템 유형 확인 (예: 온도에 민감한 아이템은 온도 조절 창고만 사용 가능)
        if (!CanStoreItemType(item.GetItemData().type))
            return false;

        return true;
    }

    /// <summary>
    /// 아이템 배치 실행
    /// </summary>
    public bool PlaceItem(TradingItem item, Vector2Int position, int rotation)
    {
        if (!CanPlaceItem(item, position, rotation))
            return false;

        // 아이템이 점유할 타일 계산
        List<Vector2Int> occupiedTiles = GetOccupiedTiles(item, position, rotation);

        // 그리드에 아이템 ID 표시
        foreach (Vector2Int tile in occupiedTiles) storageGrid[tile.y, tile.x] = item.GetItemData().id;

        // 아이템 목록에 추가
        storedItems[item.GetItemData().id] = item;

        return true;
    }
}
