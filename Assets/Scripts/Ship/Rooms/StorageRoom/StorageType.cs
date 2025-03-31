/// <summary>
/// 창고의 저장 방식 또는 특성에 따른 타입 분류입니다.
/// </summary>
public enum StorageType
{
    /// <summary>
    /// 일반 창고 – 상온 보관이 가능한 아이템 저장.
    /// </summary>
    Regular,

    /// <summary>
    /// 온도 조절 창고 – 냉장/냉동 등 온도 민감 아이템 저장.
    /// 전력 미공급 시 일반 창고로 동작.
    /// </summary>
    Temperature,

    /// <summary>
    /// 동물 우리 – 동물류 아이템만 저장 가능.
    /// </summary>
    Animal
}
