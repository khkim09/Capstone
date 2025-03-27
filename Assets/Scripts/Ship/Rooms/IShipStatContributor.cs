using System.Collections.Generic;

/// <summary>
/// ShipStat 에 영향을 주는 객체 인터페이스
/// 모든 방들과 모든 선원은 전부 이에 해당한다.
/// 방의 경우엔 RoomData 에 등록된 정보를 바탕으로 Ship에서 Contributes 를 합쳐서 배 전체의 Stat을 계산한다.
/// TODO :선원의 경우에 산소 호흡을 하는 선원의 경우 산소 소모를 초당 1% 한다. 즉, 이런 정보들도 선원에게 인터페이스를 달면 Ship 에서 산소 계산을 구현할 때 stat에서 빼와서 쉽게 구현 가능할 것이다.
/// </summary>
public interface IShipStatContributor
{
    Dictionary<ShipStat, float> GetStatContributions();
}
