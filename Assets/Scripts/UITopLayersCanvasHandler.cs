using TMPro;
using UnityEngine;

public class UITopLayersCanvasHandler : MonoBehaviour
{
    /// <summary>
    /// 보유 함선
    /// </summary>
    public Ship playerShip;

    /// <summary>
    /// 보유 선원 수
    /// </summary>
    private int currentCrewCount;

    /// <summary>
    /// 최대 보유 가능 선원 수
    /// </summary>
    private int maxCrewCount;

    /// <summary>
    /// 보유 선원 수 및 제한 표기 텍스트
    /// </summary>
    public TMP_Text crewReserve;


    /// <summary>
    /// 현재 보유 선원 수, 최대 보유 가능 선원 수 갱신해서 텍스트로 표기
    /// </summary>
    void Update()
    {
        currentCrewCount = playerShip.GetCrewCount();
        maxCrewCount = playerShip.GetMaxCrew();

        crewReserve.text = $"Crew : {currentCrewCount}/{maxCrewCount}";
    }
}
