using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomDamageEffectHandler : ISpecialEffectHandler
{
    public void HandleEffect(EventOutcome outcome)
    {
        if (outcome.specialEffectValue == "None")
        {
            ApplyRandomRoomDamage(outcome);
        }
        else
        {
            RoomType roomType = (RoomType)Enum.Parse(typeof(RoomType), outcome.specialEffectValue);

            List<Room> damageableRooms =
                GameManager.Instance.playerShip.allRooms.Where(r => r.roomType == roomType)
                    .Where(r => r.GetIsDamageable())
                    .Where(r => r.damageCondition != DamageLevel.breakdown).ToList();

            if (damageableRooms.Count == 0)
            {
                Debug.LogError("피해입을 수 있는 방이 없어서 랜덤으로 범위 확장!!");

                ApplyRandomRoomDamage(outcome);
            }

            int index = Random.Range(0, damageableRooms.Count);
            Room targetRoom = damageableRooms[index];
            ApplyRoomDamage(outcome, targetRoom);
        }
    }

    public void ApplyRandomRoomDamage(EventOutcome outcome)
    {
        List<Room> damageableRooms;

        if (outcome.specialEffectAmount == 0 || outcome.specialEffectAmount == 1)
            damageableRooms =
                GameManager.Instance.playerShip.allRooms.Where(r => r.GetIsDamageable())
                    .Where(r => r.damageCondition == DamageLevel.good).ToList();
        else
            damageableRooms =
                GameManager.Instance.playerShip.allRooms.Where(r => r.GetIsDamageable())
                    .Where(r => r.damageCondition != DamageLevel.breakdown).ToList();


        if (damageableRooms.Count == 0) Debug.LogError("랜덤으로 피해입을 수 있는 방이 없음!!");

        int index = Random.Range(0, damageableRooms.Count);
        Room targetRoom = damageableRooms[index];

        ApplyRoomDamage(outcome, targetRoom);
    }

    public void ApplyRoomDamage(EventOutcome outcome, Room targetRoom)
    {
        float currentRoomHealth = targetRoom.currentHitPoints;
        DamageLevel damageLevel = targetRoom.damageCondition;
        float targetRoomHealth = 0;
        float downRoomHealth = 0;

        if (outcome.specialEffectAmount == 0 || outcome.specialEffectAmount == 1)
        {
            if (damageLevel == DamageLevel.good)
                targetRoomHealth = targetRoom.GetRoomData().GetRoomDataByLevel(targetRoom.GetCurrentLevel())
                    .damageHitPointRate.damageLevelOne;
            else
                targetRoomHealth = targetRoom.GetRoomData().GetRoomDataByLevel(targetRoom.GetCurrentLevel())
                    .damageHitPointRate.damageLevelTwo;
            downRoomHealth = currentRoomHealth - targetRoomHealth;
        }
        else if (outcome.specialEffectAmount == 2)
        {
            targetRoomHealth = targetRoom.GetRoomData().GetRoomDataByLevel(targetRoom.GetCurrentLevel())
                .damageHitPointRate.damageLevelTwo;
            downRoomHealth = currentRoomHealth - targetRoomHealth;
        }


        targetRoom.TakeDamage(downRoomHealth + 0.1f);
    }
}
