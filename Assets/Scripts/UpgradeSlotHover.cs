using UnityEngine;
using UnityEngine.EventSystems;

public sealed class UpgradeSlotHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private UpgradeBarUI bar;
    [SerializeField] private int slotIndex;
    [SerializeField] private bool isActiveSkill;
    [SerializeField] private string skillTitle;
    [SerializeField, TextArea] private string skillDescription;

    public void Configure(UpgradeBarUI owner, int index)
    {
        bar = owner;
        slotIndex = index;
        isActiveSkill = false;
    }

    public void ConfigureSkill(UpgradeBarUI owner, string title, string description)
    {
        bar = owner;
        isActiveSkill = true;
        skillTitle = title;
        skillDescription = description;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isActiveSkill) bar?.ShowSkillTooltip(skillTitle, skillDescription);
        else bar?.ShowTooltip(slotIndex);
    }
    public void OnPointerExit(PointerEventData eventData) => bar?.HideTooltip();
}
