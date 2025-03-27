using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public class InnerShipCombatController : MonoBehaviour
{
    public static InnerShipCombatController Instance;

    [SerializeField] private Room[] playerShipRooms;
    [SerializeField] private Room[] enemyShipRooms;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // 적 선원 침투 처리
    public void TeleportEnemyToPlayerShip(CrewMember enemyCrew)
    {
        Room spawnRoom = GetRandomRoom(playerShipRooms);
        Vector2 spawnPos = spawnRoom.GetAvailablePosition();
        enemyCrew.MoveToRoom(spawnRoom, spawnPos);
    }

    // 아군 선원 적 함선 침투 처리
    public void TeleportPlayerToEnemyShip(CrewMember playerCrew)
    {
        StartCoroutine(TeleportAfterDelay(playerCrew, enemyShipRooms, 0.5f));
    }

    // 적 함선에 침투한 아군 재소환 처리 (딜레이 1초)
    public void RecallPlayerFromEnemyShip(CrewMember playerCrew)
    {
        StartCoroutine(TeleportAfterDelay(playerCrew, playerShipRooms, 1f));
    }

    private IEnumerator TeleportAfterDelay(CrewMember crew, Room[] targetRooms, float delay)
    {
        yield return new WaitForSeconds(delay);

        Room targetRoom = GetRandomRoom(targetRooms);
        Vector2 spawnPos = targetRoom.GetAvailablePosition();
        crew.MoveToRoom(targetRoom, spawnPos);
    }

    // 무작위 방 선택
    private Room GetRandomRoom(Room[] rooms)
    {
        int index = Random.Range(0, rooms.Length);
        return rooms[index];
    }

    // 타일 선택 시 행동 결정 (좌클릭으로 선택, 우클릭 이동/상호작용)
    public void HandleTileInteraction(CrewMember selectedCrew, Room targetRoom, Vector2 tilePosition)
    {
        if (!selectedCrew.isAlive)
            return;

        CrewMember enemyOnTile = targetRoom.GetEnemyOnTile(tilePosition, selectedCrew);
        CrewMember allyOnTile = targetRoom.GetAllyOnTile(tilePosition, selectedCrew);

        if (enemyOnTile != null)
        {
            selectedCrew.MoveToRoom(targetRoom, tilePosition);
            selectedCrew.Attack(enemyOnTile);
        }
        else if (allyOnTile != null)
        {
            selectedCrew.MoveToRoom(targetRoom, tilePosition);
            selectedCrew.Heal(10f); // 예시값
        }
        else
        {
            selectedCrew.MoveToRoom(targetRoom, tilePosition);
            if (targetRoom.HasEnemyInSameRoom(selectedCrew))
            {
                selectedCrew.Attack(targetRoom.GetClosestEnemy(selectedCrew));
            }
        }
    }

    // RTS 명령 우선: 수동 이동 시 전투 중단
    public void IssueMoveCommand(CrewMember crew, Room targetRoom, Vector2 targetPos)
    {
        crew.StopCombat();
        crew.MoveToRoom(targetRoom, targetPos);
    }
}
*/
