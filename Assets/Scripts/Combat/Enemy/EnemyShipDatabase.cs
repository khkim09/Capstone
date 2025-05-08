using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyShipDatabase", menuName = "Ship/Enemy Ship Database")]
public class EnemyShipDatabase : ScriptableObject
{
    public List<EnemyShipTemplate> allShips = new();

    private Dictionary<string, EnemyShipTemplate> shipDictionary;

    public void InitializeDictionary()
    {
        shipDictionary = new Dictionary<string, EnemyShipTemplate>();
        foreach (EnemyShipTemplate ship in allShips) shipDictionary[ship.templateName] = ship;
    }

    public EnemyShipTemplate GetShipTemplate(string templateName)
    {
        if (shipDictionary == null) InitializeDictionary();
        if (shipDictionary.TryGetValue(templateName, out EnemyShipTemplate ship)) return ship;
        return null;
    }
}
