using UnityEngine;

/// <summary>
/// 무기 인벤토리 항목을 드래그하여 설계도에 배치할 수 있게 하는 시스템.
/// 프리뷰 표시 및 설치 타이밍 조정 포함.
/// </summary>
public class BlueprintWeaponDragHandler : MonoBehaviour
{
    [Header("References")] public GameObject previewPrefab;
    public GridPlacer gridPlacer;

    [Header("Preview sprite color")] public Color validColor = new(0, 1, 0, 0.5f);
    public Color invalidColor = new(1, 0, 0, 0.5f);

    private GameObject previewGO;
    private SpriteRenderer previewRenderer;

    private ShipWeaponData draggingWeaponData;
    private ShipWeaponAttachedDirection draggingDirection;

    public int currentHullLevel = 0;

    private bool isDragging = false;
    private Vector2Int weaponSize = new(2, 1); // 무기 크기 고정

    // 이전 정적 변수 대신 BlueprintDragManager 참조를 통해 드래그 상태 확인
    public static bool IsWeaponBeingDragged =>
        BlueprintDragManager.Instance != null && BlueprintDragManager.Instance.IsWeaponBeingDragged;

    /// <summary>
    /// 드래그 시작 시 호출됨.
    /// </summary>
    public void StartDragging(ShipWeaponData data)
    {
        // 이미 다른 드래그가 진행 중이면 무시
        if (!BlueprintDragManager.Instance.StartWeaponDrag())
            return;

        if (previewGO != null)
            Destroy(previewGO);

        draggingWeaponData = data;
        draggingDirection = ShipWeaponAttachedDirection.North; // 기본 방향
        isDragging = true;

        previewGO = Instantiate(previewPrefab);
        previewRenderer = previewGO.GetComponent<SpriteRenderer>();

        try
        {
            if (data.blueprintSprites != null && data.blueprintSprites[currentHullLevel, 0] != null)
                previewRenderer.sprite = data.blueprintSprites[currentHullLevel, 0];
            else if (data.weaponIcon != null)
                previewRenderer.sprite = data.weaponIcon; // 아이콘으로 대체
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"무기 프리뷰 스프라이트 설정 중 오류: {ex.Message}");

            // 오류 발생 시 아이콘으로 대체
            if (data.weaponIcon != null)
                previewRenderer.sprite = data.weaponIcon;
        }

        previewRenderer.color = validColor;

        previewGO.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// 드래그 중단
    /// </summary>
    public void StopDragging()
    {
        if (previewGO != null)
            Destroy(previewGO);

        previewGO = null;
        draggingWeaponData = null;
        isDragging = false;

        // 드래그 상태 해제
        BlueprintDragManager.Instance.StopDrag();
    }

    private void Update()
    {
        if (!isDragging || draggingWeaponData == null || previewGO == null)
            return;

        Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int gridPos = gridPlacer.WorldToGridPosition(mouseWorld);
        Vector2Int size =
            RoomRotationUtility.GetRotatedSize(new Vector2Int(2, 1), RotationConstants.Rotation.Rotation0);
        Vector2 offset = RoomRotationUtility.GetRotationOffset(size, RotationConstants.Rotation.Rotation0);

        Vector3 basePos = gridPlacer.GridToWorldPosition(gridPos) + (Vector3)offset;


        // 무기는 회전이 없으므로 단순히 위치만 업데이트
        previewGO.transform.position = basePos;

        // 설치 가능 여부 시각화
        bool canPlace = gridPlacer.CanPlaceWeapon(draggingWeaponData, gridPos, draggingDirection);
        previewRenderer.color = canPlace ? validColor : invalidColor;

        // 회전 (무기는 부착 방향만 변경)
        if (Input.GetMouseButtonDown(1))
        {
            int directionIndex = 0;

            switch (draggingDirection)
            {
                case ShipWeaponAttachedDirection.East:
                    draggingDirection = ShipWeaponAttachedDirection.South;
                    directionIndex = 2; // South 방향 인덱스
                    break;
                case ShipWeaponAttachedDirection.South:
                    draggingDirection = ShipWeaponAttachedDirection.North;
                    directionIndex = 0; // North 방향 인덱스
                    break;
                case ShipWeaponAttachedDirection.North:
                    draggingDirection = ShipWeaponAttachedDirection.East;
                    directionIndex = 1; // East 방향 인덱스
                    break;
            }

            // 방향에 맞는 스프라이트로 변경 (외갑판 레벨 고려)
            try
            {
                if (draggingWeaponData.blueprintSprites != null &&
                    draggingWeaponData.blueprintSprites[currentHullLevel, directionIndex] != null)
                    previewRenderer.sprite = draggingWeaponData.blueprintSprites[currentHullLevel, directionIndex];
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"무기 회전 스프라이트 변경 중 오류: {ex.Message}");

                // 오류 발생 시 아이콘으로 대체
                if (draggingWeaponData.weaponIcon != null)
                    previewRenderer.sprite = draggingWeaponData.weaponIcon;
            }
        }

        // 설치
        if (Input.GetMouseButtonUp(0))
        {
            if (!canPlace)
            {
                StopDragging();
            }
            else
            {
                gridPlacer.PlaceWeapon(draggingWeaponData, gridPos, draggingDirection);
                StopDragging();
            }
        }
    }
}
