using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    [SerializeField] private GameObject crewManagerPrefab;
    [SerializeField] private GameObject eventManagerPrefab;
    [SerializeField] private GameObject gameManagerPrefab;
    [SerializeField] private GameObject inventoryManagerPrefab;
    [SerializeField] private GameObject questManagerPrefab;
    [SerializeField] private GameObject resourceManagerPrefab;
    [SerializeField] private GameObject shipManagerPrefab;
    [SerializeField] private GameObject uiManagerPrefab;

    private void Awake()
    {
        // 필요한 매니저들 초기화 (실제 게임에서는 Prefab 또는 Resources에서 로드)
        InstantiateIfNotExists<GameManager>(gameManagerPrefab);
        InstantiateIfNotExists<ResourceManager>(resourceManagerPrefab);
        InstantiateIfNotExists<CrewManager>(crewManagerPrefab);
        InstantiateIfNotExists<ShipManager>(shipManagerPrefab);
        InstantiateIfNotExists<EventManager>(eventManagerPrefab);
        InstantiateIfNotExists<QuestManager>(questManagerPrefab);
        InstantiateIfNotExists<InventoryManager>(inventoryManagerPrefab);
        InstantiateIfNotExists<UIManager>(uiManagerPrefab);
    }

    private T InstantiateIfNotExists<T>(GameObject prefab) where T : MonoBehaviour
    {
        var existing = FindAnyObjectByType<T>();
        if (existing == null && prefab != null)
        {
            var obj = Instantiate(prefab);
            obj.name = typeof(T).Name;
            return obj.GetComponent<T>();
        }

        return existing;
    }
}
