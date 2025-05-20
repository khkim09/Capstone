using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// 저장된 설계도를 미리보기 형태로 표시하는 컴포넌트
/// </summary>
public class BPPreviewArea : MonoBehaviour
{
    [SerializeField] private Transform previewRoot;
    [SerializeField] private GameObject previewRoomPrefab;
    [SerializeField] private GameObject previewWeaponPrefab;
    [SerializeField] private BPPreviewCamera previewCamera;
    [SerializeField] private GridPlacer gridPlacer;

    private List<GameObject> spawnedPreviews = new();

    /// <summary>
    /// 도안 preview Area에 띄우기 (미리보기)
    /// </summary>
    /// <param name="data"></param>
    public void Show(BlueprintSaveData data)
    {
        Clear();

        List<Vector2Int> tilePos = new();

        foreach (BlueprintRoomSaveData room in data.rooms)
        {
            spawnedPreviews.Add(
                PlacePreviewRoom(
                    room.bpRoomData,
                    room.bpLevelIndex,
                    room.bpPosition,
                    room.bpRotation,
                    previewRoot,
                    tilePos
                )
            );
        }

        // Debug.LogError("프리뷰 카메라 위치 계산용 tilePos 포함 타일");
        // foreach (Vector2Int tile in tilePos)
        //     Debug.LogError($"{tile}");

        foreach (BlueprintWeaponSaveData weapon in data.weapons)
        {
            spawnedPreviews.Add(
                PlacePreviewWeapon(
                    weapon.bpWeaponData,
                    weapon.bpPosition,
                    weapon.bpDirection,
                    previewRoot,
                    tilePos
                )
            );
        }

        previewCamera.FitToBlueprint(tilePos, previewRoot.position);

        // 강제 렌더링 한 프레임 수행
        previewCamera.RenderOnce();
    }

    /// <summary>
    /// 생성된 preview 게임 오브젝트 모두 삭제
    /// </summary>
    public void Clear()
    {
        foreach (GameObject obj in spawnedPreviews)
            Destroy(obj);
        spawnedPreviews.Clear();
    }

    /// <summary>
    /// 그리드 좌표를 월드 좌표로 변환합니다.
    /// </summary>
    /// <param name="gridPos">변환할 그리드 좌표.</param>
    /// <returns>해당 위치의 월드 좌표.</returns>
    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return Vector3.zero + new Vector3((gridPos.x + 0.5f) * Constants.Grids.CellSize,
            (gridPos.y + 0.5f) * Constants.Grids.CellSize, 0f);
    }

    /// <summary>
    /// 실제 월드 위치 반환
    /// </summary>
    /// <param name="gridPos"></param>
    /// <returns></returns>
    public Vector3 GetWorldPositionFromGrid(Vector2Int gridPos)
    {
        return transform.TransformPoint(GridToWorldPosition(gridPos));
    }

    #region BPPreviewArea 용 함수

    /// <summary>
    /// 실제 방을 해당 위치에 배치함
    /// </summary>
    public GameObject PlacePreviewRoom(RoomData data, int level, Vector2Int position, Constants.Rotations.Rotation rotation, Transform previewRoot, List<Vector2Int> tilePos)
    {
        Vector2Int size = RoomRotationUtility.GetRotatedSize(data.GetRoomDataByLevel(level).size, rotation);
        Vector2 offset = RoomRotationUtility.GetRotationOffset(size, rotation);
        Vector3 worldPos = GetWorldPositionFromGrid(position) + (Vector3)offset;

        GameObject previewObj = Instantiate(previewRoomPrefab, previewRoot);
        previewObj.transform.position = worldPos + new Vector3(0, 0, 10f);
        previewObj.transform.rotation = Quaternion.Euler(0, 0, -(int)rotation * 90);
        previewObj.name = $"PreviewRoom_{data.name}";

        BlueprintRoom bpRoom = previewObj.GetComponent<BlueprintRoom>();
        // bpRoom.SetGridPlacer(this);
        bpRoom.Initialize(data, level, position, rotation);
        // bpRoom.SetBlueprint(targetBlueprintShip);

        foreach (Vector2Int tile in bpRoom.GetOccupiedTiles())
            tilePos.Add(tile);

        return previewObj;
    }

    /// <summary>
    /// 실제 무기를 해당 위치에 배치함
    /// </summary>
    /// <returns>생성된 BlueprintWeapon 인스턴스</returns>
    public GameObject PlacePreviewWeapon(ShipWeaponData data, Vector2Int position, ShipWeaponAttachedDirection direction, Transform previewRoot, List<Vector2Int> tilePos)
    {
        if (data == null)
            return null;

        Vector2Int size = new(2, 1);
        Vector2 offset = RoomRotationUtility.GetRotationOffset(size, Constants.Rotations.Rotation.Rotation0);
        Vector3 worldPos = GetWorldPositionFromGrid(position) + (Vector3)offset;

        GameObject previewObj = Instantiate(previewWeaponPrefab, previewRoot);
        previewObj.transform.position = worldPos + new Vector3(0, 0, 10f);
        previewObj.name = $"PreviewWeapon_{data.name}";

        BlueprintWeapon bpWeapon = previewObj.GetComponent<BlueprintWeapon>();
        // bpWeapon.SetGridPlacer(this);
        bpWeapon.Initialize(data, position, direction);

        // 설계도 함선의 외갑판 레벨 적용 (함선에 추가하기 전)
        // int currentHullLevel = targetBlueprintShip.GetHullLevel();
        // bpWeapon.SetHullLevel(currentHullLevel);
        // bpWeapon.SetBlueprint(targetBlueprintShip);

        foreach (Vector2Int tile in bpWeapon.GetOccupiedTiles())
            tilePos.Add(tile);

        // 함선에 무기 추가 (BlueprintShip.AddPlaceable 메서드에서 다시 한번 외갑판 레벨 적용)
        // targetBlueprintShip.AddPlaceable(bpWeapon);

        return previewObj;
    }

    #endregion
}
