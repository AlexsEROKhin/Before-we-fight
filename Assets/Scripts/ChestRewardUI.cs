using UnityEngine;
using UnityEngine.UI;

public enum ChestUpgradeKind { Sword, Boots, Health, CriticalSword, SecondDash, KillExplosion, WideSwing, DashGuard, BloodSword, SteelRhythm, IronSkin, Momentum, SpinMastery }

// The panel, cards and buttons live in the Arena scene and remain editable there.
public sealed class ChestRewardUI : MonoBehaviour
{
    [SerializeField] private Button swordButton;
    [SerializeField] private Button bootsButton;
    [SerializeField] private Button healthButton;
    [SerializeField] private Text[] abilityLabels = new Text[3];

    private readonly ChestUpgradeKind[] activeChoices = new ChestUpgradeKind[3];
    private Sprite[] originalSprites;

    private TopDownPlayer player;
    private float previousTimeScale = 1f;
    public static bool IsOpen { get; private set; }

    private void Awake()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (originalSprites != null) return;
        Button[] buttons = { swordButton, bootsButton, healthButton };
        originalSprites = new Sprite[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            int slot = i;
            originalSprites[i] = buttons[i].GetComponent<Image>().sprite;
            buttons[i].onClick.AddListener(() => Select(slot));
        }
    }

    public void Show(TopDownPlayer recipient, ChestRewardProfile profile = null)
    {
        if (IsOpen || recipient == null || !recipient.IsAlive) return;
        EnsureInitialized();
        Button[] buttons = { swordButton, bootsButton, healthButton };
        ChestUpgradeKind[] defaults = { ChestUpgradeKind.Sword, ChestUpgradeKind.Boots, ChestUpgradeKind.Health };
        ChestRewardChoice[] offered = profile != null ? profile.PickChoices(buttons.Length) : null;
        for (int i = 0; i < buttons.Length; i++)
        {
            ChestRewardChoice choice = offered != null ? offered[i] : null;
            activeChoices[i] = choice != null ? choice.kind : defaults[i];
            Image image = buttons[i].GetComponent<Image>();
            image.sprite = choice != null && choice.cardSprite != null ? choice.cardSprite : originalSprites[i];
            if (abilityLabels != null && i < abilityLabels.Length && abilityLabels[i] != null)
            {
                Text label = abilityLabels[i];
                label.supportRichText = true;
                string title = choice != null ? choice.displayName : string.Empty;
                string description = choice != null ? choice.description : string.Empty;
                label.text = string.IsNullOrWhiteSpace(description) ? title :
                    title + "\n<size=17><color=#D6C5A5>" + description + "</color></size>";
                abilityLabels[i].gameObject.SetActive(choice != null && !string.IsNullOrEmpty(choice.displayName));
            }
        }
        player = recipient;
        previousTimeScale = Time.timeScale;
        IsOpen = true;
        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    private void Select(int slot)
    {
        if (!IsOpen || player == null) return;
        player.ApplyChestUpgrade(activeChoices[slot]);
        Close();
    }

    private void Close()
    {
        Time.timeScale = previousTimeScale;
        IsOpen = false;
        player = null;
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (!IsOpen) return;
        Time.timeScale = previousTimeScale;
        IsOpen = false;
    }
}
