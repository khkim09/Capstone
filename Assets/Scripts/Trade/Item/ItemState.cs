using System;

/// <summary>
/// 아이템의 상태를 나타내는 열거형입니다.
/// </summary>
[Serializable]
public enum ItemState
{
    /// <summary>정상</summary>
    Normal,

    /// <summary>조금 훼손됨 (가치 25% 하락)</summary>
    SlightlyDamaged,

    /// <summary>다소 훼손됨 (가치 50% 하락)</summary>
    Damaged,

    /// <summary>판매 불가 (가치 100% 하락)</summary>
    Unsellable
}
