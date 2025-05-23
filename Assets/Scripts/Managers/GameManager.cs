using System;
using System.Collections;
using System.Collections.Generic;
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

    /// <summary>
    /// 싱글턴 인스턴스입니다.
    /// </summary>
    public static GameManager Instance { get; private set; }

    public List<PlanetData> PlanetDataList => planetDataList;

    public List<WorldNodeData> WorldNodeDataList => worldNodeDataList;

    public List<WarpNodeData> WarpNodeDataList => warpNodeDataList;

    public int CurrentWarpTargetPlanetId => currentWarpTargetPlanetId;

    public int CurrentWarpNodeId => currentWarpNodeId;

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

        // currentEnemyShip = GameObject.Find("EnemyShip")?.GetComponent<Ship>();
        //
        //
        // if (currentEnemyShip != null)
        // {
        //     currentEnemyShip.Initialize();
        //     currentEnemyShip.isPlayerShip = false;
        //     GameObjectFactory.Instance.EnemyShipFactory.SpawnPirateShip("combat_test");
        // }

        // 기존으로 돌릴라면 아래 3개 주석 처리


        // // 이동 명령
        // StartCoroutine(MoveTest());

        // // 선행 피격 테스트 - 이거 되면 사고임
        // StartCoroutine(PreviousMissileTest());

        // // 미사일 피격 테스트
        // StartCoroutine(DelayedMissileTest());
    }

    // 피격 테스트
    private Room WhereToGo(RoomType type)
    {
        List<Room> rooms = playerShip.GetAllRooms();
        List<Room> canGo = new();
        foreach (Room room in rooms)
            if (room.GetIsDamageable() && room.roomType == type)
                canGo.Add(room);

        return canGo[Random.Range(0, canGo.Count)];
    }

    // 피격 테스트
    private IEnumerator MoveTest()
    {
        yield return new WaitForSeconds(7f);

        foreach (CrewMember crew in playerShip.allCrews)
            RTSSelectionManager.Instance.IssueMoveCommand(WhereToGo(RoomType.Engine));
    }

    // 선 피격 테스트
    private IEnumerator PreviousMissileTest()
    {
        yield return new WaitForSeconds(7.1f);

        Debug.LogError("엔진룸 (26, 33) 미사일 피격!!");
        playerShip.TakeAttack(30f, ShipWeaponType.Missile, new Vector2Int(26, 33));
    }

    // 피격 테스트
    private IEnumerator DelayedMissileTest()
    {
        yield return new WaitForSeconds(8f);

        Debug.LogError("복도 3개 (28~30, 32) 미사일 피격!!");
        playerShip.TakeAttack(110f, ShipWeaponType.Laser, new Vector2Int(28, 32));
        playerShip.TakeAttack(110f, ShipWeaponType.Laser, new Vector2Int(29, 32));
        playerShip.TakeAttack(110f, ShipWeaponType.Laser, new Vector2Int(30, 32));
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

        // Room engine2 = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Engine);
        // Room engine3 = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Engine);
        // playerShip.AddRoom(engine2, new Vector2Int(40, 40), Constants.Rotations.Rotation.Rotation0);
        // playerShip.AddRoom(engine3, new Vector2Int(43, 43), Constants.Rotations.Rotation.Rotation0);


        for (int index = 0; index < corridors.Length; index++)
            corridors[index] = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Corridor);


        playerShip.AddRoom(cockpit, new Vector2Int(35, 31), Constants.Rotations.Rotation.Rotation90);
        playerShip.AddRoom(engine, new Vector2Int(34, 28), Constants.Rotations.Rotation.Rotation270);
        playerShip.AddRoom(power, new Vector2Int(33, 33), Constants.Rotations.Rotation.Rotation90);
        playerShip.AddRoom(crewQuarters, new Vector2Int(32, 26), Constants.Rotations.Rotation.Rotation270);
        playerShip.AddRoom(teleporter, new Vector2Int(32, 32), Constants.Rotations.Rotation.Rotation90);

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

        // Room storageRoom = GameObjectFactory.Instance.CreateStorageRoomInstance(StorageType.Regular, StorageSize.Big);
        // Room storageRoom2 =
        //     GameObjectFactory.Instance.CreateStorageRoomInstance(StorageType.Regular, StorageSize.Big);
        //
        // playerShip.AddRoom(storageRoom, new Vector2Int(27, 26), Constants.Rotations.Rotation.Rotation270);
        // playerShip.AddRoom(storageRoom2, new Vector2Int(38, 24), Constants.Rotations.Rotation.Rotation90);
        // StorageRoomBase storage = (StorageRoomBase)storageRoom;
        // TradingItem item = GameObjectFactory.Instance.CreateItemInstance(0, 20);
        // TradingItem item2 = GameObjectFactory.Instance.CreateItemInstance(2, 10);
        // TradingItem item3 = GameObjectFactory.Instance.CreateItemInstance(21, 1);
        // storage.AddItem(item, new Vector2Int(0, 0), Constants.Rotations.Rotation.Rotation0);
        // storage.AddItem(item2, new Vector2Int(2, 2), Constants.Rotations.Rotation.Rotation0);
        // StorageRoomBase storage2 = (StorageRoomBase)storageRoom2;
        // storage2.AddItem(item3, new Vector2Int(1, 1), Constants.Rotations.Rotation.Rotation0);
        // Room temp = GameObjectFactory.Instance.CreateRoomInstance(RoomType.Corridor);
        // playerShip.AddRoom(temp, new Vector2Int(50, 31), Constants.Rotations.Rotation.Rotation90);
        playerShip.AddWeapon(1, new Vector2Int(35, 33), ShipWeaponAttachedDirection.East);


        CrewBase crewBase1 = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(CrewRace.Human);
        CrewBase crewBase2 = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(CrewRace.Beast);
        CrewBase crewBase3 = GameObjectFactory.Instance.CrewFactory.CreateCrewInstance(CrewRace.Insect);

        if (crewBase1 is CrewMember crewMember) playerShip.AddCrew(crewMember);
        if (crewBase2 is CrewMember crewMember2) playerShip.AddCrew(crewMember2);
        if (crewBase3 is CrewMember crewMember3) playerShip.AddCrew(crewMember3);

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

    #region 게임 데이터 관련

    // TODO: 게임 데이터 초기화 로직 및 진짜 새로운 게임 시작할 건지 물어야함.

    /// <summary>
    /// 메인 화면에서 새로운 시작을 누르면 호출되는 함수. 행성 데이터를 삭제 후 게임 시작
    /// </summary>
    public void StartNewGame()
    {
        DeleteGameData();
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

            case GameState.Event:
                SceneChanger.Instance.LoadScene("Idle");
                break;

            default:
                // 기본적으로 Idle 씬으로 이동
                SceneChanger.Instance.LoadScene("Idle");
                break;
        }

        SceneChanger.Instance.LoadScene("Idle");
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
    }

    /// <summary>
    /// 엔딩과 관련된 플레이어 데이터를 불러오는 함수.
    /// </summary>
    public void LoadPlayerData()
    {
        // 유저 데이터 (엔딩 관련)
        if (ES3.KeyExists("playerData")) playerData = ES3.Load<PlayerData>("playerData");

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


        // 망할놈의 함선
        if (ES3.FileExists("playerShip"))
        {
            Debug.Log("소환시도");

            ShipSerialization.LoadShip("playerShip");
            OnShipInitialized?.Invoke();
        }
        else
        {
            Debug.Log("없어서 기본함선 만듦");
            CreateDefaultPlayerShip();
            OnShipInitialized?.Invoke();
        }
    }

    public void DeletePlayerData()
    {
        // 함선
        ES3.DeleteFile("playerShip");
        playerShip.RemoveAllCrews();
        playerShip.RemoveAllRooms();
        playerShip.RemoveAllWeapons();
        playerShip.RemoveAllItems();
        playerShip.Initialize();

        // 사기 효과
        ES3.DeleteFile("moraleEffect");
        MoraleManager.Instance.ResetAllMorale();

        // 현재 년도
        ES3.DeleteFile("currentYear");
        currentYear = 0;

        // 플레이어 데이터 (엔딩 관련)
        ES3.DeleteKey("playerData");
        playerData.ResetPlayerData();

        // 게임 스테이트
        ES3.DeleteKey("gameState");
        ChangeGameState(GameState.MainMenu);

        // 재화
        ES3.DeleteKey("resources");
        ResourceManager.Instance.ResetResources();
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

    #endregion

    #region 행성씬전용

    public Dictionary<int, List<CrewCardSaveData>> employCardsPerPlanet = new();

    public PlanetData WhereIAm()
    {
        Debug.LogError($"현재 행성 : {currentWarpTargetPlanetId}");
        PlanetData planetData = planetDataList[currentWarpTargetPlanetId];
        return planetData;
    }

    #endregion
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
    GameOver
}
