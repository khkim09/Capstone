using System.Collections;
using UnityEngine;

/// <summary>
/// 게임의 전체 상태와 흐름을 관리하는 매니저.
/// 게임 상태 전환, 날짜 및 연도 진행, 플레이어/적 함선 정보 등을 통합적으로 제어합니다.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// 현재 플레이어가 조종 중인 함선입니다.
    /// </summary>
    public Ship playerShip;

    /// <summary>
    /// 현재 전투 중인 적 함선입니다.
    /// </summary>
    private Ship currentEnemyShip;

    /// <summary>
    /// 날짜 변경 이벤트 델리게이트입니다.
    /// </summary>
    public delegate void DayChangedHandler(int newDay);

    /// <summary>
    /// 게임 상태 변경 이벤트 델리게이트입니다.
    /// </summary>
    public delegate void GameStateChangedHandler(GameState newState);

    /// <summary>
    /// 현재 게임 상태입니다.
    /// </summary>
    [Header("Game State")]
    [SerializeField]
    private GameState currentState = GameState.MainMenu;

    /// <summary>
    /// 현재 게임 연도입니다.
    /// </summary>
    [SerializeField] private int currentYear = 0; // 게임 시작 년도 (수정 가능)

    /// <summary>
    /// 현재 연도 (읽기 전용).
    /// </summary>
    public int CurrentYear => currentYear;

    /// <summary>
    /// 싱글턴 인스턴스입니다.
    /// </summary>
    public static GameManager Instance { get; private set; }



    // 테스트용 룸 프리팹
    public GameObject testRoomPrefab1;
    public GameObject testRoomPrefab2;
    public GameObject testRoomPrefab3;


    /// <summary>
    /// 게임 상태 변경 이벤트 델리게이트입니다.
    /// </summary>
    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 필요한 초기화
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        playerShip = FindAnyObjectByType<Ship>();

        // LocalizationManager 초기화
        LocalizationManager.Initialize(this);
        LocalizationManager.OnLanguageChanged += OnLanguageChanged;

        playerShip.Initialize();
        // ForSerializeTest();
        RoomTest();
    }

    /// <summary>
    /// 게임 상태 변경 시 호출되는 이벤트입니다.
    /// </summary>
    public event GameStateChangedHandler OnGameStateChanged;

    /// <summary>
    /// 날짜 변경 시 호출되는 이벤트입니다.
    /// </summary>
    public event DayChangedHandler OnDayChanged;

    private void OnLanguageChanged(SystemLanguage newLanguage)
    {
        // 디버그 로그
        Debug.Log($"Language changed to: {newLanguage}");
    }


    /// <summary>
    /// 게임 초기화 로직을 수행합니다.
    /// </summary>
    private void InitializeGame()
    {
        // 다른 매니저들이 모두 초기화되었는지 확인
        StartCoroutine(WaitForManagers());
    }


    /// <summary>
    /// 다른 중요 매니저가 초기화될 때까지 대기합니다.
    /// </summary>
    private IEnumerator WaitForManagers()
    {
        // 다른 중요 매니저들이 초기화될 때까지 기다림
        yield return new WaitUntil(() =>
            ResourceManager.Instance != null &&
            EventManager.Instance != null &&
            CrewManager.Instance != null
        );
    }

    /// <summary>
    /// 현재 플레이어 함선을 반환합니다.
    /// </summary>
    public Ship GetPlayerShip()
    {
        return playerShip;
    }

    /// <summary>
    /// 플레이어 함선을 설정합니다.
    /// </summary>
    public Ship SetPlayerShip(Ship ship)
    {
        return playerShip = ship;
    }

    /// <summary>
    /// 현재 적 함선을 반환합니다.
    /// </summary>
    public Ship GetCurrentEnemyShip()
    {
        return currentEnemyShip;
    }

    /// <summary>
    /// 적 함선을 설정합니다.
    /// </summary>
    public void SetCurrentEnemyShip(Ship enemyShip)
    {
        currentEnemyShip = enemyShip;
    }

    /// <summary>
    /// 게임 상태를 변경하고 상태에 따라 관련 처리를 수행합니다.
    /// </summary>
    /// <param name="newState">변경할 게임 상태.</param>
    public void ChangeGameState(GameState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            OnGameStateChanged?.Invoke(newState);

            // 상태에 따른 특정 로직
            switch (newState)
            {
                case GameState.MainMenu:
                    Time.timeScale = 0f;
                    break;
                case GameState.Gameplay:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    HandleGameOver();
                    break;
            }
        }
    }


    /// <summary>
    /// 이벤트 처리 완료 후 호출됩니다.
    /// 현재는 빈 함수이며, 후속 처리 로직을 연결할 수 있습니다.
    /// </summary>
    public void OnEventCompleted()
    {
    }

    /// <summary>
    /// 게임 오버 상태 진입 시 호출됩니다.
    /// UI 업데이트, 점수 계산 등 게임 오버 관련 처리를 담당합니다.
    /// </summary>
    private void HandleGameOver()
    {
        // 게임 오버 처리 로직

        // UI 업데이트, 점수 계산 등
    }

    /// <summary>
    /// 워프 실행 시 1년을 경과시키고, 관련 효과나 이벤트를 처리합니다.
    /// 불가사의 지속 효과 갱신을 포함합니다.
    /// </summary>
    public void AddYearByWarp()
    {
        currentYear++;

        // 워프로 인한 이벤트 처리
        // EventManager.Instance.TriggerYearlyWarpEvent();

        EventMoraleEffectManager.Instance.CheckEventExpirations(currentYear); // 불가사의 지속 기간 체크

        Debug.Log($"[워프 완료] 현재 연도 : {currentYear}");
    }

    public void RoomTest()
    {
        playerShip.AddRoom(3, testRoomPrefab1.GetComponent<Room>().GetRoomData(), new Vector2Int(0, 0),
            RotationConstants.Rotation.Rotation0);
        playerShip.AddRoom(1, testRoomPrefab2.GetComponent<Room>().GetRoomData(), new Vector2Int(4, 1),
            RotationConstants.Rotation.Rotation90);
        playerShip.AddRoom(1, testRoomPrefab3.GetComponent<Room>().GetRoomData(), new Vector2Int(-4, -1),
            RotationConstants.Rotation.Rotation0);

        CrewBase crewBase1 = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(CrewRace.Human);
        CrewBase crewBase2 = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(CrewRace.Human);
        CrewBase crewBase3 = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(CrewRace.Insect);

        if (crewBase1 is CrewMember crewMember) playerShip.AddCrew(crewMember);
        if (crewBase2 is CrewMember crewMember2) playerShip.AddCrew(crewMember2);
        if (crewBase3 is CrewMember crewMember3) playerShip.AddCrew(crewMember3);
    }

    public void ForSerializeTest()
    {
        Room room2 = GameObjectFactory.Instance.RoomFactory.CreateRoomInstance(RoomType.Power, 3);
        Room room3 = GameObjectFactory.Instance.RoomFactory.CreateCrewQuartersRoomInstance(CrewQuartersRoomSize.Big);

        Room room1 =
            GameObjectFactory.Instance.RoomFactory.CreateStorageRoomInstance(StorageType.Regular, StorageSize.Big);

        playerShip.AddRoom(room2, new Vector2Int(0, 0), RotationConstants.Rotation.Rotation0);
        playerShip.AddRoom(room3, new Vector2Int(4, 1), RotationConstants.Rotation.Rotation90);
        playerShip.AddRoom(room1, new Vector2Int(-4, -1), RotationConstants.Rotation.Rotation0);

        ShipWeapon newWeapon = playerShip.AddWeapon(0, new Vector2Int(3, -1), ShipWeaponAttachedDirection.North);
        ShipWeapon newWeapon2 = playerShip.AddWeapon(6, new Vector2Int(-2, -1), ShipWeaponAttachedDirection.East);
        ShipWeapon newWeapon3 = playerShip.AddWeapon(10, new Vector2Int(6, 2), ShipWeaponAttachedDirection.North);


        CrewBase crewBase1 = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(CrewRace.Human);
        CrewBase crewBase2 = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(CrewRace.Human);
        CrewBase crewBase3 = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(CrewRace.Insect);

        if (crewBase1 is CrewMember crewMember) playerShip.AddCrew(crewMember);
        if (crewBase2 is CrewMember crewMember2) playerShip.AddCrew(crewMember2);
        if (crewBase3 is CrewMember crewMember3) playerShip.AddCrew(crewMember3);


        TradingItem tradingItem = GameObjectFactory.Instance.ItemFactory.CreateItemInstance(2, 1);

        foreach (Room room in playerShip.allRooms)
            if (room is StorageRoomBase)
            {
                StorageRoomBase storageRoom = room as StorageRoomBase;
                storageRoom.AddItem(tradingItem, new Vector2Int(1, 1), 0);
            }

        ShipSerialization.SaveShip(playerShip, Application.persistentDataPath + "/ship_data.es3");
        ShipSerialization.LoadShip(Application.persistentDataPath + "/ship_data.es3");

        // RoomSerialization.SaveAllRooms(GetAllRooms(), Application.persistentDataPath + "/room_data.es3");
        //
        // TradingItemSerialization.SaveShipItems(this, Application.persistentDataPath + "/item_data.es3");
        // ShipWeaponSerialization.SaveAllWeapons(GetAllWeapons(), Application.persistentDataPath + "/weapon_data.es3");
        // CrewSerialization.SaveAllCrews(GetAllCrew(), Application.persistentDataPath + "/crew_data.es3");
        //
        // RoomSerialization.RestoreAllRoomsToShip(Application.persistentDataPath + "/room_data.es3", this);
        // CrewSerialization.RestoreAllCrewsToShip(Application.persistentDataPath + "/crew_data.es3", this);
        // ShipWeaponSerialization.RestoreAllWeaponsToShip(Application.persistentDataPath + "/weapon_data.es3", this);
        // TradingItemSerialization.RestoreAllItemsToShip(this, Application.persistentDataPath + "/item_data.es3");
    }
}

/// <summary>
/// 게임의 상태를 나타내는 열거형입니다.
/// 게임 흐름 제어에 사용됩니다.
/// </summary>
public enum GameState
{
    /// <summary>메인 메뉴 화면 상태입니다.</summary>
    MainMenu,

    /// <summary>게임이 실제로 진행 중인 상태입니다.</summary>
    Gameplay,

    /// <summary>워프(연도 진행) 중 상태입니다.</summary>=
    Warp,

    /// <summary>이벤트 처리 중 상태입니다.</summary>
    Event,

    /// <summary>게임이 일시정지된 상태입니다.</summary>
    Paused,

    /// <summary>게임 오버 상태입니다.</summary>
    GameOver
}
