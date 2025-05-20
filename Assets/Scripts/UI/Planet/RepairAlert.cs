using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RepairAlert : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Ship playerShip;
    private List<Room> destoryedRooms;
    private int  destoryedRoomsCount;
    private Animator animator;

    public int repairCF = 10;
    private int repairCost;

    void Start()
    {
        playerShip = GameObject.Find("PlayerShip").GetComponent<Ship>();
        List<Room> rooms = playerShip.GetAllRooms();

        destoryedRooms = new List<Room>();
        repairCost = 0;
        foreach (Room room in rooms)
        {
            if(room.damageCondition==DamageLevel.breakdown)
            {
                destoryedRooms.Add(room);
                repairCost+=(int)(room.GetMaxHitPoints() - room.currentHitPoints) * repairCF;
            }
        }

        destoryedRoomsCount = destoryedRooms.Count;

        if(destoryedRoomsCount>0)
        {
            transform.Find("AlertText").GetComponent<TextMeshProUGUI>().text =
                $"Destoryed Rooms\nx {destoryedRooms.Count}";
            animator=gameObject.GetComponent<Animator>();
            animator.SetTrigger("In");

            transform.Find("RepairButton").GetChild(0).GetComponent<TextMeshProUGUI>().text
                = $"Repair: {repairCost} COMA";
        }
    }

    public void OnRepairButtonClicked()
    {
        if (ResourceManager.Instance.COMA >= repairCost)
        {
            ResourceManager.Instance.ChangeResource(ResourceType.COMA,-repairCost);
            animator.SetTrigger("Out");
            List<Room> rooms = playerShip.GetAllRooms();
            foreach (Room room in rooms)
            {
                room.Repair(room.GetMaxHitPoints());
            }
        }
    }


}
