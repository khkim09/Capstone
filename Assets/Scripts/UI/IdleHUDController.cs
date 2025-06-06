﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IdleHUDController : MonoBehaviour
{
    [Header("함선 스탯 패널")][SerializeField] private TextMeshProUGUI shipStatusText;

    [Header("재화 패널")][SerializeField] private TextMeshProUGUI COMAText;
    [SerializeField] private TextMeshProUGUI fuelText;
    [SerializeField] private TextMeshProUGUI missileText;
    [SerializeField] private TextMeshProUGUI hypersonicText;


    [Header("함선 체력바")][SerializeField] private Image shipHealthBarFill;

    [SerializeField] private GameObject buttonEnding;

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
        GameManager.Instance.playerShip.InfoPanelChanged += UpdateHealthBar;
        //shipHealthBarFill=transform.Find("Ship Health Bar/Background/Fill").GetComponent<Image>();
    }

    private void Start()
    {
        if (buttonEnding)
            buttonEnding.GetComponent<Button>().onClick.AddListener(() => { OnClickEndingButton(); });
        CheckShowEnding(GameManager.Instance.CurrentYear);
    }

    private void Update()
    {
        SetShipStatusText();
        SetResourcesText();
    }

    public void SetShipStatusText()
    {
        /*
         *"선원 고용수 : {0}/{1}
            잔여 전력량 : {2}/{3}
            배리어 : {4}/{5}
            명중률 : {6}%
            재장전 속도 : {7}%
            피해량 : {8}%
            회피율 : {9}%
            연료 효율 : {1.}%
            산소량 : {11}%
        "
         */

        Ship playerShip = GameManager.Instance.GetPlayerShip();

        shipStatusText.text = $"ui.idle.shipstat".Localize(playerShip.allCrews.Count,
            playerShip.GetStat(ShipStat.CrewCapacity),
            playerShip.GetStat(ShipStat.PowerUsing), playerShip.GetStat(ShipStat.PowerCapacity),
            playerShip.ShieldSystem.GetCurrentShield(), playerShip.GetStat(ShipStat.ShieldMaxAmount),
            100 + playerShip.GetStat(ShipStat.Accuracy),
            100 + playerShip.GetStat(ShipStat.ReloadTimeBonus),
            100 + playerShip.GetStat(ShipStat.DamageBonus),
            playerShip.GetStat(ShipStat.DodgeChance),
            playerShip.GetStat(ShipStat.FuelEfficiency),
            playerShip.OxygenSystem.GetOxygenRate().ToString("F1"));
    }

    public void SetResourcesText()
    {
        COMAText.text = CurrentCOMA.ToString("N0");
        fuelText.text =
            $"{((int)CurrentFuel).ToString()}/{((int)GameManager.Instance.GetPlayerShip().GetStat(ShipStat.FuelStoreCapacity)).ToString()}";
        missileText.text = CurrentMissle.ToString();
        hypersonicText.text = CurrentHypersonic.ToString();
    }


    public void CheckShowEnding(int year)
    {
        if (year >= Constants.Endings.EndingYear)
        {
            if (buttonEnding)
                buttonEnding.SetActive(true);
        }
    }

    private void UpdateHealthBar()
    {
        float healthPercent = GameManager.Instance.playerShip.GetCurrentHitPoints() / MaxHitpoints;
        if (shipHealthBarFill)
            shipHealthBarFill.fillAmount = healthPercent;
        else
        {
            Debug.Log("함선 체력바 참조 실패");
        }
    }

    private void OnClickEndingButton()
    {
        GameManager.Instance.ShowEnding();
    }
}
