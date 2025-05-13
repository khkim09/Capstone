using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private CrewMember cm;

    public bool isIdle = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (cm == null)
        {
            cm = this.gameObject.GetComponent<CrewMember>();
        }
        if (isIdle)
        {
            isIdle = false;
            if (cm.IsMyShip())
            {
                cm.BackToThePeace();
            }
            else
            {
                RTSSelectionManager.Instance.MoveForCombat(cm, cm.currentRoom.occupiedCrewTiles);
                return;
            }
            int which = Random.Range(0, 2);
            switch (which)
            {
                case 0: // 무작위 방으로 이동
                    RTSSelectionManager.Instance.IssueMoveCommand(WhereToGo());
                    break;
                case 1: // 무작위 조종실로 이동
                    RTSSelectionManager.Instance.IssueMoveCommand(WhereToGo(RoomType.Cockpit));
                    break;
            }
        }
        else
        {
            if (!IsDoingSomething())
            {
                isIdle = true;
            }
        }
    }

    Room WhereToGo()
    {
        List<Room> rooms = cm.currentShip.GetAllRooms();
        List<Room> canGo = new List<Room>();
        foreach (Room room in rooms)
        {
            if (room.GetIsDamageable())
            {
                canGo.Add(room);
            }
        }

        List<CrewMember> currentShipAllCrew = cm.currentShip.GetAllCrew();
        foreach (CrewMember him in currentShipAllCrew)
        {
            if (him.isPlayerControlled != cm.isPlayerControlled)
            {
                canGo.Add(him.currentRoom);
            }
        }

        return canGo[Random.Range(0, canGo.Count)];
    }

    Room WhereToGo(RoomType type)
    {
        List<Room> rooms = cm.currentShip.GetAllRooms();
        List<Room> canGo = new List<Room>();
        foreach (Room room in rooms)
        {
            if (room.GetIsDamageable() && room.roomType == type)
            {
                canGo.Add(room);
            }
        }
        return canGo[Random.Range(0, canGo.Count)];
    }

    public bool IsDoingSomething()
    {
        if (cm.isMoving)
            return true;
        if (cm.inCombat)
            return true;
        return false;
    }
}
