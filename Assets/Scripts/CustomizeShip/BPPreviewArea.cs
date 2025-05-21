using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// 저장된 설계도를 미리보기 형태로 표시하는 컴포넌트
/// </summary>
public class BPPreviewArea : MonoBehaviour
{
    [SerializeField] private Transform previewRoot;
    [SerializeField] private BPPreviewCamera previewCamera;
    [SerializeField] private GridPlacer gridPlacer;

    private List<GameObject> spawnedPreviews = new();

    /// <summary>
    /// 도안 preview Area에 띄우기 (미리보기)
    /// </summary>
    /// <param name="data"></param>
    public void UpdateAndShow(BlueprintSaveData data)
    {
        Clear();

        List<Vector2Int> tilePos = new();

        foreach (BlueprintRoomSaveData room in data.rooms)
        {
            spawnedPreviews.Add(
                gridPlacer.PlacePreviewRoom(
                    room.bpRoomData,
                    room.bpLevelIndex,
                    room.bpPosition,
                    room.bpRotation,
                    previewRoot,
                    tilePos
                )
            );
        }

        foreach (BlueprintWeaponSaveData weapon in data.weapons)
        {
            spawnedPreviews.Add(
                gridPlacer.PlacePreviewWeapon(
                    weapon.bpWeaponData,
                    weapon.bpPosition,
                    weapon.bpDirection,
                    previewRoot,
                    tilePos
                )
            );
        }

        previewCamera.FitToBlueprint(tilePos);

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
}
