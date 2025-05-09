using UnityEngine;
using UnityEngine.UI;
using System;

public class EquipmentIconButton : MonoBehaviour
{
    public Image iconImage;
    private EquipmentItem eqItem;
    private Action<EquipmentItem> onClick;

    public void Initialize(EquipmentItem item, Action<EquipmentItem> onClickAction)
    {
        eqItem = item;
        onClick = onClickAction;

        iconImage.sprite = item.eqIcon;

        GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke(eqItem));
    }
}
