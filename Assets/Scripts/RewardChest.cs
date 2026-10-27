using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(200)] // Run after the arena's tree/character sorting.
[RequireComponent(typeof(SpriteRenderer))]
public sealed class RewardChest : MonoBehaviour
{
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openingSprite;
    [SerializeField] private Sprite openedSprite;
    [SerializeField, Min(0.2f)] private float interactionRadius = 1.45f;
    [SerializeField, Min(0.02f)] private float frameSeconds = 0.12f;
    [SerializeField] private ChestRewardUI rewardPanel;
    [SerializeField] private ChestRewardProfile rewardProfile;
    [SerializeField] private EnemyCampGate enemyCamp;
    [SerializeField] private Text interactionPrompt;
    [SerializeField] private string promptText = "E - OPEN CHEST";
    [SerializeField] private bool tintWhenOpened;
    [SerializeField] private Color openedTint = new Color(0.55f, 0.55f, 0.55f, 1f);

    private static RewardChest promptOwner;

    private TopDownPlayer player;
    private SpriteRenderer playerVisual;
    private SpriteRenderer visual;
    private Collider2D playerCollider;
    private Collider2D chestCollider;
    private bool opened;

    private void Awake()
    {
        visual = GetComponent<SpriteRenderer>();
        chestCollider = GetComponent<Collider2D>();
        visual.sprite = closedSprite;
        player = FindFirstObjectByType<TopDownPlayer>();
        if (interactionPrompt != null && promptOwner == null) interactionPrompt.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (player == null) player = FindFirstObjectByType<TopDownPlayer>();
        bool inRange = !opened && !ChestRewardUI.IsOpen && player != null && player.IsAlive &&
            Vector2.Distance(transform.position, player.transform.position) <= interactionRadius;
        bool unlocked = enemyCamp == null || enemyCamp.IsCleared;
        if (interactionPrompt != null)
        {
            if (inRange && (promptOwner == null || !promptOwner.IsInRange() ||
                Vector2.Distance(transform.position, player.transform.position) <
                Vector2.Distance(promptOwner.transform.position, player.transform.position)))
            {
                promptOwner = this;
                // The existing pixel frame already contains "E - OPEN CHEST".
                // Hide the overlaid UI text for chests; use text alone for the altar.
                bool useChestFrame = unlocked && promptText.Contains("CHEST");
                interactionPrompt.enabled = !useChestFrame;
                interactionPrompt.text = unlocked ? promptText :
                    "DEFEAT ENEMIES: " + enemyCamp.RemainingEnemies;
                if (interactionPrompt.transform.childCount > 0)
                    interactionPrompt.transform.GetChild(0).gameObject.SetActive(useChestFrame);
                interactionPrompt.gameObject.SetActive(true);
            }
            else if (!inRange && promptOwner == this)
            {
                promptOwner = null;
                interactionPrompt.gameObject.SetActive(false);
            }
        }
        if (inRange && unlocked && Input.GetKeyDown(KeyCode.E)) StartCoroutine(Open());
    }

    private bool IsInRange() => !opened && player != null && player.IsAlive &&
        Vector2.Distance(transform.position, player.transform.position) <= interactionRadius;

    private void LateUpdate()
    {
        if (player == null) return;
        if (playerVisual == null)
        {
            Transform visualRoot = player.transform.Find("PlayerVisual");
            if (visualRoot != null) playerVisual = visualRoot.GetComponent<SpriteRenderer>();
        }
        if (playerVisual == null || visual == null) return;

        // Compare the same ground-contact points used by TreeSideSorting. The
        // knight's root and painted head are not ground-contact points, so
        // comparing their Y positions made the chest cover only his head.
        if (playerCollider == null) playerCollider = player.GetComponent<Collider2D>();
        float playerFootY = playerCollider != null ? playerCollider.bounds.min.y : player.transform.position.y;
        float chestFootY = chestCollider != null ? chestCollider.bounds.min.y : transform.position.y;
        bool playerBehindChest = playerFootY > chestFootY;
        int order = playerVisual.sortingOrder + (playerBehindChest ? 1 : -1);
        if (visual.sortingOrder != order) visual.sortingOrder = order;
    }

    private IEnumerator Open()
    {
        opened = true;
        if (promptOwner == this) promptOwner = null;
        if (interactionPrompt != null) interactionPrompt.gameObject.SetActive(false);
        visual.sprite = openingSprite;
        yield return new WaitForSeconds(frameSeconds);
        visual.sprite = openedSprite;
        if (tintWhenOpened) visual.color = openedTint;
        yield return new WaitForSeconds(frameSeconds);
        if (rewardPanel != null) rewardPanel.Show(player, rewardProfile);
    }
}
