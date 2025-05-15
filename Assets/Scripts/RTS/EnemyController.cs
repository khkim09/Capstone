using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private CrewMember cm;

    public bool isIdle = true;

    void Start()
    {
        cm = this.GetComponent<CrewMember>();
    }

    void Update()
    {
        // 함내에 피해 발생으로 작업을 중단하고 수리를 먼저 할 필요가 있을 경우
        if (cm.IsOwnShip() && cm.currentRoom.NeedsRepair())
        {
            isIdle = true;
        }
        if (isIdle)
        {
            if (cm.IsOwnShip())
            {
                isIdle = false;
                cm.BackToThePeace();


                ///처음에 작업 진행할 해적 빼고 나머지는 다 텔포로 아군 함선으로 넘어올거라서
                /// 애초에 작업을 하는 애들은 작업 타일 위에 있어서 BackToThePeace()에서 작업상태로 전환될거야
                /// 그렇지 않으면 텔포방으로 이동하도록 찍으면 돼
                if (!IsDoingSomething())
                {
                    RTSSelectionManager.Instance.IssueMoveCommand();
                    /// 보니까 텔포방에 들어가면 바로 이동 시작하게 해놨더라고
                    /// IssueMoveCommand에서 텔포방으로 이동하도록 좌표찍어서 인자 넣어주면 될거야
                    /// EnemyController는 update에서 계속 도는 애라 어태치 하는 순간 바로 작동될거야
                    /// 아, isPlayerControlled는 끄고 어태치 해야 제대로 작동돼
                }

            }
            else
            {
                // 자신이 위치한 방 탐색
                if (cm.isWithEnemy())
                {
                    isIdle = false;
                    RTSSelectionManager.Instance.MoveForCombat(cm);
                }
                else
                {
                    if (cm.currentRoom.GetIsDamageable() && cm.currentRoom.currentHitPoints > 0)
                    {
                        isIdle = false;
                        cm.combatCoroutine = StartCoroutine(cm.CombatRoutine());
                    }
                }
            }

            if (isIdle)
            {
                isIdle = false;
                int which = Random.Range(0, 2);
                switch (which)
                {
                    case 0: // 무작위 방으로 이동
                        RTSSelectionManager.Instance.IssueMoveCommand(WhereToGo(), cm);
                        break;
                    case 1: // 무작위 조종실로 이동
                        RTSSelectionManager.Instance.IssueMoveCommand(WhereToGo(RoomType.Cockpit), cm);
                        break;
                }
            }
        }
        else
        {
            if (!cm.IsOwnShip())
            {
                if (!IsDoingSomething())
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
                if (him.currentRoom.GetRoomType() != RoomType.Corridor)
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
        if (cm.isWorking)
            return true;
        return false;
    }
}
