using UnityEngine;

public class GameObjectFactory : MonoBehaviour
{
    public static GameObjectFactory Instance { get; private set; }

    [SerializeField] private GameObject crewFactoryPrefab;
    [SerializeField] private GameObject itemFactoryPrefab;
    [SerializeField] private GameObject shipWeaponFactoryPrefab;

    public CrewFactory CrewFactory { get; private set; }
    public ItemFactory ItemFactory { get; private set; }
    public ShipWeaponFactory ShipWeaponFactory { get; private set; }

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
    }
}
