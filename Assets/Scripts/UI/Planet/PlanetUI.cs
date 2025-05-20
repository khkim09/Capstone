using UnityEngine;
using UnityEngine.UI;

public class PlanetUI : MonoBehaviour
{
    private Image portrait;

    private Image planetIllust;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        portrait=transform.Find("Portrait/PortraitImage").GetComponent<Image>();
        planetIllust=transform.Find("PlanetIllust").GetComponent<Image>();

        PlanetRace curPlanetRace = GameManager.Instance.WhereIAm().planetRace;
        switch (curPlanetRace)
        {
            case PlanetRace.Aquatic:
                portrait=Resources.Load<Image>("Sprites/UI/Planet/Portrait/SIS");
                planetIllust=Resources.Load<Image>("Sprites/UI/Planet/Illust/SIS");
                break;
            case PlanetRace.Avian:
                portrait=Resources.Load<Image>("Sprites/UI/Planet/Portrait/CCK");
                planetIllust=Resources.Load<Image>("Sprites/UI/Planet/Illust/CCK");
                break;
            case PlanetRace.IceMan:
                portrait=Resources.Load<Image>("Sprites/UI/Planet/Portrait/ICM");
                planetIllust=Resources.Load<Image>("Sprites/UI/Planet/Illust/ICM");
                break;
            case PlanetRace.Human:
                portrait=Resources.Load<Image>("Sprites/UI/Planet/Portrait/RCE");
                planetIllust=Resources.Load<Image>("Sprites/UI/Planet/Illust/RCE");
                break;
            case PlanetRace.Amorphous:
                portrait=Resources.Load<Image>("Sprites/UI/Planet/Portrait/KTL");
                planetIllust=Resources.Load<Image>("Sprites/UI/Planet/Illust/KTL");
                break;
        }
    }

    public void OnExitButtonClicked()
    {
        // SceneChanger.Instance.LoadScene("Idle");
    }
}
