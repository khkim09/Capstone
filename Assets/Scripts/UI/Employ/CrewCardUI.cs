using TMPro;
using UnityEngine;

public class CrewCardUI : MonoBehaviour
{
    public TMP_Text crewNameText;

    public void SetCrewName(string name)
    {
        if (crewNameText != null)
            crewNameText.text = name;
    }
}
