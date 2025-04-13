/// <summary>
/// 게임 상태 머신을 구현하는 인터페이스입니다.
/// 각 상태에서 진입, 업데이트, 종료 시 호출되는 메서드를 정의합니다.
/// </summary>
public interface IGameStateMachine
{
    /// <summary>
    /// 상태에 진입할 때 호출됩니다.
    /// 상태를 초기화하거나 필요한 컴포넌트를 설정하는 등의 작업을 수행합니다.
    /// </summary>
    void Enter();


    /// <summary>
    /// 매 프레임 호출됩니다.
    /// 상태에 따른 업데이트 로직을 처리합니다.
    /// </summary>
    void Update();


    /// <summary>
    /// 상태가 종료될 때 호출됩니다.
    /// 상태 종료 시 정리 작업이나 이벤트 구독 해제 등을 수행합니다.
    /// </summary>
    void Exit();
}
