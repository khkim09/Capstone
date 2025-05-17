using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class WorldNode : MonoBehaviour
{
    [SerializeField] private Image worldNodeImage;

    public WorldNodeData WorldNodeData;

    public bool isVisited = false;
}
