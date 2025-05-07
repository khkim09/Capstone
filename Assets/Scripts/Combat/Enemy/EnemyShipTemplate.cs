using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a template for pirate ships that can be spawned as enemies.
/// References saved ship templates and controls crew generation parameters.
/// </summary>
[CreateAssetMenu(fileName = "New Enemy Ship Template", menuName = "Ship/Pirate Ship Template")]
public class EnemyShipTemplate : ScriptableObject
{
    [Header("Template Reference")] [Tooltip("Name of the saved ship template file (without extension)")]
    public string templateName = "pirate_default";

    [Tooltip("Path within Resources folder where ship templates are stored")]
    public string templatePath = "ShipTemplates";

    [Header("Crew Generation")] [Tooltip("Minimum number of crew members to generate")]
    public int minCrewCount = 2;

    [Tooltip("Maximum number of crew members to generate")]
    public int maxCrewCount = 5;

    [Tooltip("Possible crew races with relative weights")]
    public List<CrewRaceOption> possibleRaces = new();

    /// <summary>
    /// Get the full path to the template file
    /// </summary>
    public string GetTemplatePath()
    {
        return $"{templatePath}/template_{templateName}";
    }
}

/// <summary>
/// Defines a crew race option with weight for random selection
/// </summary>
[System.Serializable]
public class CrewRaceOption
{
    public CrewRace race;
    [Range(0f, 1f)] public float spawnWeight = 1.0f;
}
