public static class SortingOrderConstants
{
    public const int Background = -15;
    public const int Room = -10;
    public const int Door = 10;
    public const int Weapon = -10;
    public const int RoomIcon = -9;
    public const int TradingItemBox = 100;
    public const int TradingItemFrame = 110;
    public const int TradingItemIcon = 120;
    public const int TradingItemBoxDragging = 130;
    public const int TradingItemFrameDragging = 140;
    public const int TradingItemIconDragging = 150;
    public const int Character = 500;
    public const int CharacterHealthBar = 510;
    public const int UI = 1000;
    public const int SettingUI = 1500;

    /// <summary>
    /// Scene 전환 시 트래지션에 사용되는 페이드 효과. 이것보다 높은 Sorting Order는 존재하면 안됨
    /// </summary>
    public const int SceneFade = 2000;
}
