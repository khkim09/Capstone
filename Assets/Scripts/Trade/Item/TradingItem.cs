using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// TradingItem 클래스는 거래 가능한 아이템의 데이터를 담고 있으며,
/// 아이템의 속성(행성 코드, 티어, 이름, 상태, 분류 등)과 가격 관련 기능을 제공합니다.
/// </summary>
[Serializable]
public class TradingItem : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler,
    IGridPlaceable
{
    /// <summary>
    /// 아이템 상태
    /// </summary>
    [Tooltip("아이템 상태")] private ItemState itemState;

    [SerializeField] private TradingItemData itemData;

    public int amount;

    [SerializeField] private SpriteRenderer boxRenderer;
    [SerializeField] private SpriteRenderer itemRenderer;
    [SerializeField] private SpriteRenderer frameRenderer;

    private float frameOffsetY = 0.11f;

    public Constants.Rotations.Rotation rotation;

    private Sprite[] boxSprites;

    public bool[][] boxGrid;

    public StorageRoomBase parentStorage;

    public Vector2Int gridPosition;

    // 한 번 계산된 최종 가격을 캐싱할 변수
    private float? cachedPrice = null;

    // 구매 당시 가격을 저장하는 변수
    private float purchasePrice;

    // 처음 호출되었는지 여부 (한 번만 초기화할지 판단)
    private bool priceInitialized = false;

    // 드래그 관련 변수
    public bool isDragging = false;
    public bool isDragMode = false; // 드래그 모드 여부 (프리뷰 사용 중일 때)

    private void Start()
    {
    }

    public void Initialize(TradingItemData data, int quantity, ItemState state = ItemState.Normal)
    {
        itemData = data;

        string boxName = $"lot{itemData.shape}";
        boxSprites = Resources.LoadAll<Sprite>($"Sprites/Item/{boxName}");

        // rotation = Constants.Rotations.Rotation.Rotation0;

        Rotate(rotation);

        boxRenderer.sortingOrder = Constants.SortingOrders.TradingItemBox;

        frameRenderer.sortingOrder = Constants.SortingOrders.TradingItemFrame;

        // 아이템 이미지 설정 (예: 아이템별 개별 스프라이트)
        itemRenderer.sprite = itemData.itemSprite;
        itemState = state;

        itemRenderer.sortingOrder = Constants.SortingOrders.TradingItemIcon;

        amount = quantity;
        Math.Clamp(amount, 0, data.capacity);
        amount = 1; // 테스트용 하나


        if (transform.parent != null) parentStorage = transform.parent.GetComponent<StorageRoomBase>();
    }

    public void CopyFrom(TradingItem other)
    {
        if (other == null)
        {
            Debug.LogWarning("[TradingItem] 복사 대상이 null입니다.");
            return;
        }

        // 기본 데이터 복사
        itemData = other.itemData;
        amount = other.amount;
        itemState = other.itemState;
        rotation = other.rotation;
        gridPosition = other.gridPosition;

        // 부모 스토리지 설정 (복사본은 일반적으로 동일 스토리지에 배치)
        parentStorage = other.parentStorage;


        // 박스 스프라이트 관련
        boxSprites = other.boxSprites;
        boxGrid = ItemShape.Instance.itemShapes[itemData.shape][(int)rotation];
        boxRenderer.sprite = boxSprites[(int)rotation];

        // 정렬 순서 맞추기
        boxRenderer.sortingOrder = Constants.SortingOrders.TradingItemBox;
        itemRenderer.sortingOrder = Constants.SortingOrders.TradingItemIcon;
        frameRenderer.sortingOrder = Constants.SortingOrders.TradingItemFrame;

        // 프레임 위치 재설정
        PositionFrameAtGridCenter();

        // 콜라이더 사이즈 갱신
        UpdateColliderSize();

        // 가격 캐싱도 복사
        cachedPrice = other.cachedPrice;
        priceInitialized = other.priceInitialized;
    }

    /// <summary>
    /// 현재 가격을 반환합니다.
    /// 아직 초기화되지 않았다면 RecalculatePrice()를 호출하여 basePrice ± fluctuation 범위에서
    /// 랜덤으로 결정한 후, 동일한 값을 반환합니다.
    /// </summary>
    /// <returns>현재 계산된 가격</returns>
    public float GetCurrentPrice()
    {
        // 최초 한 번만 가격 결정
        if (!priceInitialized)
        {
            RecalculatePrice();
            priceInitialized = true;
        }

        return cachedPrice.Value;
    }

    /// <summary>
    /// 외부에서 가격을 갱신할 필요가 있을 때 호출합니다.
    /// basePrice ± fluctuation 범위 내에서 새로 랜덤으로 가격을 계산합니다.
    /// 음수가 되지 않도록 보정합니다.
    /// </summary>
    public void RecalculatePrice()
    {
        // 가격 계산 로직 구현
        float basePrice = GetBasePrice();
        float fluctuation = basePrice * itemData.costChangerate;

        float minPrice = Mathf.Max(0, basePrice - fluctuation);
        float maxPrice = basePrice + fluctuation;

        cachedPrice = UnityEngine.Random.Range(minPrice, maxPrice);
    }

    /// <summary>
    /// 원본 가격(BasePrice)을 반환합니다.
    /// 변동폭이나 기타 가격 계산 로직과 무관하게, JSON에 정의된 기본 가격을 그대로 반환합니다.
    /// 인벤토리 아이템의 가격을 나타낼 때 사용됩니다.
    /// </summary>
    /// <returns>아이템의 기본 가격(basePrice)</returns>
    public float GetBasePrice()
    {
        return itemData.costBase;
    }

    public TradingItemData GetItemData()
    {
        return itemData;
    }

    public void ShowItemFrame()
    {
        frameRenderer.enabled = true;
    }


    public void HideItemFrame()
    {
        frameRenderer.enabled = false;
    }

    public Sprite GetItemSprite()
    {
        return itemData.itemSprite;
    }

    public Vector2 GetWorldPosition()
    {
        Vector2 parentPosition = parentStorage.transform.position;
        return new Vector2(parentPosition.x + Constants.Grids.CellSize * 2,
            parentPosition.y + Constants.Grids.CellSize * 2);
    }

    // TradingItem.cs의 Rotate 메서드 수정
    public void Rotate(Constants.Rotations.Rotation newRotation)
    {
        rotation = (Constants.Rotations.Rotation)newRotation;

        // 부모 방의 회전 값 가져오기
        int roomRotation = 0;
        if (parentStorage != null)
            // Room 클래스에서 회전 값을 가져오는 메서드가 필요
            // 예를 들어 parentStorage.GetRoomRotation() 같은 메서드
            roomRotation = (int)parentStorage.currentRotation;

        // 스프라이트 인덱스 계산: (물건 회전 + 방 회전) % 스프라이트 개수
        int spriteIndex = ((int)rotation + roomRotation) % boxSprites.Length;
        boxRenderer.sprite = boxSprites[spriteIndex];

        // 점유 타일은 물건의 논리적 회전만 사용
        boxGrid = ItemShape.Instance.itemShapes[itemData.shape][(int)rotation];


        // 물건 오브젝트 자체를 방 회전의 반대 방향으로 회전
        // 방이 시계 반대 방향으로 90도씩 회전하므로 (z축 -90도)
        // 물건은 시계 방향으로 회전 (z축 +90도)
        float objectRotationZ = roomRotation * 90f;
        transform.localEulerAngles = new Vector3(0, 0, objectRotationZ);

        // 프레임 위치 재설정
        PositionFrameAtGridCenter();

        // 콜라이더 크기 업데이트
        UpdateColliderSize();
    }


    public object GetRotation()
    {
        return rotation;
    }

    public Vector2Int GetGridPosition()
    {
        return gridPosition;
    }

    public void SetGridPosition(Vector2Int position)
    {
        gridPosition = position;
    }

    /// <summary>
    /// 부모 창고 설정 시 회전도 함께 적용
    /// </summary>
    public void SetParentStorage(StorageRoomBase storage)
    {
        parentStorage = storage;

        // 부모 창고가 설정되면 즉시 회전 적용
        if (parentStorage != null) Rotate(rotation);
    }

    // 클릭 이벤트 처리
    public void OnPointerClick(PointerEventData eventData)
    {
        // 드래그 모드일 때는 클릭 이벤트 무시
        if (isDragMode) return;

        // 아이템 재배치가 허용되지 않으면 클릭 이벤트 무시
        if (!TradingItemDragHandler.IsItemRepositioningAllowed)
        {
            Debug.Log($"[TradingItem] {GetInstanceID()} 아이템 재배치가 허용되지 않으므로 클릭 무시");
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 아이템 선택
            if (parentStorage != null)
                parentStorage.SelectItem(this);
            else
                Debug.Log($"[TradingItem] {GetInstanceID()} 부모 스토리지 없음, 선택 불가");
        }
    }

    // 드래그 시작 시 호출
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그 모드일 때는 드래그 이벤트 무시
        if (isDragMode || parentStorage == null)
        {
            Debug.Log($"[TradingItem] {GetInstanceID()} 드래그 모드 중이거나 부모 스토리지 없음, 드래그 무시");
            return;
        }

        // 아이템 재배치가 허용되지 않으면 드래그 이벤트 무시
        if (!TradingItemDragHandler.IsItemRepositioningAllowed)
        {
            Debug.Log($"[TradingItem] {GetInstanceID()} 아이템 재배치가 허용되지 않으므로 드래그 무시");
            return;
        }

        // 왼쪽 마우스 버튼으로 드래그할 때만 처리
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            Debug.Log($"[TradingItem] {GetInstanceID()} 왼쪽 마우스 버튼이 아님, 드래그 무시");
            return;
        }

        // 이제 TradingItemDragHandler로 드래그 처리를 위임
        TradingItemDragHandler.Instance.StartDragging(this, parentStorage, eventData.position);
    }

    // 드래그 중 호출 (개선된 핸들러를 사용하므로 빈 구현)
    public void OnDrag(PointerEventData eventData)
    {
        // TradingItemDragHandler에서 처리
    }

    // 드래그 종료 시 호출 (개선된 핸들러를 사용하므로 빈 구현)
    public void OnEndDrag(PointerEventData eventData)
    {
        // TradingItemDragHandler에서 처리
    }

    /// <summary>
    /// 드래그 모드 설정 (프리뷰 사용 중일 때 원본 아이템의 가시성 설정)
    /// </summary>
    public void SetDragMode(bool isDragging)
    {
        isDragMode = isDragging;

        // 드래그 모드일 때 콜라이더 비활성화 추가
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
            collider.enabled = !isDragging;
        else
            Debug.Log($"[TradingItem] {GetInstanceID()} 콜라이더가 NULL입니다!");

        // 드래그 모드일 때는 반투명하게 또는 비활성화
        if (boxRenderer != null)
        {
            boxRenderer.enabled = !isDragging;
            // 소팅 오더 유지
            if (!isDragging)
                boxRenderer.sortingOrder = Constants.SortingOrders.TradingItemBox;
        }

        if (itemRenderer != null)
        {
            itemRenderer.enabled = !isDragging;
            // 소팅 오더 유지
            if (!isDragging)
                itemRenderer.sortingOrder = Constants.SortingOrders.TradingItemIcon;
        }

        if (frameRenderer != null)
        {
            frameRenderer.enabled = !isDragging;
            // 소팅 오더 유지
            if (!isDragging)
                frameRenderer.sortingOrder = Constants.SortingOrders.TradingItemFrame;
        }

        // 드래그 모드가 끝나면 콜라이더 크기 갱신
        if (!isDragging) UpdateColliderSize();
    }


    /// <summary>
    /// 구매 당시 가격(kg 기준)을 저장합니다.
    /// </summary>
    public void SetPurchasePrice(float pricePerKg)
    {
        purchasePrice = pricePerKg;
    }


    /// <summary>
    /// boxGrid 정보를 기반으로 아이템 모양에 맞게 BoxCollider2D를 생성합니다.
    /// 창고의 회전도 고려하여 실제 표시되는 모양에 맞게 콜라이더를 생성합니다.
    /// </summary>
    private void UpdateColliderSize()
    {
        // 기존 콜라이더 모두 제거
        foreach (BoxCollider2D collider in GetComponents<BoxCollider2D>()) Destroy(collider);

        // boxGrid가 없다면 종료
        if (boxGrid == null)
        {
            Debug.LogWarning($"[TradingItem] {GetInstanceID()} UpdateColliderSize 실패: boxGrid가 NULL");
            return;
        }

        // 스프라이트의 크기 정보 가져오기
        if (boxRenderer == null || boxRenderer.sprite == null)
        {
            Debug.LogWarning($"[TradingItem] {GetInstanceID()} UpdateColliderSize 실패: boxRenderer 또는 sprite가 NULL");
            return;
        }

        // 아이템의 실제 회전 상태 계산 (창고 회전 포함)
        // localEulerAngles.z를 0, 1, 2, 3으로 매핑 (90도마다)
        int visualRotation = Mathf.RoundToInt(transform.localEulerAngles.z / 90f) % 4;

        // 논리적 회전 + 시각적 회전으로 실제 boxGrid 결정
        int actualRotation = ((int)rotation + visualRotation) % 4;
        bool[][] actualBoxGrid = ItemShape.Instance.itemShapes[itemData.shape][actualRotation];

        // 실제 표시되는 모양에서 아이템의 점유 영역 찾기
        int minX = 4, maxX = 0, minY = 4, maxY = 0;
        bool foundValidCell = false;

        for (int y = 0; y < 5; y++)
        for (int x = 0; x < 5; x++)
            if (actualBoxGrid[y][x])
            {
                minX = Mathf.Min(minX, x);
                maxX = Mathf.Max(maxX, x);
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y);
                foundValidCell = true;
            }

        if (!foundValidCell)
        {
            Debug.LogWarning($"[TradingItem] {GetInstanceID()} 유효한 점유 셀이 없음");
            return;
        }

        // 스프라이트의 실제 크기
        float spriteWidth = boxRenderer.bounds.size.x;
        float spriteHeight = boxRenderer.bounds.size.y;

        // 각 타일별로 콜라이더 생성
        for (int y = minY; y <= maxY; y++)
        for (int x = minX; x <= maxX; x++)
            if (actualBoxGrid[y][x])
            {
                // 새 콜라이더 추가
                BoxCollider2D newCollider = gameObject.AddComponent<BoxCollider2D>();

                // 아이템의 전체 크기
                int itemWidth = maxX - minX + 1;
                int itemHeight = maxY - minY + 1;

                // 각 셀의 크기 계산
                float cellWidth = spriteWidth / itemWidth;
                float cellHeight = spriteHeight / itemHeight;

                // 콜라이더 크기 설정 (한 셀 크기)
                newCollider.size = new Vector2(cellWidth * 0.99f, cellHeight * 0.99f); // 약간 작게 만들어 겹침 방지

                // 스프라이트 좌하단 계산
                float spriteBottomLeftX = -spriteWidth / 2f;
                float spriteBottomLeftY = -spriteHeight / 2f;

                // 해당 셀의 중심 위치 계산 (y 좌표는 반전)
                float cellCenterX = spriteBottomLeftX + (x - minX + 0.5f) * cellWidth;
                float cellCenterY = spriteBottomLeftY + (maxY - y + 0.5f) * cellHeight;

                // 콜라이더 위치 설정
                newCollider.offset = new Vector2(cellCenterX, cellCenterY);

                // 이벤트 트리거로 설정
                newCollider.isTrigger = true;
            }
    }

    public SpriteRenderer GetBoxRenderer()
    {
        return boxRenderer;
    }

    public void PositionFrameAtGridCenter()
    {
        if (frameRenderer != null)
        {
            if (boxGrid == null)
            {
                Debug.LogWarning("boxGrid is null, cannot position frame correctly");
                return;
            }

            // 아이템의 실제 회전 상태 계산 (창고 회전 포함)
            // localEulerAngles.z를 0, 1, 2, 3으로 매핑 (90도마다)
            int visualRotation = Mathf.RoundToInt(transform.localEulerAngles.z / 90f) % 4;

            // 논리적 회전 + 시각적 회전으로 실제 boxGrid 결정
            int actualRotation = ((int)rotation + visualRotation) % 4;
            bool[][] actualBoxGrid = ItemShape.Instance.itemShapes[itemData.shape][actualRotation];

            // 실제 표시되는 모양에서 아이템이 차지하는 영역 계산
            int minX = 4, maxX = 0, minY = 4, maxY = 0;
            bool foundValidCell = false;

            for (int y = 0; y < 5; y++)
            for (int x = 0; x < 5; x++)
                if (actualBoxGrid[y][x])
                {
                    minX = Mathf.Min(minX, x);
                    maxX = Mathf.Max(maxX, x);
                    minY = Mathf.Min(minY, y);
                    maxY = Mathf.Max(maxY, y);
                    foundValidCell = true;
                }

            if (!foundValidCell)
            {
                Debug.LogWarning("No valid cells found in actualBoxGrid");
                return;
            }

            // 실제 아이템의 가로/세로 크기
            int itemWidth = maxX - minX + 1;
            int itemHeight = maxY - minY + 1;

            // 실제 스프라이트의 가로/세로 크기
            float spriteWidth = boxRenderer.bounds.size.x;
            float spriteHeight = boxRenderer.bounds.size.y;

            // 5x5 그리드에서 각 셀의 실제 크기 계산
            float cellWidth = spriteWidth / itemWidth;
            float cellHeight = spriteHeight / itemHeight;

            // 스프라이트의 로컬 좌표계에서 좌하단 모서리
            float spriteBottomLeftX = -spriteWidth / 2f;
            float spriteBottomLeftY = -spriteHeight / 2f;

            // 그리드 (2,2) 위치에 해당하는 스프라이트 상의 좌표 계산
            float frameX, frameY;

            // 그리드 (2,2)가 실제 아이템 영역 내에 있는지 확인
            if (2 >= minX && 2 <= maxX && 2 >= minY && 2 <= maxY)
            {
                frameX = spriteBottomLeftX + (2 - minX + 0.5f) * cellWidth;

                int effectiveY = 4 - 2; // 5x5 그리드에서 y좌표 반전 (4는 최대 인덱스)
                frameY = spriteBottomLeftY + (effectiveY - (4 - maxY) + 0.5f) * cellHeight;
            }
            else
            {
                frameX = 0;
                frameY = 0;
            }

            // 스프라이트가 입체인 점을 고려하여 살짝 아래로 배치
            frameY -= Constants.Grids.CellSize * frameOffsetY;
            // 프레임 위치 설정
            frameRenderer.transform.localPosition = new Vector3(frameX, frameY, 0);

            // 프레임 회전은 항상 정면으로 유지
            frameRenderer.transform.localRotation = Quaternion.identity;
        }
    }

    public bool GetIsDragMode()
    {
        return isDragMode;
    }

    public int GetItemId()
    {
        return itemData.id;
    }

    public ItemState GetItemState()
    {
        return itemState;
    }

    public void SetItemState(ItemState state)
    {
        itemState = state;
    }

    public ItemPlanet GetItemPlanet()
    {
        return itemData.planet;
    }

    public ItemTierLevel GetItemTierLevel()
    {
        return itemData.tier;
    }

    public string GetItemName()
    {
        return itemData.itemName;
    }

    public ItemCategory GetItemCategory()
    {
        return itemData.type;
    }

    public int GetItemShape()
    {
        return itemData.shape;
    }

    public float GetTemperaturMin()
    {
        return itemData.temperatureMin;
    }

    public float GetTemperaturMax()
    {
        return itemData.temperatureMax;
    }

    public int GetCapacity()
    {
        return itemData.capacity;
    }

    public int GetCostMin()
    {
        return itemData.costMin;
    }

    public int GetCostMax()
    {
        return itemData.costMax;
    }

    public string GetDescription()
    {
        return itemData.description;
    }

    public float GetCostChangeRate()
    {
        return itemData.costChangerate;
    }

    public int GetRandomCost()
    {
        // TODO : 현재는 아이템 인스턴스 하나 별로 가격을 책정하고 있는데, 같은 무역 아이템이라면 같이 올라가거나 해야될 것 같다.
        //        같은 ID가 아니더라도, 같은 카테고리 등의 요소로도 가격 책정이 동일해야할 수 있으니 일단 보류
        return UnityEngine.Random.Range(GetCostMin(), GetCostMax());
    }

    public StorageRoomBase GetParentStorage()
    {
        return parentStorage;
    }
    /// <summary>
    /// 구매 당시 가격을 반환합니다.
    /// </summary>
    public float GetPurchasePrice()
    {
        return purchasePrice;
    }
}
