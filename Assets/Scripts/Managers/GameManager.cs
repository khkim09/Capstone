using System;
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
    /// 엔딩 조건과 관련된 플레이어 데이터
    /// </summary>
    public PlayerData playerData;

    /// <summary>
    /// 게임 상태 변경 이벤트 델리게이트입니다.
    /// </summary>
    public delegate void GameStateChangedHandler(GameState newState);

    public event Action<int> OnYearChanged;

    /// <summary>
    /// 현재 게임 상태입니다.
    /// </summary>
    [Header("Game State")] [SerializeField]
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


    public event Action OnShipInitialized;


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
        // LocalizationManager 초기화
        LocalizationManager.Initialize(this);
        LocalizationManager.OnLanguageChanged += OnLanguageChanged;


        if (playerShip == null)
        {
        }

        playerShip = FindAnyObjectByType<Ship>();


        playerShip.Initialize();
        CreateDefaultPlayerShip();
        OnShipInitialized?.Invoke();
        ForSerializeTest();
    }

    /// <summary>
    /// 게임 상태 변경 시 호출되는 이벤트입니다.
    /// </summary>
    public event GameStateChangedHandler OnGameStateChanged;

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

    public Ship CreateDefaultPlayerShip()
    {
        Room cockpit = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Cockpit);
        Room engine = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Engine);
        Room power = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Power);
        Room crewQuarters = GameObjectFactory.Instance.CreateCrewQuartersRoomInstance(CrewQuartersRoomSize.Basic);
        Room teleporter = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Teleporter);
        Room[] corridors = new Room[15];

        for (int index = 0; index < corridors.Length; index++)
            corridors[index] = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Corridor);

        playerShip.AddRoom(cockpit, new Vector2Int(35, 31), RotationConstants.Rotation.Rotation90);
        playerShip.AddRoom(engine, new Vector2Int(34, 28), RotationConstants.Rotation.Rotation270);
        playerShip.AddRoom(power, new Vector2Int(33, 33), RotationConstants.Rotation.Rotation90);
        playerShip.AddRoom(crewQuarters, new Vector2Int(32, 26), RotationConstants.Rotation.Rotation270);
        playerShip.AddRoom(teleporter, new Vector2Int(32, 32), RotationConstants.Rotation.Rotation90);

        playerShip.AddRoom(corridors[0], new Vector2Int(31, 32));
        playerShip.AddRoom(corridors[1], new Vector2Int(31, 31));
        playerShip.AddRoom(corridors[2], new Vector2Int(31, 30));
        playerShip.AddRoom(corridors[3], new Vector2Int(32, 31));
        playerShip.AddRoom(corridors[4], new Vector2Int(32, 30));
        playerShip.AddRoom(corridors[5], new Vector2Int(33, 30));
        playerShip.AddRoom(corridors[6], new Vector2Int(34, 31));
        playerShip.AddRoom(corridors[7], new Vector2Int(34, 30));
        playerShip.AddRoom(corridors[8], new Vector2Int(33, 31));
        playerShip.AddRoom(corridors[9], new Vector2Int(31, 33));
        playerShip.AddRoom(corridors[10], new Vector2Int(32, 33));
        playerShip.AddRoom(corridors[11], new Vector2Int(31, 34));
        playerShip.AddRoom(corridors[12], new Vector2Int(32, 34));
        playerShip.AddRoom(corridors[13], new Vector2Int(31, 35));
        playerShip.AddRoom(corridors[14], new Vector2Int(32, 35));

        // Room temp = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Corridor);
        // playerShip.AddRoom(temp, new Vector2Int(50, 31), RotationConstants.Rotation.Rotation90);

        playerShip.AddWeapon(1, new Vector2Int(35, 33), ShipWeaponAttachedDirection.East);

        playerShip.UpdateOuterHullVisuals();

        return null;
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
    /// </summary>
    public void AddYear()
    {
        currentYear++;
        OnYearChanged?.Invoke(currentYear);
        Debug.Log($"[워프 완료] 현재 연도 : {currentYear}");
    }

    public void ForSerializeTest()
    {
        // Room room1 = GameObjectFactory.Instance.RoomFactory.CreateRoomInstance(RoomType.Cockpit, 1);
        // Room room2 = GameObjectFactory.Instance.RoomFactory.CreateRoomInstance(RoomType.Engine, 1);
        // Room room3 = GameObjectFactory.Instance.RoomFactory.CreateRoomInstance(RoomType.Power, 1);
        // Room room4 = GameObjectFactory.Instance.RoomFactory.CreateRoomInstance(RoomType.Teleporter, 1);
        // Room room5 = GameObjectFactory.Instance.RoomFactory.CreateCrewQuartersRoomInstance(CrewQuartersRoomSize.Basic);
        // Room room6 = GameObjectFactory.Instance.RoomFactory.CreateRoomInstance(RoomType.Corridor, 1);
        // Room room7 = GameObjectFactory.Instance.RoomFactory.CreateRoomInstance(RoomType.Corridor, 1);
        // Room room8 = GameObjectFactory.Instance.RoomFactory.CreateRoomInstance(RoomType.Corridor, 1);
        //
        // playerShip.AddRoom(room1, new Vector2Int(30, 33), RotationConstants.Rotation.Rotation0);
        // playerShip.AddRoom(room2, new Vector2Int(26, 32), RotationConstants.Rotation.Rotation0);
        // playerShip.AddRoom(room3, new Vector2Int(28, 34), RotationConstants.Rotation.Rotation90);
        // playerShip.AddRoom(room4, new Vector2Int(31, 32), RotationConstants.Rotation.Rotation180);
        // playerShip.AddRoom(room5, new Vector2Int(29, 28), RotationConstants.Rotation.Rotation270);
        // playerShip.AddRoom(room6, new Vector2Int(28, 32), RotationConstants.Rotation.Rotation0);
        // playerShip.AddRoom(room7, new Vector2Int(29, 32), RotationConstants.Rotation.Rotation0);
        // playerShip.AddRoom(room8, new Vector2Int(30, 32), RotationConstants.Rotation.Rotation0);
        //
        // // ShipWeapon newWeapon = playerShip.AddWeapon(0, new Vector2Int(3, -1), ShipWeaponAttachedDirection.North);
        // // ShipWeapon newWeapon2 = playerShip.AddWeapon(6, new Vector2Int(-2, -1), ShipWeaponAttachedDirection.East);
        // // ShipWeapon newWeapon3 = playerShip.AddWeapon(10, new Vector2Int(6, 2), ShipWeaponAttachedDirection.North);
        // //
        // //
        // CrewBase crewBase1 = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(CrewRace.Human);
        // CrewBase crewBase2 = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(CrewRace.Beast);
        // CrewBase crewBase3 = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(CrewRace.Insect);
        //
        // if (crewBase1 is CrewMember crewMember) playerShip.AddCrew(crewMember);
        // if (crewBase2 is CrewMember crewMember2) playerShip.AddCrew(crewMember2);
        // if (crewBase3 is CrewMember crewMember3) playerShip.AddCrew(crewMember3);
        //
        //
        // TradingItem tradingItem = GameObjectFactory.Instance.ItemFactory.CreateItemInstance(2, 1);
        //
        // foreach (Room room in playerShip.allRooms)
        //     if (room is StorageRoomBase)
        //     {
        //         StorageRoomBase storageRoom = room as StorageRoomBase;
        //         storageRoom.AddItem(tradingItem, new Vector2Int(1, 1), 0);
        //     }
        //
        //ShipSerialization.SaveShip(playerShip, Application.persistentDataPath + "/ship_data.es3");
        //ShipSerialization.LoadShip(Application.persistentDataPath + "/ship_data.es3");
        //ShipSerialization.LoadShip(Application.persistentDataPath + "/ship_data.es3");
        //

        //ShipSerialization.SaveShip(playerShip, Application.persistentDataPath + "/ship_data_test.es3");
        // ShipSerialization.LoadShip(Application.persistentDataPath + "/ship_data_test.es3");
        // ShipSerialization.LoadShip(Application.persistentDataPath + "/ship_data_test.es3");

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

    /// <summary>워프(연도 진행) 중 상태입니다.</summary>
    Warp,

    /// <summary>이벤트 처리 중 상태입니다.</summary>
    Event,

    /// <summary>게임이 일시정지된 상태입니다.</summary>
    Paused,

    /// <summary>게임 오버 상태입니다.</summary>
    GameOver
}
