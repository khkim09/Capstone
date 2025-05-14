using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private CrewMember cm;

    public bool isIdle = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cm = this.GetComponent<CrewMember>();
    }

    // Update is called once per frame
    void Update()
    {
        //함내에 피해 발생으로 작업을 중단하고 수리를 먼저 할 필요가 있을 경우
        if (cm.IsMyShip() && cm.currentRoom.NeedsRepair())
        {
            isIdle = true;
        }
        if (isIdle)
        {
            if (cm.IsMyShip())
            {
                isIdle = false;
                cm.BackToThePeace();
            }
            else
            {
                //자신이 위치한 방 탐색
                if(cm.isWithEnemy())
                {
                    isIdle = false;
                    RTSSelectionManager.Instance.MoveForCombat(cm, cm.currentRoom.occupiedCrewTiles);
                }
                else
                {
                    if (cm.currentRoom.GetIsDamageable() && cm.currentRoom.currentHitPoints>0)
                    {
                        isIdle = false;
                        cm.combatCoroutine = StartCoroutine(cm.CombatRoutine());
                    }
                }
            }
            if(isIdle)
            {
                isIdle = false;
                int which = Random.Range(0, 2);
                switch (which)
                {
                    case 0: // 무작위 방으로 이동
                        RTSSelectionManager.Instance.IssueMoveCommand(WhereToGo(),cm);
                        break;
                    case 1: // 무작위 조종실로 이동
                        RTSSelectionManager.Instance.IssueMoveCommand(WhereToGo(RoomType.Cockpit),cm);
                        break;
                }
            }
        }
        else
        {
            if (!cm.IsMyShip())
            {
                if(!IsDoingSomething())
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
                if(him.currentRoom.GetRoomType()!=RoomType.Corridor)
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
