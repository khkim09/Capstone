using System.Collections.Generic;
using UnityEngine;

public class MoraleSystem : ShipSystem
{
    public override void Initialize(Ship ship)
    {
        base.Initialize(ship);
    }

    public override void Update(float deltaTime)
    {
    }

    public float CalculateGlobalMorale()
    {
        float resultMorale = 0, crewCapacity = 0;

        foreach (Room room in parentShip.GetAllRooms())
        {
            if (room is LifeSupportRoom lifeSupportRoom)
                resultMorale += lifeSupportRoom.GetStatContributions()[ShipStat.CrewMoraleBonus];

            if (room is CrewQuartersRoom crewQuartersRoom)
                crewCapacity += crewQuartersRoom.GetStatContributions()[ShipStat.CrewCapacity];
        }

        if (crewCapacity < parentShip.GetCrewCount())
            resultMorale -= parentShip.GetCrewCount() - crewCapacity;


        return resultMorale;
    }
}
