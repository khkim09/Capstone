using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponFrameFunction : MonoBehaviour
{
    /// <summary>
    /// 연결된 ShipWeapon
    /// </summary>
    public ShipWeapon weapon;
    /// <summary>
    /// PowerButton에 표시되고 있는 현재 전원 상태
    /// </summary>
    private Power powerState;
    /// <summary>
    /// 충전량 게이지 이미지
    /// </summary>
    private Image energyBar;

    /// <summary>
    /// PowerButton\On
    /// </summary>
    private GameObject on;
    /// <summary>
    /// PowerButton\Off
    /// </summary>
    private GameObject off;
    /// <summary>
    /// PowerButton\Auto
    /// </summary>
    private GameObject auto;
    /// <summary>
    /// FireButton\Ready
    /// </summary>
    private GameObject ready;
    /// <summary>
    /// FireButton\Charging
    /// </summary>
    private GameObject charging;

    void Start()
    {
        if (weapon.IsEnabled())
            powerState = Power.on;
        else
            powerState = Power.off;
        weapon.updateWeaponPanel += UpdateWeaponPanel;

        GetComponentInChildren<TextMeshProUGUI>().text = weapon.GetWeaponName();

        energyBar = transform.Find("EnergyBar").GetComponent<Image>();
        on = transform.Find("Power").Find("On").gameObject;
        off = transform.Find("Power").Find("Off").gameObject;
        auto = transform.Find("Power").Find("Auto").gameObject;

        ready = transform.Find("FireButton").Find("Ready").gameObject;
        charging = transform.Find("FireButton").Find("Charging").gameObject;

        UpdateWeaponPanel();
    }

    /// <summary>
    /// weapon panel frame에 정보를 갱신하여 다시 그린다.
    /// 만약 전원 상태가 Auto라면 게이지가 꽉 찼을 때 자동발사한다.
    /// </summary>
    public void UpdateWeaponPanel()
    {
        SetPowerButton(powerState);
        SetFireButton();
        float percentage = weapon.GetCooldownPercentage();
        energyBar.fillAmount = percentage;

        if (powerState == Power.auto && percentage >= 1)
            weapon.TryFire();
    }

    /// <summary>
    /// 인자로 받은 Power로 powerState를 변경하고 상태에 맞는 버튼만 활성화시킴
    /// </summary>
    /// <param name="power"></param>
    private void SetPowerButton(Power power)
    {
        on.SetActive(false);
        off.SetActive(false);
        auto.SetActive(false);

        switch (power)
        {
            case Power.on:
                on.SetActive(true);
                break;
            case Power.off:
                off.SetActive(true);
                break;
            case Power.auto:
                auto.SetActive(true);
                break;
        }
    }
    /// <summary>
    /// FireButton을 현재 충전 상태에 따라 갱신한다.
    /// </summary>
    private void SetFireButton()
    {
        ready.SetActive(false);
        charging.SetActive(false);

        if (weapon.IsReady())
            ready.SetActive(true);
        else
            charging.SetActive(true);
    }

    public void PowerButtonClicked()
    {
        switch (powerState)
        {
            case Power.on:
                powerState = Power.auto;
                SetPowerButton(powerState);
                weapon.TryFire();
                break;
            case Power.off:
                powerState = Power.on;
                weapon.SetEnabled(true);
                break;
            case Power.auto:
                powerState = Power.off;
                weapon.SetEnabled(false);
                break;
        }
    }

    public void FireButtonClicked()
    {
        Debug.Log("발사");
        weapon.TryFire();
        //todo: 무기 실제로 연결해줘야됨
    }
}

enum Power
{
    on,
    off,
    auto
}
