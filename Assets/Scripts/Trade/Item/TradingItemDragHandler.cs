using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 개선된 무역 아이템 드래그 앤 드롭 핸들러.
/// 실제 아이템을 드래그하는 대신 임시 프리뷰를 사용합니다.
/// 프리팹 없이 직접 프리뷰 오브젝트를 생성합니다.
/// </summary>
public class TradingItemDragHandler : MonoBehaviour
{
    // 싱글톤 패턴으로 구현
    public static TradingItemDragHandler Instance { get; private set; }

    [Header("Preview Colors")]
    [SerializeField]
    private Color validPlacementColor = new(0, 1, 0, 0.5f);

    [SerializeField] private Color invalidPlacementColor = new(1, 0, 0, 0.5f);
    [SerializeField] private Color normalPreviewColor = new(1, 1, 1, 0.7f);

    // 프리뷰 오브젝트 참조
    private GameObject previewObject;
    private SpriteRenderer previewBoxRenderer;

    // 드래그 중인 아이템 정보
    private TradingItem originalItem;
    private StorageRoomBase sourceStorage;
    private Vector2Int originalPosition;
    private RotationConstants.Rotation originalRotation;
    private RotationConstants.Rotation currentRotation;

    // 드래그 상태
    private bool isDragging = false;
    private Vector2 dragOffset;

    // 그리드 좌표 캐싱
    private Vector2Int currentGridPosition;
    private StorageRoomBase currentTargetStorage;

    // 코루틴 참조를 저장할 변수
    private Coroutine flashCoroutine = null;
    private Coroutine resetCoroutine = null;

    // 아이템 재배치 가능 여부를 제어하는 플래그
    public static bool IsItemRepositioningAllowed { get; set; } = true;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 아이템 드래그 시작
    /// </summary>
    public void StartDragging(TradingItem item, StorageRoomBase storage, Vector2 mousePosition)
    {
        // 아이템 재배치가 허용되지 않으면 드래그 시작하지 않음
        if (!IsItemRepositioningAllowed)
        {
            Debug.Log("[TradingItemDragHandler] 아이템 재배치가 허용되지 않아 드래그 시작하지 않음");
            return;
        }

        // 기존에 드래그 중이던 것이 있으면 취소
        if (isDragging)
        {
            Debug.Log("[TradingItemDragHandler] 이미 드래그 중인 아이템이 있어 취소함");
            CancelDragging();
        }

        // 드래그 정보 저장
        originalItem = item;
        sourceStorage = storage;
        originalPosition = item.gridPosition;
        originalRotation = item.rotation;
        currentRotation = originalRotation; // 현재 회전 상태 초기화
        isDragging = true;


        // 원본 아이템을 임시로 비활성화
        item.SetDragMode(true);

        // 프리뷰 생성
        CreatePreview(item);

        // 현재 회전 상태에 맞는 박스 스프라이트로 업데이트
        UpdatePreviewBoxSprite();

        // 드래그 오프셋을 0으로 설정하여 마우스가 항상 아이템 중심에 위치하도록 함
        dragOffset = Vector2.zero;

        // 현재 그리드 위치 초기화
        currentGridPosition = originalPosition;
        currentTargetStorage = sourceStorage;

        // 프리뷰 위치 초기화
        UpdatePreviewPosition(mousePosition);

        // CameraZoomController에게 드래그 시작 알림 (카메라 드래그 방지)
        CameraZoomController cameraController = FindObjectOfType<CameraZoomController>();
        // if (cameraController != null)
        //     cameraController.DisablePanning();
    }

    /// <summary>
    /// 프리뷰 오브젝트 생성 - 프리팹 사용 없이 직접 생성
    /// </summary>
    private void CreatePreview(TradingItem item)
    {
        // 이미 프리뷰가 있다면 제거
        if (previewObject != null)
            Destroy(previewObject);

        // 프리뷰 오브젝트 생성
        previewObject = new GameObject("ItemPreview");

        // 박스 스프라이트 렌더러 추가
        previewBoxRenderer = previewObject.AddComponent<SpriteRenderer>();

        // 박스 스프라이트 설정 (원본 아이템의 박스 스프라이트와 동일하게)
        SpriteRenderer originalBoxRenderer = item.GetBoxRenderer();
        if (originalBoxRenderer != null && originalBoxRenderer.sprite != null)
        {
            previewBoxRenderer.sprite = originalBoxRenderer.sprite;
            previewBoxRenderer.color = normalPreviewColor;
            previewBoxRenderer.sortingOrder = SortingOrderConstants.TradingItemBoxDragging;
        }

        // 아이템 스프라이트 렌더러를 위한 자식 오브젝트 생성
        GameObject itemIconObject = new("ItemIcon");
        itemIconObject.transform.SetParent(previewObject.transform, false);
    }

    private bool lastLoggedDraggingState = false;

    /// <summary>
    /// 매 프레임 마우스 위치에 따라 프리뷰 업데이트
    /// </summary>
    private void Update()
    {
        if (!isDragging || originalItem == null || previewObject == null)
            return;

        if (lastLoggedDraggingState != isDragging)
        {
            // 드래깅 중인 아이템 정보도 함께 출력
            if (originalItem == null)
                Debug.Log("[TradingItemDragHandler] 드래그 중인 아이템 없음 (originalItem is null)");

            lastLoggedDraggingState = isDragging;
        }

        // 마우스 위치 가져오기
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 프리뷰 위치 업데이트
        UpdatePreviewPosition(mouseWorldPos);

        // 현재 위치에 배치 가능한지 검사
        bool canPlace = false;
        StorageRoomBase targetStorage = GetStorageUnderMouse();

        if (targetStorage != null)
        {
            // 그리드 위치 계산 - 마우스 위치를 그대로 사용
            Vector2Int gridPos = targetStorage.WorldToGridPosition(mouseWorldPos);

            // 이전과 동일한 위치면 다시 계산하지 않음
            if (targetStorage != currentTargetStorage || gridPos != currentGridPosition)
            {
                currentGridPosition = gridPos;
                currentTargetStorage = targetStorage;

                // 미리 배치 가능 여부 확인
                canPlace = targetStorage.CanPlaceItem(originalItem, gridPos, currentRotation);

                // 그리드에 스냅된 정확한 위치 계산
                UpdatePreviewToGridPosition(targetStorage, gridPos);
            }
            else
            {
                canPlace = targetStorage.CanPlaceItem(originalItem, currentGridPosition, currentRotation);
            }

            // 배치 가능 여부에 따라 프리뷰 색상 변경
            Color targetColor = canPlace ? validPlacementColor : invalidPlacementColor;
            previewBoxRenderer.color = targetColor;
        }
        else
        {
            // 창고 위에 없을 때는 일반 프리뷰 색상 사용
            previewBoxRenderer.color = normalPreviewColor;
            currentTargetStorage = null;
        }

        // 회전 처리 (좌클릭 드래그 상태에서 우클릭)
        if (Input.GetMouseButtonDown(1) && isDragging) RotatePreview();

        // 드래그 종료 (마우스 버튼 뗌)
        if (Input.GetMouseButtonUp(0)) EndDragging(mouseWorldPos);
    }

    /// <summary>
    /// 그리드 위치에 맞게 프리뷰 위치를 정확하게 업데이트합니다.
    /// 실제 배치와 동일한 로직을 사용합니다.
    /// </summary>
    private void UpdatePreviewToGridPosition(StorageRoomBase storage, Vector2Int gridPos)
    {
        // 점유 타일 계산
        List<Vector2Int> occupiedTiles = storage.GetOccupiedTiles(originalItem, gridPos, (int)currentRotation);

        if (occupiedTiles.Count == 0)
            return;

        // 아이템이 차지하는 영역의 경계 찾기
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        foreach (Vector2Int tile in occupiedTiles)
        {
            minX = Mathf.Min(minX, tile.x);
            maxX = Mathf.Max(maxX, tile.x);
            minY = Mathf.Min(minY, tile.y);
            maxY = Mathf.Max(maxY, tile.y);
        }

        // 아이템 영역의 중앙 계산
        Vector2 itemCenter = new(
            (minX + maxX) / 2.0f,
            (minY + maxY) / 2.0f
        );

        // 방의 로컬 좌표계에서 그리드 원점
        Vector2Int storageSize = storage.GetSize();
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

        // 월드 좌표로 변환
        Vector3 worldPos = storage.transform.TransformPoint(localPos);

        // 프리뷰 위치 설정
        previewObject.transform.position = worldPos;
    }

    /// <summary>
    /// 프리뷰 회전 처리
    /// </summary>
    private void RotatePreview()
    {
        // 다음 회전 상태 계산
        currentRotation = (RotationConstants.Rotation)(((int)currentRotation + 1) % 4);

        // 회전에 맞는 박스 스프라이트 가져오기
        UpdatePreviewBoxSprite();

        // 원본 아이템에게도 알려서 boxGrid 등의 내부 데이터도 업데이트
        bool[][] blockShape = ItemShape.Instance.itemShapes[originalItem.GetItemData().shape][(int)currentRotation];
        originalItem.boxGrid = blockShape;

        // 현재 스토리지가 있으면 그리드 위치에 맞게 업데이트
        if (currentTargetStorage != null) UpdatePreviewToGridPosition(currentTargetStorage, currentGridPosition);
    }

    /// <summary>
    /// 현재 회전 상태에 맞는 박스 스프라이트로 업데이트
    /// </summary>
    private void UpdatePreviewBoxSprite()
    {
        if (originalItem == null || previewBoxRenderer == null)
            return;

        // 해당 아이템 모양에 맞는 스프라이트 배열 로드
        string boxName = $"lot{originalItem.GetItemData().shape}";
        Sprite[] boxSprites = Resources.LoadAll<Sprite>($"Sprites/Item/{boxName}");

        if (boxSprites != null && boxSprites.Length > 0)
        {
            // 현재 회전 상태에 맞는 스프라이트 선택
            int spriteIndex = (int)currentRotation % boxSprites.Length;
            previewBoxRenderer.sprite = boxSprites[spriteIndex];
        }
    }

    /// <summary>
    /// 프리뷰 위치 업데이트
    /// </summary>
    private void UpdatePreviewPosition(Vector2 mouseWorldPos)
    {
        // 창고가 있는 경우, 그리드에 스냅
        StorageRoomBase targetStorage = GetStorageUnderMouse();

        if (targetStorage != null)
        {
            // 마우스 위치를 그대로 사용하여 그리드 위치 계산 (드래그 오프셋 없음)
            Vector2Int gridPos = targetStorage.WorldToGridPosition(mouseWorldPos);
            currentGridPosition = gridPos;
            currentTargetStorage = targetStorage;

            // 그리드 위치에 맞게 업데이트
            UpdatePreviewToGridPosition(targetStorage, gridPos);
        }
        else
        {
            // 창고 위가 아닌 경우, 마우스 위치에 직접 배치
            previewObject.transform.position = new Vector3(mouseWorldPos.x, mouseWorldPos.y, 0);
            currentTargetStorage = null;
        }
    }

    /// <summary>
    /// 드래그 종료 처리 - 다른 창고로 이동하는 개선된 로직
    /// </summary>
    private void EndDragging(Vector2 mouseWorldPos)
    {
        if (!isDragging || originalItem == null) return;

        bool success = false;
        TradingItem itemToReset = originalItem; // 참조 저장

        try
        {
            // 현재 타겟 스토리지와 그리드 위치 사용
            if (currentTargetStorage != null)
            {
                // 마우스 위치로부터 그리드 위치 재계산 (최종 위치 확인)
                Vector2Int gridPos = currentTargetStorage.WorldToGridPosition(mouseWorldPos);

                // 해당 위치에 배치 가능한지 마지막으로 한번 더 확인
                if (currentTargetStorage.CanPlaceItem(originalItem, gridPos, currentRotation))
                {
                    // 현재 드래그 중인 아이템의 원래 창고와 새 창고가 다른 경우
                    if (sourceStorage != currentTargetStorage)
                    {
                        // 새 창고의 AddItem 메서드에서 원 창고에서의 아이템 제거를 처리함
                        success = currentTargetStorage.AddItem(originalItem, gridPos, (int)currentRotation);
                    }
                    else
                    {
                        // 같은 창고 내에서 위치만 변경하는 경우
                        // 먼저 원래 위치에서 제거 (그리드에서만 제거)
                        bool removeSuccess = sourceStorage.RemoveItem(originalItem);

                        // 새 위치에 추가
                        success = currentTargetStorage.AddItem(originalItem, gridPos, (int)currentRotation);
                    }
                }
                else
                {
                    // 배치할 수 없는 이유에 대한 시각적 피드백
                    if (previewObject != null && previewBoxRenderer != null)
                    {
                        // 이미 실행 중인 코루틴이 있다면 중지
                        if (flashCoroutine != null)
                        {
                            StopCoroutine(flashCoroutine);
                            flashCoroutine = null;
                        }

                        // 새 코루틴 시작
                        flashCoroutine = StartCoroutine(FlashInvalidPlacement());
                    }
                }
            }
            else
            {
                Debug.LogWarning("[TradingItemDragHandler] 마우스 아래에 유효한 창고가 없음");
            }
        }
        catch (System.Exception e)
        {
            success = false;
        }
        finally
        {
            // 배치에 실패한 경우 원래 위치로 복원
            if (!success)
                if (originalItem != null && sourceStorage != null)
                {
                    // 드래그 모드 해제 (렌더러, 콜라이더 활성화)
                    originalItem.SetDragMode(false);

                    // 현재 창고에서 우선 제거 (중복 제거 방지를 위한 확인)
                    if (sourceStorage.storedItems.Contains(originalItem))
                    {
                        bool removeSuccess = sourceStorage.RemoveItem(originalItem);
                    }

                    // 회전과 위치 원래대로 설정
                    originalItem.Rotate(originalRotation);

                    // 명시적으로 다시 추가
                    bool addSuccess = sourceStorage.AddItem(originalItem, originalPosition, (int)originalRotation);

                    if (!addSuccess)
                    {
                        // 최후의 수단: 트랜스폼 직접 설정 및 그리드 위치 업데이트
                        originalItem.gridPosition = originalPosition;

                        // 정확한 월드 위치 계산
                        Vector3 worldPos = sourceStorage.GridToWorldPosition(originalPosition);
                        originalItem.transform.position = worldPos;

                        // 창고 목록에 추가
                        if (!sourceStorage.storedItems.Contains(originalItem))
                            sourceStorage.storedItems.Add(originalItem);
                    }
                }

            // 원본 아이템 드래그 모드 해제 - null 체크
            if (itemToReset != null)
            {
                itemToReset.SetDragMode(false);
                // 명시적으로 콜라이더 재활성화 추가
                BoxCollider2D collider = itemToReset.GetComponent<BoxCollider2D>();
                if (collider != null)
                    collider.enabled = true;
            }

            // 프리뷰 정리 및 상태 초기화
            CleanupDrag();

            // 카메라 컨트롤러 패닝 재활성화
            CameraZoomController cameraController = FindObjectOfType<CameraZoomController>();
            // if (cameraController != null)
            //     cameraController.EnablePanning();

            // 모든 아이템의 상태 확인
            foreach (StorageRoomBase storage in FindObjectsOfType<StorageRoomBase>())
            {
                foreach (TradingItem item in storage.storedItems)
                    if (item != null)
                    {
                        // 콜라이더 상태도 함께 확인
                        BoxCollider2D collider = item.GetComponent<BoxCollider2D>();
                        if (collider == null)
                            Debug.LogWarning($"[TradingItemDragHandler] 아이템 {item.GetInstanceID()} 콜라이더 없음!");
                    }
            }
        }

        // 드래그 종료 후 아이템 상호작용 활성화를 위한 짧은 지연
        if (resetCoroutine != null) StopCoroutine(resetCoroutine);
        resetCoroutine = StartCoroutine(ResetAllItemsInteractivity());
    }

    /// <summary>
    /// 드래그 취소 (ESC 키 등으로 취소하는 경우)
    /// </summary>
    public void CancelDragging()
    {
        if (!isDragging || originalItem == null)
            return;

        TradingItem itemToReset = originalItem; // 참조 저장

        try
        {
            // 원본 아이템을 원래 위치로 복원
            if (sourceStorage != null)
            {
                originalItem.SetDragMode(false);

                if (!sourceStorage.storedItems.Contains(originalItem))
                {
                    bool addSuccess = sourceStorage.AddItem(originalItem, originalPosition, (int)originalRotation);
                    if (!addSuccess)
                    {
                        Debug.LogError("Failed to restore item during cancel!");

                        // 강제 복원 시도
                        originalItem.Rotate(originalRotation);
                        originalItem.gridPosition = originalPosition;
                        Vector3 worldPos = sourceStorage.GridToWorldPosition(originalPosition);
                        originalItem.transform.position = worldPos;

                        if (!sourceStorage.storedItems.Contains(originalItem))
                            sourceStorage.storedItems.Add(originalItem);
                    }
                }
                else
                {
                    // 이미 창고에 있다면 위치와 회전만 복원
                    originalItem.Rotate(originalRotation);
                    originalItem.gridPosition = originalPosition;
                    Vector3 worldPos = sourceStorage.GridToWorldPosition(originalPosition);
                    originalItem.transform.position = worldPos;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error during cancel dragging: {e.Message}");
        }
        finally
        {
            // 원본 아이템 활성화
            if (itemToReset != null)
            {
                itemToReset.SetDragMode(false);
                // 명시적으로 콜라이더 재활성화 추가
                BoxCollider2D collider = itemToReset.GetComponent<BoxCollider2D>();
                if (collider != null)
                    collider.enabled = true;
            }

            // 프리뷰 정리 및 상태 초기화
            CleanupDrag();

            // CameraZoomController에게 드래그 종료 알림 (카메라 드래그 재활성화)
            CameraZoomController cameraController = FindObjectOfType<CameraZoomController>();
            // if (cameraController != null)
            //     cameraController.EnablePanning();
        }

        // 모든 아이템 상호작용 재설정
        StartCoroutine(ResetAllItemsInteractivity());
    }

    /// <summary>
    /// 모든 아이템의 상호작용을 재활성화하는 코루틴
    /// </summary>
    private IEnumerator ResetAllItemsInteractivity()
    {
        // 1프레임 기다리기
        yield return null;

        // 모든 창고의 모든 아이템 찾기
        StorageRoomBase[] allStorages = FindObjectsOfType<StorageRoomBase>();

        int resetCount = 0;
        foreach (StorageRoomBase storage in allStorages)
        {
            foreach (TradingItem item in storage.storedItems)
                if (item != null)
                {
                    // 강제로 드래그 모드 해제 및 콜라이더 활성화
                    item.SetDragMode(false);

                    BoxCollider2D collider = item.GetComponent<BoxCollider2D>();
                    if (collider != null) collider.enabled = true;

                    resetCount++;
                }
        }

        resetCoroutine = null;
    }

    /// <summary>
    /// 드래그 관련 리소스 정리
    /// </summary>
    private void CleanupDrag()
    {
        // 진행 중인 깜빡임 코루틴이 있다면 중지
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        // 프리뷰 오브젝트 제거
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
            previewBoxRenderer = null;
        }

        // 상태 초기화
        isDragging = false;

        originalItem = null;
        sourceStorage = null;
        currentTargetStorage = null;
    }

    /// <summary>
    /// 마우스 아래에 있는 스토리지 컴포넌트 반환
    /// </summary>
    private StorageRoomBase GetStorageUnderMouse()
    {
        // 마우스 위치에서 Ray를 발사
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        // Ray가 스토리지 오브젝트에 맞았는지 확인
        if (hit.collider != null)
        {
            StorageRoomBase storage = hit.collider.GetComponentInParent<StorageRoomBase>();
            if (storage != null)
                // Debug.Log($"[TradingItemDragHandler] 마우스 아래 창고 발견: {storage.GetInstanceID()}");
                return storage;
        }

        return null;
    }

    /// <summary>
    /// 배치 불가능 상태 시각적 피드백
    /// </summary>
    private IEnumerator FlashInvalidPlacement()
    {
        if (previewObject == null || previewBoxRenderer == null)
        {
            Debug.LogWarning("[TradingItemDragHandler] FlashInvalidPlacement - 프리뷰 오브젝트 또는 렌더러가 NULL");
            yield break;
        }

        // 원래 색상 저장
        Color originalBoxColor = previewBoxRenderer.color;

        // 빨간색으로 깜빡임
        previewBoxRenderer.color = Color.red;

        yield return new WaitForSeconds(0.2f);

        // 프리뷰가 아직 존재하는지 확인
        if (previewObject != null && previewBoxRenderer != null) previewBoxRenderer.color = originalBoxColor;
    }
}
