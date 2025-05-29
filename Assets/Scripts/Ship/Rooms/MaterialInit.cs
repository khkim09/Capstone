using System.Collections;
using UnityEngine;

public class MaterialInit : MonoBehaviour
{

    public Material material;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Room romm in GameManager.Instance.playerShip.GetAllRooms())
        {
            romm.GetComponent<SpriteRenderer>().material = material;
        }
    }

    public IEnumerator materialInit()
    {
        yield return null;
        yield return null;
        yield return null;


        foreach (Room romm in GameManager.Instance.playerShip.GetAllRooms())
        {
            romm.GetComponent<SpriteRenderer>().material = material;
        }
    }
}
