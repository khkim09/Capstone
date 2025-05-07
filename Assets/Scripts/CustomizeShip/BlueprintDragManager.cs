using UnityEngine;

/// <summary>
/// 함선 설계도에서 모든 드래그 작업을 통합 관리하는 싱글톤 클래스.
/// 방과 무기의 동시 드래그를 방지합니다.
/// </summary>
public class BlueprintDragManager : MonoBehaviour
{
    public static BlueprintDragManager Instance;

    /// <summary>
    /// 현재 드래그 중인 오브젝트 타입
    /// </summary>
    public enum DragType
    {
        None,
        Room,
        Weapon
    }

    /// <summary>
    /// 현재 드래그 중인 오브젝트 타입
    /// </summary>
    private DragType currentDragType = DragType.None;

    /// <summary>
    /// 현재 드래그 중인지 여부
    /// </summary>
    public bool IsDragging => currentDragType != DragType.None;

    /// <summary>
    /// 방 드래그 중인지 여부
    /// </summary>
    public bool IsRoomBeingDragged => currentDragType == DragType.Room;

    /// <summary>
    /// 무기 드래그 중인지 여부
    /// </summary>
    public bool IsWeaponBeingDragged => currentDragType == DragType.Weapon;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this) Destroy(gameObject);
    }

    /// <summary>
    /// 방 드래그 시작
    /// </summary>
    /// <returns>드래그 시작 가능 여부</returns>
    public bool StartRoomDrag()
    {
        if (IsDragging)
            return false;

        currentDragType = DragType.Room;
        return true;
    }

    /// <summary>
    /// 무기 드래그 시작
    /// </summary>
    /// <returns>드래그 시작 가능 여부</returns>
    public bool StartWeaponDrag()
    {
        if (IsDragging)
            return false;

        currentDragType = DragType.Weapon;
        return true;
    }

    /// <summary>
    /// 드래그 종료
    /// </summary>
    public void StopDrag()
    {
        currentDragType = DragType.None;
    }
}
