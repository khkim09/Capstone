using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// 게임의 전체 상태와 흐름을 관리하는 매니저.
/// 게임 상태 전환, 날짜 및 연도 진행, 플레이어/적 함선 정보 등을 통합적으로 제어합니다.
/// </summary>
[DefaultExecutionOrder(-10)]
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// 현재 플레이어가 조종 중인 함선입니다.
    /// </summary>
    public Ship playerShip;

    /// <summary>
    /// 현재 전투 중인 적 함선입니다.
    /// </summary>
    public Ship currentEnemyShip;

    /// <summary>
    /// 엔딩 조건과 관련된 플레이어 데이터
    /// </summary>
    public PlayerData playerData;

    /// <summary>
    /// 현재 게임의 행성 데이터들
    /// </summary>
    private List<PlanetData> planetDataList = new();

    /// <summary>
    /// 월드맵의 노드 데이터
    /// </summary>
    private List<WorldNodeData> worldNodeDataList = new();

    /// <summary>
    /// 워프맵의 노드 데이터
    /// </summary>
    private List<WarpNodeData> warpNodeDataList = new();

    /// <summary>
    /// 현재 워프 노드의 ID
    /// </summary>
    private int currentWarpNodeId = -1;

    /// <summary>
    /// 현재 워프 목표 행성 ID
    /// </summary>
    private int currentWarpTargetPlanetId = -1;

    /// <summary>
    /// 현재 행성맵에서의 유저의 위치
    /// </summary>
    public Vector2 normalizedPlayerPosition = Vector2.zero;

    /// <summary>
    /// 게임 상태 변경 이벤트 델리게이트입니다.
    /// </summary>
    public delegate void GameStateChangedHandler(GameState newState);

    public event Action<int> OnYearChanged;

    /// <summary>
    /// 현재 게임 상태입니다.
    /// </summary>
    [Header("Game State")]
    [SerializeField]
    private GameState currentState = GameState.MainMenu;

    public GameState CurrentState => currentState;

    /// <summary>
    /// 현재 게임 연도입니다.
    /// </summary>
    [SerializeField] private int currentYear = 0; // 게임 시작 년도 (수정 가능)

    /// <summary>
    /// 현재 연도 (읽기 전용).
    /// </summary>
    public int CurrentYear => currentYear;

    public int CurrentGlobalWeaponLevel { get; set; }
    public int CurrentGlobalShieldLevel { get; set; }

    /// <summary>
    /// 싱글턴 인스턴스입니다.
    /// </summary>
    public static GameManager Instance { get; private set; }

    public List<PlanetData> PlanetDataList => planetDataList;

    public List<WorldNodeData> WorldNodeDataList => worldNodeDataList;

    public List<WarpNodeData> WarpNodeDataList => warpNodeDataList;

    public int CurrentWarpTargetPlanetId => currentWarpTargetPlanetId;

    public int CurrentWarpNodeId => currentWarpNodeId;

    /// <summary>
    /// 완료된 미스터리 이벤트 ID 목록 (한 번 뜬 미스터리 이벤트는 다시 나오지 않음)
    /// </summary>
    private List<int> completedMysteryEventIds = new();

    /// <summary>
    /// 완료된 미스터리 이벤트 ID 목록을 반환합니다 (읽기 전용)
    /// </summary>
    public List<int> CompletedMysteryEventIds => completedMysteryEventIds.ToList();

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

            Debug.Assert(Constants.Planets.PlanetTotalCount >= 5, "행성 개수는 반드시 5개 이상이어야 합니다.");

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

        playerShip = GameObject.Find("PlayerShip")?.GetComponent<Ship>();
        playerShip.Initialize();

        LoadGameData();

        if (playerShip != null)
        {
            playerShip.isPlayerShip = true; // 유저 함선
            OnShipInitialized?.Invoke();
        }
    }

    private void OnEnable()
    {
        GameEvents.OnItemAcquired += (itemId) => CheckItemQuests();
        GameEvents.OnPirateKilled += OnPirateKilled;
        GameEvents.OnItemRemoved += (itemId) => CheckItemQuests();
    }

    private void OnDestroy()
    {
        GameEvents.OnItemAcquired -= (itemId) => CheckItemQuests();
        GameEvents.OnPirateKilled -= OnPirateKilled;
        GameEvents.OnItemRemoved -= (itemId) => CheckItemQuests();
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
            EventManager.Instance != null
        );
    }

    public Ship CreateDefaultPlayerShip()
    {
        Room cockpit = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Cockpit);
        Room engine = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Engine);
        Room power = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Power);
        Room crewQuarters = GameObjectFactory.Instance.CreateCrewQuartersRoomInstance(CrewQuartersRoomSize.Basic);
        Room teleporter = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Teleporter);
        Room oxygenRoom = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Oxygen);
        Room storageRoom =
            GameObjectFactory.Instance.CreateStorageRoomInstance(StorageType.Regular, StorageSize.Medium);
        Room[] corridors = new Room[8];
        for (int index = 0; index < corridors.Length; index++)
            corridors[index] = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Corridor);

        // Room engine2 = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Engine);
        // Room engine3 = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Engine);
        // playerShip.AddRoom(engine2, new Vector2Int(40, 40), Constants.Rotations.Rotation.Rotation0);
        // playerShip.AddRoom(engine3, new Vector2Int(43, 43), Constants.Rotations.Rotation.Rotation0);


        playerShip.AddRoom(cockpit, new Vector2Int(33, 31), Constants.Rotations.Rotation.Rotation90);
        playerShip.AddRoom(engine, new Vector2Int(31, 33), Constants.Rotations.Rotation.Rotation90);
        playerShip.AddRoom(power, new Vector2Int(32, 28), Constants.Rotations.Rotation.Rotation270);
        playerShip.AddRoom(crewQuarters, new Vector2Int(27, 31), Constants.Rotations.Rotation.Rotation0);
        playerShip.AddRoom(teleporter, new Vector2Int(26, 30), Constants.Rotations.Rotation.Rotation0);
        playerShip.AddRoom(oxygenRoom, new Vector2Int(30, 28), Constants.Rotations.Rotation.Rotation270);
        playerShip.AddRoom(storageRoom, new Vector2Int(27, 29), Constants.Rotations.Rotation.Rotation90);

        playerShip.AddRoom(corridors[0], new Vector2Int(27, 30));
        playerShip.AddRoom(corridors[1], new Vector2Int(28, 30));
        playerShip.AddRoom(corridors[2], new Vector2Int(29, 30));
        playerShip.AddRoom(corridors[3], new Vector2Int(30, 30));
        playerShip.AddRoom(corridors[4], new Vector2Int(31, 30));
        playerShip.AddRoom(corridors[5], new Vector2Int(32, 30));
        playerShip.AddRoom(corridors[6], new Vector2Int(31, 31));
        playerShip.AddRoom(corridors[7], new Vector2Int(32, 31));


        // 6,7,8 미사일 9,10,11 초음속탄 (총알 필요)
        playerShip.AddWeapon(6, new Vector2Int(34, 32), ShipWeaponAttachedDirection.North);
        playerShip.AddWeapon(9, new Vector2Int(34, 29), ShipWeaponAttachedDirection.South);


        CrewBase crewBase1 = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(CrewRace.Human);
        CrewBase crewBase2 = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(CrewRace.Beast);
        CrewBase crewBase3 = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(CrewRace.Insect);
        CrewBase crewBase4 = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(CrewRace.MechanicSup);

        if (crewBase1 is CrewMember crewMember) playerShip.AddCrew(crewMember);
        if (crewBase2 is CrewMember crewMember2) playerShip.AddCrew(crewMember2);
        if (crewBase3 is CrewMember crewMember3) playerShip.AddCrew(crewMember3);
        if (crewBase4 is CrewMember crewMember4) playerShip.AddCrew(crewMember4);

        playerShip.SetOuterHullLevel(0);
        playerShip.UpdateOuterHullVisuals();

        return null;
    }

    public void CreateDefaultEnemyShip(Ship enemyShip)
    {
        Room cockpit = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Cockpit);
        Room engine = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Engine);
        Room power = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Power);
        Room crewQuarters = GameObjectFactory.Instance.CreateCrewQuartersRoomInstance(CrewQuartersRoomSize.Basic);
        Room teleporter = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Teleporter);
        Room[] corridors = new Room[15];

        // Room engine2 = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Engine);
        // Room engine3 = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Engine);
        // enemyShip.AddRoom(engine2, new Vector2Int(40, 40), Constants.Rotations.Rotation.Rotation0);
        // enemyShip.AddRoom(engine3, new Vector2Int(43, 43), Constants.Rotations.Rotation.Rotation0);


        for (int index = 0; index < corridors.Length; index++)
            corridors[index] = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Corridor);


        enemyShip.AddRoom(cockpit, new Vector2Int(35, 31), Constants.Rotations.Rotation.Rotation90);
        enemyShip.AddRoom(engine, new Vector2Int(34, 28), Constants.Rotations.Rotation.Rotation270);
        enemyShip.AddRoom(power, new Vector2Int(33, 33), Constants.Rotations.Rotation.Rotation90);
        enemyShip.AddRoom(crewQuarters, new Vector2Int(32, 26), Constants.Rotations.Rotation.Rotation270);
        enemyShip.AddRoom(teleporter, new Vector2Int(32, 32), Constants.Rotations.Rotation.Rotation90);

        enemyShip.AddRoom(corridors[0], new Vector2Int(31, 32));
        enemyShip.AddRoom(corridors[1], new Vector2Int(31, 31));
        enemyShip.AddRoom(corridors[2], new Vector2Int(31, 30));
        enemyShip.AddRoom(corridors[3], new Vector2Int(32, 31));
        enemyShip.AddRoom(corridors[4], new Vector2Int(32, 30));
        enemyShip.AddRoom(corridors[5], new Vector2Int(33, 30));
        enemyShip.AddRoom(corridors[6], new Vector2Int(34, 31));
        enemyShip.AddRoom(corridors[7], new Vector2Int(34, 30));
        enemyShip.AddRoom(corridors[8], new Vector2Int(33, 31));
        enemyShip.AddRoom(corridors[9], new Vector2Int(31, 33));
        enemyShip.AddRoom(corridors[10], new Vector2Int(32, 33));
        enemyShip.AddRoom(corridors[11], new Vector2Int(31, 34));
        enemyShip.AddRoom(corridors[12], new Vector2Int(32, 34));
        enemyShip.AddRoom(corridors[13], new Vector2Int(31, 35));
        enemyShip.AddRoom(corridors[14], new Vector2Int(32, 35));

        // Room storageRoom = GameObjectFactory.Instance.CreateStorageRoomInstance(StorageType.Regular, StorageSize.Big);
        // Room storageRoom2 =
        //     GameObjectFactory.Instance.CreateStorageRoomInstance(StorageType.Regular, StorageSize.Big);
        //
        // enemyShip.AddRoom(storageRoom, new Vector2Int(27, 26), Constants.Rotations.Rotation.Rotation270);
        // enemyShip.AddRoom(storageRoom2, new Vector2Int(38, 24), Constants.Rotations.Rotation.Rotation90);
        // StorageRoomBase storage = (StorageRoomBase)storageRoom;
        // TradingItem item = GameObjectFactory.Instance.CreateItemInstance(0, 20);
        // TradingItem item2 = GameObjectFactory.Instance.CreateItemInstance(2, 10);
        // TradingItem item3 = GameObjectFactory.Instance.CreateItemInstance(21, 1);
        // storage.AddItem(item, new Vector2Int(0, 0), Constants.Rotations.Rotation.Rotation0);
        // storage.AddItem(item2, new Vector2Int(2, 2), Constants.Rotations.Rotation.Rotation0);
        // StorageRoomBase storage2 = (StorageRoomBase)storageRoom2;
        // storage2.AddItem(item3, new Vector2Int(1, 1), Constants.Rotations.Rotation.Rotation0);
        // Room temp = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Corridor);
        // enemyShip.AddRoom(temp, new Vector2Int(50, 31), Constants.Rotations.Rotation.Rotation90);
        enemyShip.AddWeapon(1, new Vector2Int(35, 33), ShipWeaponAttachedDirection.East);

        // playerShip.AddWeapon(8, new Vector)

        enemyShip.MoveShipToFacingTargetShip(playerShip);

        CrewBase crewBase1 = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(CrewRace.Human, "인간해적", false);
        CrewBase crewBase3 = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(CrewRace.Insect, "벌레해적", false);

        if (crewBase1 is CrewMember crewMember)
        {
            crewMember.gameObject.AddComponent<EnemyController>();
            enemyShip.AddCrew(crewMember);
        }

        if (crewBase3 is CrewMember crewMember3)
        {
            crewMember3.gameObject.AddComponent<EnemyController>();
            enemyShip.AddCrew(crewMember3);
        }

        //
        enemyShip.UpdateOuterHullVisuals();
    }

    public void CreateDefaultEnemyShip2(Ship enemyShip)
    {
        Room cockpit = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Cockpit);
        Room engine = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Engine);
        Room power = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Power, 2);
        Room crewQuarters = GameObjectFactory.Instance.CreateCrewQuartersRoomInstance(CrewQuartersRoomSize.Basic);
        Room teleporter = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Teleporter);
        Room medbay = GameObjectFactory.Instance.CreateRoomInstance(RoomType.MedBay);
        Room oxygen = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Oxygen, 2);
        Room[] corridors = new Room[8];


        for (int index = 0; index < corridors.Length; index++)
            corridors[index] = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Corridor);


        enemyShip.AddRoom(cockpit, new Vector2Int(32, 31), Constants.Rotations.Rotation.Rotation90);
        enemyShip.AddRoom(engine, new Vector2Int(31, 33), Constants.Rotations.Rotation.Rotation90);
        enemyShip.AddRoom(power, new Vector2Int(30, 29), Constants.Rotations.Rotation.Rotation90);
        enemyShip.AddRoom(crewQuarters, new Vector2Int(24, 30), Constants.Rotations.Rotation.Rotation0);
        enemyShip.AddRoom(teleporter, new Vector2Int(28, 32), Constants.Rotations.Rotation.Rotation90);
        enemyShip.AddRoom(medbay, new Vector2Int(28, 28), Constants.Rotations.Rotation.Rotation270);
        enemyShip.AddRoom(oxygen, new Vector2Int(29, 28), Constants.Rotations.Rotation.Rotation270);

        enemyShip.AddRoom(corridors[0], new Vector2Int(31, 30), Constants.Rotations.Rotation.Rotation0);
        enemyShip.AddRoom(corridors[1], new Vector2Int(31, 31), Constants.Rotations.Rotation.Rotation0);
        enemyShip.AddRoom(corridors[2], new Vector2Int(30, 30), Constants.Rotations.Rotation.Rotation0);
        enemyShip.AddRoom(corridors[3], new Vector2Int(29, 30), Constants.Rotations.Rotation.Rotation0);
        enemyShip.AddRoom(corridors[4], new Vector2Int(29, 31), Constants.Rotations.Rotation.Rotation0);
        enemyShip.AddRoom(corridors[5], new Vector2Int(28, 30), Constants.Rotations.Rotation.Rotation0);
        enemyShip.AddRoom(corridors[6], new Vector2Int(28, 30), Constants.Rotations.Rotation.Rotation0);
        enemyShip.AddRoom(corridors[7], new Vector2Int(28, 31), Constants.Rotations.Rotation.Rotation0);


        enemyShip.AddWeapon(0, new Vector2Int(31, 34), ShipWeaponAttachedDirection.North);
        enemyShip.AddWeapon(3, new Vector2Int(31, 26), ShipWeaponAttachedDirection.South);


        // Room storageRoom = GameObjectFactory.Instance.CreateStorageRoomInstance(StorageType.Regular, StorageSize.Big);
        // Room storageRoom2 =
        //     GameObjectFactory.Instance.CreateStorageRoomInstance(StorageType.Regular, StorageSize.Big);
        //
        // enemyShip.AddRoom(storageRoom, new Vector2Int(27, 26), Constants.Rotations.Rotation.Rotation270);
        // enemyShip.AddRoom(storageRoom2, new Vector2Int(38, 24), Constants.Rotations.Rotation.Rotation90);
        // StorageRoomBase storage = (StorageRoomBase)storageRoom;
        // TradingItem item = GameObjectFactory.Instance.CreateItemInstance(0, 20);
        // TradingItem item2 = GameObjectFactory.Instance.CreateItemInstance(2, 10);
        // TradingItem item3 = GameObjectFactory.Instance.CreateItemInstance(21, 1);
        // storage.AddItem(item, new Vector2Int(0, 0), Constants.Rotations.Rotation.Rotation0);
        // storage.AddItem(item2, new Vector2Int(2, 2), Constants.Rotations.Rotation.Rotation0);
        // StorageRoomBase storage2 = (StorageRoomBase)storageRoom2;
        // storage2.AddItem(item3, new Vector2Int(1, 1), Constants.Rotations.Rotation.Rotation0);
        // Room temp = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Corridor);
        // enemyShip.AddRoom(temp, new Vector2Int(50, 31), Constants.Rotations.Rotation.Rotation90);


        // playerShip.AddWeapon(8, new Vector)

        enemyShip.MoveShipToFacingTargetShip(playerShip);

        CrewBase crewBase1 = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(CrewRace.Human, "인간해적", false);
        CrewBase crewBase3 = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(CrewRace.Insect, "벌레해적", false);

        if (crewBase1 is CrewMember crewMember)
        {
            crewMember.gameObject.AddComponent<EnemyController>();
            enemyShip.AddCrew(crewMember);
        }

        if (crewBase3 is CrewMember crewMember3)
        {
            crewMember3.gameObject.AddComponent<EnemyController>();
            enemyShip.AddCrew(crewMember3);
        }

        enemyShip.UpdateOuterHullVisuals();
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
                    Time.timeScale = 1f;
                    break;
                case GameState.Gameplay:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.Warp:
                    Time.timeScale = 1f;
                    break;
                case GameState.GameOver:
                    HandleGameOver();
                    break;
                case GameState.Planet:
                    Time.timeScale = 1f;
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
        // currentYear++;
        //TestCode :
        currentYear += 100;
        OnYearChanged?.Invoke(currentYear);
        Debug.Log($"[워프 완료] 현재 연도 : {currentYear}");
    }

    public event Action<int> OnlyWarpEffect;

    public void WarpEffect()
    {
        OnlyWarpEffect?.Invoke(currentYear);
    }

    #region 게임 데이터 관련

    // TODO: 게임 데이터 초기화 로직 및 진짜 새로운 게임 시작할 건지 물어야함.

    /// <summary>
    /// 메인 화면에서 새로운 시작을 누르면 호출되는 함수. 행성 데이터를 삭제 후 게임 시작
    /// </summary>
    public void StartNewGame()
    {
        DeleteGameData();
        StartCoroutine(DelayedStartNewGame());
    }

    private IEnumerator DelayedStartNewGame()
    {
        yield return new WaitForSeconds(0.5f);
        CreateDefaultPlayerShip();
        playerShip.isPlayerShip = true;
        OnShipInitialized?.Invoke();

        GeneratePlanetsData(); // 새 데이터 생성

        currentState = GameState.Gameplay;
        SceneChanger.Instance.LoadScene("Idle");
    }

    /// <summary>
    /// 메인 화면에서 이어하기를 누르면 호출되는 함수. 행성 데이터를 로드 후 게임 시작
    /// </summary>
    public void ContinueGame()
    {
        // 저장된 상태에 따라 적절한 씬으로 이동
        switch (currentState)
        {
            case GameState.Gameplay:
                SceneChanger.Instance.LoadScene("Idle");
                break;

            case GameState.Warp:
                SceneChanger.Instance.LoadScene("Idle"); // 워프 UI는 Idle 씬에서 표시
                break;

            case GameState.Combat:
                SceneChanger.Instance.LoadScene("Combat");
                break;

            case GameState.Planet:
                SceneChanger.Instance.LoadScene("Planet");
                break;

            default:
                // 기본적으로 Idle 씬으로 이동
                SceneChanger.Instance.LoadScene("Idle");
                break;
        }
    }

    /// <summary>
    /// 게임 전체를 저장하는 함수. 워프를 할 때마다 떠야할 것이다.
    /// </summary>
    public void SaveGameData()
    {
        Debug.Log("저장 시작");
        SaveWorldMap();
        SaveWarpMap();
        SavePlayerData();
        // TODO : 현재 배, 재화, 플레이어 데이터 등 게임 플레이에 관련된 모든 것을 저장하는 함수.
    }

    /// <summary>
    /// 게임 전체를 불러오는 함수.
    /// </summary>
    public void LoadGameData()
    {
        LoadWorldMap();
        LoadPlayerData();
        LoadWarpMap();

        ;
        // TODO : 현재 배, 재화, 플레이어 데이터 등 게임 플레이에 관련된 모든 것을 저장하는 함수.
    }

    public void DeleteGameData()
    {
        DeleteWorldMap();
        DeletePlayerData();
        DeleteWarpMap();
    }

    /// <summary>
    /// 엔딩과 관련된 플레이어 데이터를 저장하는 함수.
    /// </summary>
    public void SavePlayerData()
    {
        // 플레이어 데이터 (엔딩 관련)
        ES3.Save<PlayerData>("playerData", playerData);

        // 게임 스테이트
        ES3.Save<GameState>("gameState", currentState);

        // 함선
        ShipSerialization.SaveShip(playerShip, "playerShip");

        // 사기 효과
        ES3.Save<List<MoraleEffectData>>("moraleEffect", MoraleManager.Instance.activeEffects);

        // 년도
        ES3.Save<int>("currentYear", currentYear);

        // 재화
        ES3.Save<List<ResourcesData>>("resources", ResourceManager.Instance.resources);

        // 공용 장비 레벨
        ES3.Save<int>("globalWeaponLevel", CurrentGlobalWeaponLevel);
        ES3.Save<int>("globalShieldLevel", CurrentGlobalShieldLevel);

        // 행성에서 장비 구매했는지 여부
        ES3.Save<bool>("isBoughtEquipment", isBoughtEquipment);

        SaveBlueprintData();
    }

    /// <summary>
    /// 엔딩과 관련된 플레이어 데이터를 불러오는 함수.
    /// </summary>
    public void LoadPlayerData()
    {
        // 유저 데이터 (엔딩 관련)
        if (ES3.KeyExists("playerData")) playerData = ES3.Load<PlayerData>("playerData");

        // 완료된 미스터리 이벤트 ID 목록 로드
        if (ES3.KeyExists("completedMysteryEventIds"))
        {
            completedMysteryEventIds = ES3.Load<List<int>>("completedMysteryEventIds");
            playerData.mysteryFound = completedMysteryEventIds.Count;
        }

        // 게임 스테이트
        if (ES3.KeyExists("gameState")) currentState = ES3.Load<GameState>("gameState");

        // 현재 년도
        if (ES3.KeyExists("currentYear")) currentYear = ES3.Load<int>("currentYear");

        // 사기 효과
        if (ES3.KeyExists("moraleEffect"))
        {
            MoraleManager.Instance.activeEffects = ES3.Load<List<MoraleEffectData>>("moraleEffect");

            MoraleManager.Instance.ReapplyLoadedEffects(); // 로드 직후 효과 재적용
        }

        // 재화
        if (ES3.KeyExists("resources")) ResourceManager.Instance.resources = ES3.Load<List<ResourcesData>>("resources");

        // 공용 장비 레벨
        if (ES3.KeyExists("globalWeaponLevel")) CurrentGlobalWeaponLevel = ES3.Load<int>("globalWeaponLevel");
        if (ES3.KeyExists("globalShieldLevel")) CurrentGlobalShieldLevel = ES3.Load<int>("globalShieldLevel");

        // 행성에서 장비 구매했는지 여부
        if (ES3.KeyExists("isBoughtEquipment")) isBoughtEquipment = ES3.Load<bool>("isBoughtEquipment");

        // 망할놈의 함선
        if (ES3.FileExists("playerShip"))
        {
            Debug.Log("소환시도");

            ShipSerialization.LoadShip("playerShip");
            // OnShipInitialized?.Invoke();
        }
        else
        {
            Debug.Log("없어서 기본함선 만듦");
            CreateDefaultPlayerShip();
            // OnShipInitialized?.Invoke();
        }

        LoadBlueprintData();
    }

    public void DeletePlayerData()
    {
        // 함선
        ES3.DeleteFile("playerShip");
        playerShip.RemoveAllCrews();
        playerShip.RemoveAllRooms();
        playerShip.RemoveAllWeapons();
        playerShip.RemoveAllItems();
        playerShip.ClearExistingHulls();
        playerShip.Initialize();

        ES3.DeleteKey("completedMysteryEventIds");
        completedMysteryEventIds.Clear();

        // 사기 효과
        ES3.DeleteFile("moraleEffect");
        MoraleManager.Instance.ResetAllMorale();

        // 현재 년도
        ES3.DeleteFile("currentYear");
        currentYear = 0;

        // 완료된 미스터리 이벤트 ID 목록 저장
        ES3.Save<List<int>>("completedMysteryEventIds", completedMysteryEventIds);

        // 플레이어 데이터 (엔딩 관련)
        ES3.DeleteKey("playerData");
        playerData.ResetPlayerData();


        // 게임 스테이트
        ES3.DeleteKey("gameState");
        ChangeGameState(GameState.MainMenu);

        // 재화
        ES3.DeleteKey("resources");
        ResourceManager.Instance.ResetResources();

        // 공용 장비 레벨
        ES3.DeleteKey("globalWeaponLevel");
        CurrentGlobalWeaponLevel = 0;

        ES3.DeleteKey("globalShieldLevel");
        CurrentGlobalShieldLevel = 0;

        // 행성 씬 장비 구매 여부
        ES3.DeleteKey("isBoughtEquipment");
        isBoughtEquipment = false;

        DeleteBlueprintData();
    }

    #endregion


    #region 행성 맵

    public void SaveWorldMap()
    {
        ES3.Save("planetList", planetDataList);
        ES3.Save("worldNodeList", worldNodeDataList);
        ES3.Save("currentPosition", normalizedPlayerPosition);
    }

    public void LoadWorldMap()
    {
        if (ES3.KeyExists("planetList"))
        {
            planetDataList = ES3.Load<List<PlanetData>>("planetList");
            foreach (PlanetData planet in planetDataList)
            {
                planet.currentSprite=Resources.Load<Sprite>($"Sprites/Planet/{planet.GetSpeciesPrefix(planet.PlanetRace)}");
            }

            if (ES3.KeyExists("worldNodeList"))
                worldNodeDataList = ES3.Load<List<WorldNodeData>>("worldNodeList");

            if (ES3.KeyExists("currentPosition"))
                normalizedPlayerPosition = ES3.Load<Vector2>("currentPosition");
        }
        else
        {
            // 데이터가 없으면 새로 생성 후 저장
            GeneratePlanetsData();
            SaveWorldMap();
        }
    }

    public void DeleteWorldMap()
    {
        ES3.DeleteKey("planetList");
        ES3.DeleteKey("worldNodeList");
        ES3.DeleteKey("currentPosition");
    }

    private void GeneratePlanetsData()
    {
        planetDataList.Clear();
        worldNodeDataList.Clear();

        normalizedPlayerPosition =
            new Vector2(
                Random.Range(Constants.Planets.PlanetCurrentPositionIndicatorSize * 2,
                    1 - Constants.Planets.PlanetCurrentPositionIndicatorSize * 2),
                Random.Range(Constants.Planets.PlanetCurrentPositionIndicatorSize * 2,
                    1 - Constants.Planets.PlanetCurrentPositionIndicatorSize * 2));

        for (int index = 0; index < Constants.Planets.PlanetTotalCount; index++)
        {
            PlanetData newData = new();
            newData.CreateRandomData();
            planetDataList.Add(newData);
        }
    }

    public PlanetData GetRandomPlanetData()
    {
        int index = Random.Range(0, planetDataList.Count);
        return planetDataList[index];
    }

    #endregion

    #region 워프 맵

    // 워프맵 저장
    public void SaveWarpMap()
    {
        if (warpNodeDataList.Count > 0)
        {
            ES3.Save("currentWarpNodes", warpNodeDataList);
            ES3.Save("currentWarpTargetPlanetId", currentWarpTargetPlanetId);
            ES3.Save("currentWarpNodeId", currentWarpNodeId);
            Debug.Log($"워프맵 저장: {warpNodeDataList.Count}개 노드");
        }
    }

    // 워프맵 로드
    public void LoadWarpMap()
    {
        if (ES3.KeyExists("currentWarpNodes"))
        {
            warpNodeDataList = ES3.Load<List<WarpNodeData>>("currentWarpNodes");

            if (ES3.KeyExists("currentWarpTargetPlanetId"))
                currentWarpTargetPlanetId = ES3.Load<int>("currentWarpTargetPlanetId");

            if (ES3.KeyExists("currentWarpNodeId"))
                currentWarpNodeId = ES3.Load<int>("currentWarpNodeId");

            Debug.Log($"워프맵 로드: {warpNodeDataList.Count}개 노드");
            Debug.Log($"타겟 행성 ID : {currentWarpTargetPlanetId}");
        }
        else
        {
            warpNodeDataList.Clear();
            currentWarpTargetPlanetId = -1;
        }
    }

    // 워프맵 삭제
    public void DeleteWarpMap()
    {
        ES3.DeleteKey("currentWarpNodes");
        ES3.DeleteKey("currentWarpTargetPlanetId");
        warpNodeDataList.Clear();
        currentWarpTargetPlanetId = -1;
        Debug.Log("워프맵 데이터 삭제");
    }

    public void SetCurrentWarpMap(List<WarpNodeData> nodes, int targetPlanetId)
    {
        warpNodeDataList = new List<WarpNodeData>(nodes);
        currentWarpTargetPlanetId = targetPlanetId;
    }

    public void SetCurrentWarpNodeId(int nodeId)
    {
        currentWarpNodeId = nodeId;
    }

    // 워프맵 클리어
    public void ClearCurrentWarpMap()
    {
        warpNodeDataList.Clear();
        currentWarpTargetPlanetId = -1;
    }

    public void SetCurrentWarpTargetPlanetId(int planetId)
    {
        currentWarpTargetPlanetId = planetId;
    }

    public void LandOnPlanet()
    {
        ChangeGameState(GameState.Planet);
        SceneChanger.Instance.LoadScene("Planet");
    }

    public void ShowEnding()
    {
        SceneChanger.Instance.LoadScene("EndingScene");
    }

    #endregion

    #region 행성씬전용

    public Dictionary<int, List<CrewCardSaveData>> employCardsPerPlanet = new();
    public bool isBoughtEquipment = false;

    public PlanetData WhereIAm()
    {
        PlanetData planetData = planetDataList[currentWarpTargetPlanetId];
        return planetData;
    }

    #endregion

    #region 퀘스트

    public void OnPirateKilled()
    {
        playerData.pirateDefeated++;
        CheckPirateQuests();
    }

    /// <summary>
    /// 해적 처치 시 모든 해적 잡기 퀘스트를 체크합니다.
    /// </summary>
    public void CheckPirateQuests()
    {
        foreach (PlanetData planet in planetDataList)
            foreach (RandomQuest quest in planet.questList.Where(q =>
                         q.objectives[0].objectiveType == QuestObjectiveType.PirateHunt &&
                         q.status == QuestStatus.Active))
            {
                quest.objectives[0].currentAmount++;

                if (quest.objectives[0].currentAmount >= quest.objectives[0].amount) quest.SetCanComplete(true);
            }
    }

    /// <summary>
    /// 모든 아이템 관련 퀘스트를 체크합니다.
    /// </summary>
    public void CheckItemQuests()
    {
        foreach (PlanetData planet in planetDataList)
            foreach (RandomQuest quest in planet.questList.Where(q =>
                         (q.objectives[0].objectiveType == QuestObjectiveType.ItemTransport ||
                          q.objectives[0].objectiveType == QuestObjectiveType.ItemProcurement) &&
                         q.status == QuestStatus.Active))
            {
                int targetId = quest.objectives[0].targetId;
                int targetAmount = quest.objectives[0].amount;

                // ID와 양이 정확히 일치하는 아이템이 있는지 체크
                bool hasMatchingItem = playerShip.GetAllItems()
                    .Any(i => i.GetItemId() == targetId && i.GetItemData().amount == targetAmount);

                quest.SetCanComplete(hasMatchingItem);
            }
    }

    #endregion

    #region Blueprint 데이터 관리

    /// <summary>
    /// Blueprint 데이터를 JSON 파일로 저장합니다.
    /// </summary>
    public void SaveBlueprintData()
    {
        try
        {
            if (BlueprintSlotManager.Instance != null)
            {
                BlueprintSlotSaveWrapper wrapper = new()
                {
                    blueprintSlots = BlueprintSlotManager.Instance.blueprintSlots,
                    isValidBP = BlueprintSlotManager.Instance.isValidBP,
                    currentSlotIndex = BlueprintSlotManager.Instance.currentSlotIndex,
                    appliedSlotIndex = BlueprintSlotManager.Instance.appliedSlotIndex,
                    occupiedTilesPerSlot = new List<List<Vector2Int>>()
                };

                // HashSet을 List로 변환하여 저장
                foreach (HashSet<Vector2Int> tileSet in BlueprintSlotManager.Instance.occupiedTilesPerSlot)
                    wrapper.occupiedTilesPerSlot.Add(new List<Vector2Int>(tileSet));

                string json = JsonUtility.ToJson(wrapper);
                System.IO.File.WriteAllText(Application.persistentDataPath + "/blueprints.json", json);

                Debug.Log("Blueprint 데이터 저장 완료");
            }
            else
            {
                Debug.LogWarning("BlueprintSlotManager가 존재하지 않아 Blueprint 데이터를 저장할 수 없습니다.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Blueprint 데이터 저장 중 오류 발생: {e.Message}");
        }
    }

    /// <summary>
    /// Blueprint 데이터를 JSON 파일에서 로드합니다.
    /// </summary>
    public void LoadBlueprintData()
    {
        try
        {
            string path = Application.persistentDataPath + "/blueprints.json";

            if (!System.IO.File.Exists(path))
            {
                Debug.Log("Blueprint 저장 파일이 존재하지 않습니다.");
                return;
            }

            string json = System.IO.File.ReadAllText(path);
            BlueprintSlotSaveWrapper wrapper = JsonUtility.FromJson<BlueprintSlotSaveWrapper>(json);

            if (BlueprintSlotManager.Instance != null)
            {
                // 데이터 복원
                BlueprintSlotManager.Instance.blueprintSlots = wrapper.blueprintSlots ?? new List<BlueprintSaveData>();
                BlueprintSlotManager.Instance.isValidBP = wrapper.isValidBP ?? new List<bool>();
                BlueprintSlotManager.Instance.currentSlotIndex = wrapper.currentSlotIndex;
                BlueprintSlotManager.Instance.appliedSlotIndex = wrapper.appliedSlotIndex;

                // occupiedTilesPerSlot 복원
                BlueprintSlotManager.Instance.occupiedTilesPerSlot = new List<HashSet<Vector2Int>>();

                if (wrapper.occupiedTilesPerSlot != null)
                    foreach (List<Vector2Int> tileList in wrapper.occupiedTilesPerSlot)
                        BlueprintSlotManager.Instance.occupiedTilesPerSlot.Add(new HashSet<Vector2Int>(tileList));

                // 4칸 보장 (누락된 슬롯이 있으면 빈 데이터로 채움)
                while (BlueprintSlotManager.Instance.blueprintSlots.Count < 4)
                    BlueprintSlotManager.Instance.blueprintSlots.Add(
                        new BlueprintSaveData(new List<BlueprintRoomSaveData>(), new List<BlueprintWeaponSaveData>(),
                            -1));

                while (BlueprintSlotManager.Instance.occupiedTilesPerSlot.Count < 4)
                    BlueprintSlotManager.Instance.occupiedTilesPerSlot.Add(new HashSet<Vector2Int>());

                while (BlueprintSlotManager.Instance.isValidBP.Count < 4)
                    BlueprintSlotManager.Instance.isValidBP.Add(false);

                Debug.Log("Blueprint 데이터 로드 완료");
            }
            else
            {
                Debug.LogWarning("BlueprintSlotManager가 존재하지 않아 Blueprint 데이터를 로드할 수 없습니다.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Blueprint 데이터 로드 중 오류 발생: {e.Message}");
        }
    }

    /// <summary>
    /// Blueprint 데이터 파일을 삭제하고 SlotManager 데이터를 초기화합니다.
    /// </summary>
    public void DeleteBlueprintData()
    {
        try
        {
            string path = Application.persistentDataPath + "/blueprints.json";

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
                Debug.Log("Blueprint 데이터 파일 삭제 완료");
            }

            // SlotManager가 존재하면 데이터 초기화
            if (BlueprintSlotManager.Instance != null)
            {
                BlueprintSlotManager.Instance.currentSlotIndex = -1;
                BlueprintSlotManager.Instance.appliedSlotIndex = 0;

                // 4칸 빈 데이터로 초기화
                BlueprintSlotManager.Instance.blueprintSlots.Clear();
                BlueprintSlotManager.Instance.occupiedTilesPerSlot.Clear();
                BlueprintSlotManager.Instance.isValidBP.Clear();

                for (int i = 0; i < 4; i++)
                {
                    BlueprintSlotManager.Instance.blueprintSlots.Add(
                        new BlueprintSaveData(new List<BlueprintRoomSaveData>(), new List<BlueprintWeaponSaveData>(),
                            -1));
                    BlueprintSlotManager.Instance.occupiedTilesPerSlot.Add(new HashSet<Vector2Int>());
                    BlueprintSlotManager.Instance.isValidBP.Add(false);
                }

                Debug.Log("BlueprintSlotManager 데이터 초기화 완료");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Blueprint 데이터 삭제 중 오류 발생: {e.Message}");
        }
    }

    #endregion

    /// <summary>
    /// 미스터리 이벤트 완료 시 호출하여 ID를 기록합니다
    /// </summary>
    /// <param name="eventId">완료된 미스터리 이벤트 ID</param>
    public void AddCompletedMysteryEvent(int eventId)
    {
        if (!completedMysteryEventIds.Contains(eventId))
        {
            completedMysteryEventIds.Add(eventId);
            playerData.mysteryFound++;
            Debug.Log($"미스터리 이벤트 완료 기록: ID {eventId}");
        }
    }

    /// <summary>
    /// 특정 미스터리 이벤트가 이미 완료되었는지 확인합니다
    /// </summary>
    /// <param name="eventId">확인할 이벤트 ID</param>
    /// <returns>완료된 경우 true</returns>
    public bool IsMysteryEventCompleted(int eventId)
    {
        return completedMysteryEventIds.Contains(eventId);
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

    /// <summary>
    /// 적 함선 만난 상태
    /// </summary>
    Combat,

    /// <summary>
    /// 행성에 있는 상태
    /// </summary>
    Planet,

    /// <summary>
    /// 도안 작성 상태
    /// </summary>
    Customize,

    /// <summary>게임이 일시정지된 상태입니다.</summary>
    Paused,

    /// <summary>게임 오버 상태입니다.</summary>
    GameOver,

    /// <summary>
    /// 전투에서 진 패잔병입니다.
    /// </summary>
    Looser
}
