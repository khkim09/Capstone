using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

public class WeaponPanel : MonoBehaviour
{
    /// <summary>
    /// weaponPanel이 펼쳐진 상태인지
    /// </summary>
    public bool isActive = false;

    /// <summary>
    /// 펼쳐진 상태의 weapon panel
    /// </summary>
    public GameObject WeaponPanelOn;
    /// <summary>
    /// 접힌 상태의 weapon panel
    /// </summary>
    public GameObject WeaponPanelOff;
    /// <summary>
    /// 패널 이동을 위한 애니메이터
    /// </summary>
    public Animator animator;
    /// <summary>
    /// 플레이어의 함선
    /// </summary>
    private Ship playerShip;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponentInParent<Animator>();
        WeaponPanelOff = transform.GetChild(0).gameObject;
        WeaponPanelOn=transform.GetChild(1).gameObject;

        playerShip=GameManager.Instance.playerShip;
        if (playerShip != null)
        {
            List<ShipWeapon> weapons = playerShip.GetAllWeapons();
            if (weapons.Count > 0)
            {
                foreach (ShipWeapon weapon in weapons)
                {
                    {
                        AddWeaponFrame(weapon);
                    }
                }
            }
        }
    }

    public void OpenPanelClicked()
    {
        if (isActive)   //클릭 했을 때 활성화되어있던 상황으로 패널을 접는 상황
        {
            WeaponPanelOff.SetActive(true);
            WeaponPanelOn.SetActive(false);
        }
        else
        {
            WeaponPanelOn.SetActive(true);
            WeaponPanelOff.SetActive(false);
        }
        isActive=!isActive;
        animator.SetBool("OnPanel", isActive);
    }

    public GameObject weaponFramePrefab;
    public Transform contentTransform;
    /// <summary>
    /// weapon panel에 무기를 추가
    /// </summary>
    /// <param name="weapon"></param>
    private void AddWeaponFrame(ShipWeapon weapon)
    {
        GameObject weaponFrame = Instantiate(weaponFramePrefab, contentTransform,false);
        WeaponFrameFunction weaponFrameFunction = weaponFrame.GetComponent<WeaponFrameFunction>();
        weaponFrameFunction.weapon = weapon;
    }

    //todo: WeaponFrame이랑 InfoPanel에서 액션리스너 제거해줘야됨

    public void AllFireButtonClicked()
    {

    }
}
