using UnityEngine;

/// <summary>
/// 함선에 추가되는 모듈의 기본 클래스입니다.
/// 모든 모듈은 이 클래스를 상속받아 공통 기능(예: 배치 시 처리, 비용, 크기 등)을 사용합니다.
/// </summary>
public abstract class ShipModuleBase : MonoBehaviour
{
    [Header("Module Base Settings")]
    public string moduleName = "New Module";
    public int moduleCost = 0;
    public Vector2Int moduleSize = new Vector2Int(1, 1); // 그리드 상에서 차지하는 셀의 크기

    // ShipCustomizationManager에서 배치할 때 설정되는 그리드 위치
    public Vector2Int gridPosition;

    /// <summary>
    /// 모듈이 함선에 배치되었을 때 호출됩니다.
    /// 자식 클래스에서 추가적인 초기화나 동작을 구현할 수 있습니다.
    /// </summary>
    public virtual void OnPlaced()
    {
        Debug.Log($"{moduleName} has been placed at grid position {gridPosition}.");
    }

    /// <summary>
    /// 모듈이 함선에서 제거될 때 호출됩니다.
    /// </summary>
    public virtual void OnRemoved()
    {
        Debug.Log($"{moduleName} has been removed from the ship.");
    }

    // 필요에 따라 공통 동작(예: 모듈 업데이트, 상호작용 처리 등)을 추가할 수 있습니다.
}
