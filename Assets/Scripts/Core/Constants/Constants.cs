using UnityEngine;

/// <summary>
/// 게임에 쓰이는 모든 상수 클래스. 클래스명은 사전순으로 정리되어있음.
/// </summary>
public static class Constants
{
    public static class Endings
    {
        /// <summary>
        /// 엔딩을 보기 위한 최소 년도
        /// </summary>
        public const int EndingYear = 500;
    }

    /// <summary>
    /// 이벤트 관련 상수
    /// </summary>
    public static class Events
    {
        /// <summary>
        /// 이벤트가 지속되는 시간
        /// </summary>
        public const int EventDuration = 10;
    }

    /// <summary>
    /// 그리드 관련 상수
    /// </summary>
    public static class Grids
    {
        /// <summary>
        /// 그리드 사이즈
        /// </summary>
        public const float CellSize = 1.0f;
    }

    public static class WarpNodes
    {
        public const float NodeSize = 0.05f;

        public const float EdgeMarginHorizontal = 0.1f;
        public const float EdgeMarginVertical = 0.2f;


        public const int LayerNodeCountMin = 2;

        public const int LayerNodeCountMax = 5;

        public const float ConnectionLineThickness = 0.1f;

        public const float EventNodeRate = 0.6f;

        public const float CombatNodeRate = 1 - EventNodeRate;
    }

    /// <summary>
    /// 행성 관련 상수
    /// </summary>
    public static class Planets
    {
        /// <summary>
        /// 게임마다 생성될 행성의 수 입니다. 행성의 수는 무조건 5 개 이상이어야 합니다.
        /// </summary>
        public const int PlanetTotalCount = 10;

        /// <summary>
        /// 행성이 맵에 그려지는 상대적인 크기
        /// </summary>
        public const float PlanetSize = 0.05f;

        /// <summary>
        /// 행성이 맵에 그려질 때, 행성끼리 가져야하는 최소 간격
        /// </summary>
        public const float PlanetSpacingMin = PlanetSize * 3;

        /// <summary>
        /// 행성이 맵에 그려질 때, 가장자리와 떨어진 최소 간격
        /// </summary>
        public const float PlanetSpacingEdge = PlanetSize * 1.5f;

        /// <summary>
        /// 현재 위치 표시기가 맵에 그려지는 상대적인 크기
        /// </summary>
        public const float PlanetCurrentPositionIndicatorSize = PlanetSize * 2f;

        /// <summary>
        /// 유저가 찍는 노드가 맵에 그려지는 상대적인 크기
        /// </summary>
        public const float PlanetNodeSize = PlanetSize * 0.5f;

        /// <summary>
        /// 유저가 현재 위치부터 이동할 수 있는 범위의 상대적인 크기
        /// </summary>
        public const float PlanetNodeValidRadius = PlanetSize * 6f;

        /// <summary>
        /// 2 티어 아이템 해금을 위해 필요한 거래 총액
        /// </summary>
        public const int Tier2RevenueRequirement = 5000;

        /// <summary>
        /// 3 티어 아이템 해금을 위해 필요한 거래 총액
        /// </summary>
        public const int Tier3RevenueRequirement = 15000;
    }

    public static class Quest
    {
        /// <summary>
        /// 퀘스트가 만료되기까지 걸리는 시간 (년)
        /// </summary>
        public const int QuestDuration = 20;

        /// <summary>
        /// 워프 시마다 퀘스트가 생성되는 확률
        /// </summary>
        public const float QuestCreateRate = 0.8f;
    }

    /// <summary>
    /// 회전 관련 상수
    /// </summary>
    public static class Rotations
    {
        /// <summary>
        /// 4방향 회전
        /// </summary>
        public enum Rotation
        {
            Rotation0 = 0,
            Rotation90 = 1,
            Rotation180 = 2,
            Rotation270 = 3
        }
    }

    /// <summary>
    /// SortOrder 관련 상수
    /// </summary>
    public static class SortingOrders
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
}
