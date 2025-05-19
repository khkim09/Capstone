using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IdleHUDController : MonoBehaviour
{
    public TextMeshProUGUI infoText;
    public Image shipHealthBarFill;

    [SerializeField] private Button buttonEnding;

    private float CurrentHitpoints => GameManager.Instance.GetPlayerShip().GetCurrentHitPoints();
    private float MaxHitpoints => GameManager.Instance.GetPlayerShip().GetStat(ShipStat.HitPointsMax);
    private float CurrentCOMA => ResourceManager.Instance.COMA;
    private float CurrentFuel => ResourceManager.Instance.Fuel;
    private float CurrentOxygen => GameManager.Instance.GetPlayerShip().GetOxygenRate();
    private int CurrentMissle => ResourceManager.Instance.Missle;
    private int CurrentHypersonic => ResourceManager.Instance.Hypersonic;

    private void Awake()
    {
        GameManager.Instance.OnYearChanged += CheckShowEnding;
    }

    public void CheckShowEnding(int year)
    {
        if (year >= Constants.Endings.EndingYear) buttonEnding.gameObject.SetActive(true);
    }
}
