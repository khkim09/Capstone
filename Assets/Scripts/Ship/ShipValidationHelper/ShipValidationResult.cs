/// <summary>
/// 함선 배치 검증 결과를 저장하는 클래스입니다.
/// </summary>
public class ValidationResult
{
    /// <summary>오류 유형</summary>
    public ShipValidationError ErrorType { get; private set; }

    /// <summary>추가 설명 메시지</summary>
    public string Message { get; private set; }

    /// <summary>유효한 배치인지 여부</summary>
    public bool IsValid => ErrorType == ShipValidationError.None;

    /// <summary>
    /// 기본 생성자
    /// </summary>
    /// <param name="errorType">오류 유형</param>
    /// <param name="message">추가 설명 메시지</param>
    public ValidationResult(ShipValidationError errorType, string message = "")
    {
        ErrorType = errorType;
        Message = message;
    }
}
