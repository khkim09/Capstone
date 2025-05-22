using UnityEngine;

public class GameObjectFactory : MonoBehaviour
{
    public static GameObjectFactory Instance { get; private set; }

    [SerializeField] private GameObject crewFactoryPrefab;
    [SerializeField] private GameObject itemFactoryPrefab;
    [SerializeField] private GameObject shipWeaponFactoryPrefab;
    [SerializeField] private GameObject roomFactory;
    [SerializeField] private GameObject shipFactory;
    [SerializeField] private GameObject enemyShipFactory;

    public CrewFactory CrewFactory { get; private set; }
    public ItemFactory ItemFactory { get; private set; }
    public ShipWeaponFactory ShipWeaponFactory { get; private set; }
    public RoomFactory RoomFactory { get; private set; }
    public ShipFactory ShipFactory { get; private set; }
    public EnemyShipFactory EnemyShipFactory { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFactories();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeFactories()
    {
        // 프리팹에서 팩토리 인스턴스 생성
        GameObject crewFactoryObj = Instantiate(crewFactoryPrefab, transform);
        CrewFactory = crewFactoryObj.GetComponent<CrewFactory>();

        GameObject itemFactoryObj = Instantiate(itemFactoryPrefab, transform);
        ItemFactory = itemFactoryObj.GetComponent<ItemFactory>();

        GameObject weaponFactoryObj = Instantiate(shipWeaponFactoryPrefab, transform);
        ShipWeaponFactory = weaponFactoryObj.GetComponent<ShipWeaponFactory>();

        GameObject roomFactoryObj = Instantiate(roomFactory, transform);
        RoomFactory = roomFactoryObj.GetComponent<RoomFactory>();

        GameObject shipFactoryObj = Instantiate(shipFactory, transform);
        ShipFactory = shipFactoryObj.GetComponent<ShipFactory>();

        GameObject enemyShipFactoryObj = Instantiate(enemyShipFactory, transform);
        EnemyShipFactory = enemyShipFactoryObj.GetComponent<EnemyShipFactory>();
    }

    #region Crew

    public CrewBase CreateCrewInstance(CrewRace race, string name = null, bool isPlayerControlled = true)
    {
        return CrewFactory.CreateCrewInstance(race, name, isPlayerControlled);
    }

    public CrewBase CreateCrewObject(CrewBase crew)
    {
        return CrewFactory.CreateCrewObject(crew);
    }

    #endregion

    #region Room

    public Room CreateRoomObject(Room sourceRoom)
    {
        return RoomFactory.CreateRoomObject(sourceRoom);
    }

    public Room CreateRoomInstance(RoomType roomType, int level = 1)
    {
        return RoomFactory.CreateRoomInstance(roomType, level);
    }

    public StorageRoomBase CreateStorageRoomInstance(StorageType storageType, StorageSize size, int level = 1)
    {
        return RoomFactory.CreateStorageRoomInstance(storageType, size, level);
    }

    public LifeSupportRoom CreateLifeSupportRoomInstance(LifeSupportRoomType facilityType, int level = 1)
    {
        return RoomFactory.CreateLifeSupportRoomInstance(facilityType, level);
    }

    public CrewQuartersRoom CreateCrewQuartersRoomInstance(CrewQuartersRoomSize size, int level = 1)
    {
        return RoomFactory.CreateCrewQuartersRoomInstance(size, level);
    }

    #endregion

    #region TradingItem

    public GameObject CreateItemObject(int itemId, int quantity, Vector2Int position = new())
    {
        return ItemFactory.CreateItemObject(itemId, quantity, position);
    }

    public TradingItem CreateItemObject(TradingItem item)
    {
        return ItemFactory.CreateItemObject(item);
    }

    public TradingItem CreateItemInstance(int itemId, int quantity)
    {
        return ItemFactory.CreateItemInstance(itemId, quantity);
    }

    #endregion

    #region ShipWeapon

    public ShipWeapon CreateWeaponInstance(int weaponId)
    {
        return ShipWeaponFactory.CreateWeaponInstance(weaponId);
    }


    public ShipWeapon CreateWeaponObject(ShipWeapon shipWeapon)
    {
        return ShipWeaponFactory.CreateWeaponObject(shipWeapon);
    }

    #endregion

    public Ship CreateEnemyShipInstance(string shipName)
    {
        return EnemyShipFactory.SpawnPirateShip(shipName);
    }
}
