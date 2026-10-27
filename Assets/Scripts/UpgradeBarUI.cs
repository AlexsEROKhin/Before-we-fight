using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// The six visible slots and their art are ordinary editable objects in Arena HUD Canvas.
public sealed class UpgradeBarUI : MonoBehaviour
{
    [Serializable]
    public sealed class UpgradeArt
    {
        public ChestUpgradeKind kind;
        public Sprite icon;
        public string title;
        [TextArea] public string description;
    }

    [Serializable]
    public sealed class Slot
    {
        public Image icon;
        public Text level;
    }

    [SerializeField] private Slot[] slots;
    [SerializeField] private UpgradeArt[] art;
    [SerializeField] private GameObject tooltip;
    [SerializeField] private Text tooltipText;
    [SerializeField] private Color emptyTint = new Color(0.29f, 0.23f, 0.28f, 0.8f);
    [SerializeField] private Color acquiredTint = Color.white;

    private readonly List<ChestUpgradeKind> acquiredOrder = new();
    private readonly Dictionary<ChestUpgradeKind, int> levels = new();

    public void Configure(Slot[] configuredSlots, UpgradeArt[] configuredArt,
        GameObject configuredTooltip, Text configuredTooltipText)
    {
        slots = configuredSlots;
        art = configuredArt;
        tooltip = configuredTooltip;
        tooltipText = configuredTooltipText;
    }

    private void Awake()
    {
        Refresh();
        HideTooltip();
    }

    public void RecordUpgrade(ChestUpgradeKind kind)
    {
        if (!levels.ContainsKey(kind))
        {
            levels[kind] = 0;
            acquiredOrder.Add(kind);
        }
        levels[kind]++;
        Refresh();
    }

    public void ShowTooltip(int slotIndex)
    {
        int firstVisible = Mathf.Max(0, acquiredOrder.Count - (slots == null ? 0 : slots.Length));
        int acquiredIndex = firstVisible + slotIndex;
        if (slotIndex < 0 || acquiredIndex >= acquiredOrder.Count || tooltip == null || tooltipText == null)
        {
            HideTooltip();
            return;
        }
        ChestUpgradeKind kind = acquiredOrder[acquiredIndex];
        UpgradeArt entry = FindArt(kind);
        string title = entry != null && !string.IsNullOrEmpty(entry.title) ? entry.title : kind.ToString();
        string description = entry != null ? entry.description : string.Empty;
        tooltipText.text = title + "  LV." + levels[kind] +
            (string.IsNullOrEmpty(description) ? string.Empty : "\n" + description);
        tooltip.SetActive(true);
    }

    public void HideTooltip()
    {
        if (tooltip != null) tooltip.SetActive(false);
    }

    public void ShowSkillTooltip(string title, string description)
    {
        if (tooltip == null || tooltipText == null) return;
        tooltipText.text = title + (string.IsNullOrWhiteSpace(description) ?
            string.Empty : "\n" + description);
        tooltip.SetActive(true);
    }

    private UpgradeArt FindArt(ChestUpgradeKind kind)
    {
        if (art == null) return null;
        foreach (UpgradeArt entry in art)
            if (entry != null && entry.kind == kind) return entry;
        return null;
    }

    private void Refresh()
    {
        if (slots == null) return;
        int firstVisible = Mathf.Max(0, acquiredOrder.Count - slots.Length);
        for (int i = 0; i < slots.Length; i++)
        {
            Slot slot = slots[i];
            if (slot == null || slot.icon == null) continue;
            int acquiredIndex = firstVisible + i;
            bool occupied = acquiredIndex < acquiredOrder.Count;
            UpgradeArt entry = occupied ? FindArt(acquiredOrder[acquiredIndex]) : null;
            slot.icon.sprite = entry != null ? entry.icon : null;
            slot.icon.color = occupied && entry != null && entry.icon != null ? acquiredTint : emptyTint;
            slot.icon.enabled = occupied && entry != null && entry.icon != null;
            if (slot.level != null)
            {
                int level = occupied ? levels[acquiredOrder[acquiredIndex]] : 0;
                slot.level.text = level > 1 ? level.ToString() : string.Empty;
                slot.level.gameObject.SetActive(level > 1);
            }
        }
    }
}
