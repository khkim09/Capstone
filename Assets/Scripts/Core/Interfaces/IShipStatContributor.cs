using System.Collections.Generic;


/// <summary>
/// 함선의 ShipStat에 영향을 주는 객체 인터페이스.
/// 방(Room)과 선원(Crew)은 모두 이 인터페이스를 통해 스탯에 기여합니다.
/// </summary>
/// <remarks>
/// Ship은 이 인터페이스를 구현한 모든 컴포넌트의 스탯을 수집하여
/// 전체 함선의 능력치를 계산합니다.
///
/// 향후 선원 스탯 (예: 산소 소모량 등)도 이 구조로 통합 가능하도록 설계되어 있습니다.
/// </remarks>
public interface IShipStatContributor
{
    Dictionary<ShipStat, float> GetStatContributions();
}
