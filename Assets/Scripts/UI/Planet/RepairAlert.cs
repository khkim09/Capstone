using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RepairAlert : MonoBehaviour
{
    private Ship playerShip;
    private List<Room> destroyedRooms;
    private int destroyedRoomsCount;
    private Animator animator;

    public int repairCF = 10;
    private int repairCost;

    void Start()
    {
        playerShip = GameManager.Instance.playerShip;
        List<Room> rooms = playerShip.GetAllRooms();

        destroyedRooms = new List<Room>();
        repairCost = 0;
        foreach (Room room in rooms)
        {
            if (room.currentHitPoints<room.GetMaxHitPoints())
            {
                destroyedRooms.Add(room);
                repairCost += (int)(room.GetMaxHitPoints() - room.currentHitPoints) * repairCF;
            }
        }

        destroyedRoomsCount = destroyedRooms.Count;

        if (destroyedRoomsCount > 0)
        {
            transform.Find("AlertText").GetComponent<TextMeshProUGUI>().text =
                $"{"ui.planet.alert.destroyed".Localize()}\nx {destroyedRooms.Count}";
            animator = gameObject.GetComponent<Animator>();
            animator.SetTrigger("In");

            transform.Find("RepairButton").GetChild(0).GetComponent<TextMeshProUGUI>().text
                = $"{"ui.planet.alert.repair".Localize()}: {repairCost} COMA";
        }
    }

    public void OnRepairButtonClicked()
    {
        if (ResourceManager.Instance.COMA >= repairCost)
        {
            ResourceManager.Instance.ChangeResource(ResourceType.COMA, -repairCost);
            animator.SetTrigger("Out");
            List<Room> rooms = playerShip.GetAllRooms();
            foreach (Room room in rooms)
                room.FullRepair(room.GetMaxHitPoints());

            GameManager.Instance.playerShip.RecalculateAllStats();
        }
    }
}
