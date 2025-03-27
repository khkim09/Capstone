using System.Collections.Generic;
using UnityEngine;

public class CrewSystem : ShipSystem
{
    private List<CrewBase> crews = new();

    public override void Initialize(Ship ship)
    {
        base.Initialize(ship);

        if (GetCrewCount() == 0)
        {
            // AlertNeedCrew();
        }
    }

    public override void Update(float deltaTime)
    {
    }

    public bool AddCrewMember(CrewBase newCrew)
    {
        if (crews.Count >= GetShipStat(ShipStat.CrewCapacity))
            return false;


        crews.Add(newCrew);
        return true;
    }


    public bool RemoveCrewMember(CrewMember crewToRemove)
    {
        if (!crews.Contains(crewToRemove))
            return false;

        crews.Remove(crewToRemove);

        return true;
    }

    public int GetCrewCount()
    {
        return crews.Count;
    }

    public List<CrewBase> GetCrews()
    {
        return crews;
    }
}
