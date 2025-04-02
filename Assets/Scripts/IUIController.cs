// 상태별 UI 컨트롤러 인터페이스

/// <summary>
/// 상태별 UI 컨트롤러 인터페이스.
/// 이 인터페이스는 UI 상태에 따라 각기 다른 UI 컴포넌트들이 구현해야 하는 메서드들을 정의합니다.
/// </summary>
public interface IUIController
{
    /// <summary>
    /// UI 컴포넌트를 초기화하는 메서드입니다.
    /// 이 메서드는 UI 컴포넌트를 처음 시작할 때 필요한 초기화 작업을 수행합니다.
    /// </summary>
    void Initialize();

    /// <summary>
    /// UI를 표시하는 메서드입니다.
    /// UI가 화면에 나타나게 합니다.
    /// </summary>
    void Show();

    /// <summary>
    /// UI를 숨기는 메서드입니다.
    /// UI를 화면에서 제거하거나 비활성화하여 숨깁니다.
    /// </summary>
    void Hide();

    /// <summary>
    /// UI의 상태를 업데이트하는 메서드입니다.
    /// 매 프레임마다 UI의 상태나 데이터를 업데이트합니다.
    /// </summary>
    void Update();
}
