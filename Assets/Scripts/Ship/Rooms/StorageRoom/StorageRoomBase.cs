using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 모든 창고 타입의 기본 클래스.
/// 저장 가능한 아이템의 타입, 보관/제거/저하 로직 등을 공통으로 정의합니다.
/// </summary>
public abstract class StorageRoomBase : Room<StorageRoomBaseData, StorageRoomBaseData.StorageRoomBaseLevel>,
    IWorldGridSwitcher
{
    /// <summary>저장된 아이템 목록.</summary>
    public List<TradingItem> storedItems = new();

    // 그리드 셀에 저장된 아이템 인스턴스 참조를 저장하는 2D 배열
    private TradingItem[,] itemGrid;

    /// <summary>현재 선택된 아이템</summary>
    protected TradingItem selectedItem;

    /// <summary>방 크기</summary>
    private Vector2Int cachedSize;

    /// <summary>창고 콜라이더</summary>
    [SerializeField] protected BoxCollider2D storageCollider;

    /// <summary>
    /// 초기화 시 방 타입을 Storage로 설정합니다.
    /// </summary>
    protected override void Start()
    {
        base.Start();
        roomType = RoomType.Storage;

        // 캐시된 사이즈 초기화 및 그리드 생성
        cachedSize = GetSize();
        itemGrid = new TradingItem[cachedSize.y, cachedSize.x];

        // 콜라이더 초기화
        InitializeCollider();

        // TODO: 테스트용 코드
        TradingItem tradingItem = ItemManager.Instance.CreateItemInstance(2, 1);
        AddItem(tradingItem, new Vector2Int(1, 1), 0);
    }

    /// <summary>
    /// 창고 영역을 위한 콜라이더 초기화
    /// </summary>
    protected virtual void InitializeCollider()
    {
        // 콜라이더가 없으면 생성
        if (storageCollider == null) storageCollider = gameObject.AddComponent<BoxCollider2D>();

        // 방 크기에 맞게 콜라이더 설정
        Vector2Int size = GetSize();
        storageCollider.size = new Vector2(
            size.x * GridConstants.CELL_SIZE,
            size.y * GridConstants.CELL_SIZE
        );

        // 콜라이더가 트리거로 작동하도록 설정
        storageCollider.isTrigger = true;
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
    /// 아이템을 지정된 위치에 배치할 수 있는지 확인합니다.
    /// </summary>
    public bool CanPlaceItem(TradingItem item, Vector2Int position, RotationConstants.Rotation rotation)
    {
        // 아이템이 점유할 타일 계산
        List<Vector2Int> occupiedTiles = GetOccupiedTiles(item, position, (int)rotation);

        // 창고 크기
        Vector2Int storageSize = GetSize();

        // 각 타일에 대해 유효성 검사
        foreach (Vector2Int tile in occupiedTiles)
        {
            // 1. 창고 경계를 벗어나는지 확인
            if (tile.x < 0 || tile.x >= storageSize.x || tile.y < 0 || tile.y >= storageSize.y) return false;

            // 2. 이미 다른 아이템이 점유하고 있는지 확인
            // 그리드 위치에 있는 아이템이 현재 아이템과 다른 경우에만 충돌로 간주
            if (itemGrid[tile.y, tile.x] != null && itemGrid[tile.y, tile.x] != item) return false;
        }

        // 아이템 유형 확인
        if (!CanStoreItemType(item.GetItemData().type))
        {
            Debug.Log($"이 창고는 아이템 유형 {item.GetItemData().type}을 저장할 수 없음");
            return false;
        }

        // 모든 검사를 통과했으므로 아이템을 배치할 수 있음
        return true;
    }

    /// <summary>
    /// 아이템을 창고에 추가합니다.
    /// </summary>
    public virtual bool AddItem(TradingItem item, Vector2Int position, int rotation)
    {
        if (!CanPlaceItem(item, position, (RotationConstants.Rotation)rotation)) return false;

        // 아이템이 이미 다른 창고에 있다면 먼저 그쪽에서 제거
        StorageRoomBase currentStorage = null;
        foreach (StorageRoomBase storage in FindObjectsOfType<StorageRoomBase>())
            if (storage != this && storage.storedItems.Contains(item))
            {
                currentStorage = storage;
                break;
            }

        if (currentStorage != null)
        {
            bool removed = currentStorage.RemoveItem(item);
            if (!removed) return false;
        }

        item.Rotate((RotationConstants.Rotation)rotation);

        // 아이템을 방의 자식으로 설정
        item.transform.SetParent(transform, false);

        // 그리드 위치 저장
        item.gridPosition = position;

        // 아이템에 부모 창고 설정
        item.SetParentStorage(this);

        // 방 크기
        Vector2Int storageSize = GetSize();

        // 점유 타일 계산
        List<Vector2Int> occupiedTiles = GetOccupiedTiles(item, position, rotation);

        if (occupiedTiles.Count == 0)
        {
            Debug.LogError("AddItem: No occupied tiles found");
            return false;
        }

        // 아이템이 차지하는 영역의 경계 찾기
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        foreach (Vector2Int tile in occupiedTiles)
        {
            minX = Mathf.Min(minX, tile.x);
            maxX = Mathf.Max(maxX, tile.x);
            minY = Mathf.Min(minY, tile.y);
            maxY = Mathf.Max(maxY, tile.y);

            // 그리드에 아이템 참조 설정
            if (tile.x >= 0 && tile.x < storageSize.x && tile.y >= 0 && tile.y < storageSize.y)
                itemGrid[tile.y, tile.x] = item;
        }

        // 아이템 영역의 중앙 계산
        Vector2 itemCenter = new(
            (minX + maxX) / 2.0f,
            (minY + maxY) / 2.0f
        );

        // 방의 로컬 좌표계에서 그리드 원점
        Vector2 gridOrigin = new(
            -storageSize.x * GridConstants.CELL_SIZE / 2.0f,
            -storageSize.y * GridConstants.CELL_SIZE / 2.0f
        );

        // 아이템 중앙의 월드 위치 계산
        Vector2 itemCenterWorldPos = new(
            gridOrigin.x + (itemCenter.x + 0.5f) * GridConstants.CELL_SIZE,
            gridOrigin.y + (itemCenter.y + 0.5f) * GridConstants.CELL_SIZE
        );

        // 정확한 위치를 위해 반올림
        Vector3 localPos = new(
            Mathf.Round(itemCenterWorldPos.x * 1000f) / 1000f,
            Mathf.Round(itemCenterWorldPos.y * 1000f) / 1000f,
            0
        );

        // 아이템 배치 (아이템의 피벗이 중앙에 있으므로 그대로 사용)
        item.transform.localPosition = localPos;

        // 아이템 목록에 추가 (이미 있는지 체크)
        if (!storedItems.Contains(item)) storedItems.Add(item);

        // 드래그 모드 해제 (아이템이 보이도록)
        item.SetDragMode(false);


        return true;
    }

    /// <summary>
    /// 아이템을 창고에서 제거합니다.
    /// </summary>
    public virtual bool RemoveItem(TradingItem item)
    {
        if (item == null)
        {
            Debug.LogWarning("RemoveItem: item is null");
            return false;
        }

        // 해당 아이템이 이 창고에 있는지 확인
        if (!storedItems.Contains(item))
        {
            Debug.LogWarning($"RemoveItem: Item {item.GetItemData().itemName} is not in this storage.");
            return false;
        }

        // 그리드 사이즈 확인
        Vector2Int size = GetSize();
        if (size.x <= 0 || size.y <= 0)
        {
            Debug.LogError("RemoveItem: Invalid storage size!");
            return false;
        }

        try
        {
            // 그리드에서 아이템 참조 제거
            int removedCells = 0;

            for (int y = 0; y < size.y; y++)
            for (int x = 0; x < size.x; x++)
                if (itemGrid[y, x] == item)
                {
                    itemGrid[y, x] = null;
                    removedCells++;
                }

            // 저장된 아이템 목록에서도 제거
            storedItems.Remove(item);

            Debug.Log($"Removed item {item.GetItemData().itemName} from storage. Cleared {removedCells} cells.");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in RemoveItem: {e.Message}");
            return false;
        }
    }


    // 가장 좌하단 타일 찾기 헬퍼 메서드
    private Vector2Int FindBottomLeftTile(List<Vector2Int> tiles)
    {
        if (tiles.Count == 0)
            return Vector2Int.zero;

        Vector2Int bottomLeft = tiles[0];

        foreach (Vector2Int tile in tiles)
            // x가 더 작거나, x가 같고 y가 더 작은 경우
            if (tile.x < bottomLeft.x || (tile.x == bottomLeft.x && tile.y < bottomLeft.y))
                bottomLeft = tile;

        return bottomLeft;
    }


    /// <summary>
    /// 아이템을 완전히 삭제합니다.
    /// </summary>
    public virtual bool DestroyItem(TradingItem item)
    {
        if (item == null) return false;

        // 그리드에서 제거
        RemoveItem(item);

        // 목록에서 제거
        storedItems.Remove(item);

        // GameObject 파괴
        Destroy(item.gameObject);

        return true;
    }

    /// <summary>
    /// 워프 후 아이템 상태를 확인 및 갱신합니다.
    /// </summary>
    public virtual void CheckItemsAfterWarp()
    {
        foreach (TradingItem item in storedItems)
            // 각 아이템 타입에 따라 조건 확인
            // 예: 온도 조건이 맞지 않는 아이템은 상태 저하
            if (!IsValidStorageForItem(item))
                DegradeItemState(item);
    }

    /// <summary>
    /// 현재 창고가 해당 아이템에 적합한지 확인합니다.
    /// 기본적으로는 항상 적합하며, 온도 조절 창고 등에서 오버라이드합니다.
    /// </summary>
    protected virtual bool IsValidStorageForItem(TradingItem item)
    {
        return true;
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
                Debug.Log($"Item {item.GetItemData().itemName} is slightly damaged. Value decreased by 25%.");
                break;
            case ItemState.SlightlyDamaged:
                item.itemState = ItemState.ModeratelyDamaged;
                Debug.Log($"Item {item.GetItemData().itemName} is moderately damaged. Value decreased by 50%.");
                break;
            case ItemState.ModeratelyDamaged:
                item.itemState = ItemState.Unsellable;
                Debug.Log($"Item {item.GetItemData().itemName} is completely damaged and unsellable.");
                break;
        }
    }

    /// <summary>
    /// 아이템이 점유하는 모든 타일의 좌표를 반환합니다.
    /// </summary>
    public List<Vector2Int> GetOccupiedTiles(TradingItem item, Vector2Int position, int rotation)
    {
        List<Vector2Int> occupiedTiles = new();


        // 회전에 맞는 아이템 박스 그리드 설정
        RotationConstants.Rotation rotEnum = (RotationConstants.Rotation)rotation;
        bool[][] blockShape = ItemShape.Instance.itemShapes[item.GetItemData().shape][(int)rotEnum];
        item.boxGrid = blockShape;

        // 각 점유된 블록에 대해 창고 좌표 계산
        for (int y = 0; y < 5; y++)
        for (int x = 0; x < 5; x++)
            if (blockShape[y][x])
            {
                // 아이템 블록 좌표를 창고 좌표로 변환 (중심점 2,2 기준)
                int storageX = position.x + (x - 2);
                int storageY = position.y + (2 - y);

                occupiedTiles.Add(new Vector2Int(storageX, storageY));
            }

        return occupiedTiles;
    }

    /// <summary>
    /// 월드 좌표를 그리드 좌표로 변환합니다.
    /// </summary>
    public Vector2Int WorldToGridPosition(Vector2 worldPos)
    {
        // 방 크기
        Vector2Int storageSize = GetSize();

        // 로컬 좌표로 변환
        Vector3 localPos = transform.InverseTransformPoint(worldPos);

        // 그리드 원점 (좌하단)
        Vector2 gridOrigin = new(
            -storageSize.x * GridConstants.CELL_SIZE / 2.0f,
            -storageSize.y * GridConstants.CELL_SIZE / 2.0f
        );

        // 그리드 좌표 계산
        float relX = localPos.x - gridOrigin.x;
        float relY = localPos.y - gridOrigin.y;

        int gridX = Mathf.FloorToInt(relX / GridConstants.CELL_SIZE);
        int gridY = Mathf.FloorToInt(relY / GridConstants.CELL_SIZE);

        Vector2Int result = new(gridX, gridY);

        return result;
    }

    /// <summary>
    /// 그리드 좌표를 월드 좌표로 변환합니다.
    /// </summary>
    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        // 방 크기
        Vector2Int storageSize = GetSize();

        // 그리드 원점 (좌하단)
        Vector2 gridOrigin = new(
            -storageSize.x * GridConstants.CELL_SIZE / 2.0f,
            -storageSize.y * GridConstants.CELL_SIZE / 2.0f
        );

        // 로컬 좌표 계산 (셀 중앙으로)
        Vector3 localPos = new(
            gridOrigin.x + (gridPos.x + 0.5f) * GridConstants.CELL_SIZE,
            gridOrigin.y + (gridPos.y + 0.5f) * GridConstants.CELL_SIZE,
            0
        );

        // 정확한 위치를 위해 반올림
        localPos.x = Mathf.Round(localPos.x * 1000f) / 1000f;
        localPos.y = Mathf.Round(localPos.y * 1000f) / 1000f;

        // 월드 좌표로 변환
        Vector3 worldPos = transform.TransformPoint(localPos);

        return worldPos;
    }

    /// <summary>
    /// 선택된 아이템 설정
    /// </summary>
    public virtual void SelectItem(TradingItem item)
    {
        selectedItem = item;
    }

    /// <summary>
    /// 아이템 회전
    /// </summary>
    public virtual bool RotateItem(TradingItem item)
    {
        Debug.Log(
            $"[StorageRoomBase] {GetInstanceID()} RotateItem 호출 - 아이템: {(item != null ? item.GetInstanceID().ToString() : "null")}");

        if (item == null)
        {
            Debug.LogWarning($"[StorageRoomBase] {GetInstanceID()} RotateItem: 아이템이 null");
            return false;
        }

        // 현재 회전 상태
        RotationConstants.Rotation currentRotation = item.rotation;

        // 다음 회전 상태
        RotationConstants.Rotation nextRotation = (RotationConstants.Rotation)(((int)currentRotation + 1) % 4);
        Debug.Log($"[StorageRoomBase] {GetInstanceID()} 회전 시도: {currentRotation} -> {nextRotation}");

        // 현재 위치에서 회전 가능한지 확인
        if (!CanPlaceItem(item, item.gridPosition, nextRotation))
        {
            Debug.LogWarning($"[StorageRoomBase] {GetInstanceID()} 위치 {item.gridPosition}에서 {nextRotation}으로 회전할 수 없음");
            return false;
        }

        // 기존 그리드에서 제거
        bool removeSuccess = RemoveItem(item);
        Debug.Log($"[StorageRoomBase] {GetInstanceID()} 회전을 위한 아이템 제거 결과: {removeSuccess}");

        // 회전 적용
        item.Rotate(nextRotation);
        Debug.Log($"[StorageRoomBase] {GetInstanceID()} 아이템 회전됨: {nextRotation}");

        // 다시 배치
        bool addSuccess = AddItem(item, item.gridPosition, (int)nextRotation);
        Debug.Log($"[StorageRoomBase] {GetInstanceID()} 회전 후 아이템 재배치 결과: {addSuccess}");
        return addSuccess;
    }

    /// <summary>
    /// 방 크기를 반환합니다. (캐싱을 통해 성능 최적화)
    /// </summary>
    public new Vector2Int GetSize()
    {
        if (cachedSize == Vector2Int.zero)
            cachedSize = base.GetSize();
        return cachedSize;
    }
}
