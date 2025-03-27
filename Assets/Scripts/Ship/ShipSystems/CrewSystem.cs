using System.Collections.Generic;
using UnityEngine;

public class CrewSystem : ShipSystem
{
    private List<CrewMember> crews = new();

    public override void Update(float deltaTime)
    {
    }

    public bool AddCrewMember(CrewMember newCrew)
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

    public List<CrewMember> GetCrews()
    {
        return crews;
    }
}
