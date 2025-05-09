using System;
using UnityEngine;
using UnityEngine.UI;

public class CrewIconButton : MonoBehaviour
{
    /// <summary>
    /// 장비 착용됨을 알리는 작은 Apply UI
    /// </summary>
    public Image crewImage;
    private CrewMember myCrew;
    private Action<CrewMember> onSelected;

    public Image weaponIcon;
    public Image shieldIcon;
    public Image assistantIcon;

    public Sprite noneIcon;

    /// <summary>
    /// 개별 선원 Icon 초기화 작업 (선원 이미지 할당)
    /// </summary>
    /// <param name="crew"></param>
    /// <param name="eq"></param>
    /// <param name="onClick"></param>
    public void Initialize(CrewMember crew, Action<CrewMember> onClick)
    {
        myCrew = crew;
        onSelected = onClick;

        crewImage.sprite = crew.spriteRenderer.sprite;

        weaponIcon.sprite = crew.equippedWeapon.eqIcon != null ? crew.equippedWeapon.eqIcon : noneIcon;
        shieldIcon.sprite = crew.equippedShield.eqIcon != null ? crew.equippedShield.eqIcon : noneIcon;
        assistantIcon.sprite = crew.equippedAssistant.eqIcon != null ? crew.equippedAssistant.eqIcon : noneIcon;

        GetComponent<Button>().onClick.AddListener(() =>
        {
            onSelected?.Invoke(myCrew);
        });
    }

    public CrewMember GetCrew() => myCrew;
}
