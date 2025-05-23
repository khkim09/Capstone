using UnityEngine;
using UnityEngine.UI;

public class PlanetUI : MonoBehaviour
{
    private Image portrait;

    private Image planetIllust;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        portrait = transform.Find("Portrait/PortraitImage").GetComponent<Image>();
        planetIllust = transform.Find("PlanetIllust").GetComponent<Image>();

        PlanetRace curPlanetRace = GameManager.Instance.WhereIAm().planetRace;
        switch (curPlanetRace)
        {
            case PlanetRace.Aquatic:
                portrait.sprite = Resources.Load<Sprite>("Sprites/UI/Planet/Portrait/SIS");
                planetIllust.sprite = Resources.Load<Sprite>("Sprites/UI/Planet/Illust/SIS");
                break;
            case PlanetRace.Avian:
                portrait.sprite = Resources.Load<Sprite>("Sprites/UI/Planet/Portrait/CCK");
                planetIllust.sprite = Resources.Load<Sprite>("Sprites/UI/Planet/Illust/CCK");
                break;
            case PlanetRace.IceMan:
                portrait.sprite = Resources.Load<Sprite>("Sprites/UI/Planet/Portrait/ICM");
                planetIllust.sprite = Resources.Load<Sprite>("Sprites/UI/Planet/Illust/ICM");
                break;
            case PlanetRace.Human:
                portrait.sprite = Resources.Load<Sprite>("Sprites/UI/Planet/Portrait/RCE");
                planetIllust.sprite = Resources.Load<Sprite>("Sprites/UI/Planet/Illust/RCE");
                break;
            case PlanetRace.Amorphous:
                portrait.sprite = Resources.Load<Sprite>("Sprites/UI/Planet/Portrait/KTL");
                planetIllust.sprite = Resources.Load<Sprite>("Sprites/UI/Planet/Illust/KTL");
                break;
        }
    }

    public void OnExitButtonClicked()
    {
        // 1. 행성 떠날 때 WorldNodeDataList clear
        GameManager.Instance.WorldNodeDataList.Clear();

        // 2. WarpNodeDataList clear
        GameManager.Instance.WarpNodeDataList.Clear();

        // 3. currentWorldNodePosition 현재 행성 위치로
        // currentWorldNodePosition = GameManager.Instance.PlanetDataList[GameManager.Instance.CurrentWarpTargetPlanetId].normalizedPosition;

        SceneChanger.Instance.LoadScene("Idle");
    }
}
