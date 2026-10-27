using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D))]
public sealed class MageCreep : MonoBehaviour, IEnemySupportTarget
{
    [Header("Editable distances")]
    [SerializeField, Min(0.5f)] private float detectionRadius = 6f;
    [SerializeField, Min(0.5f)] private float retreatDistance = 2.6f;
    [SerializeField, Min(0f)] private float retreatSpeed = 1.35f;
    [SerializeField] private bool blocksPlayer;
    [Header("Sky spell")]
    [SerializeField, Min(0.2f)] private float warningSeconds = 2f;
    [SerializeField, Min(0f)] private float cooldownSeconds = 2.2f;
    [SerializeField, Min(0.1f)] private float impactRadius = 0.65f;
    [SerializeField, Min(0.05f)] private float impactAnimationSeconds = 0.18f;
    [SerializeField, Range(0f, 1f)] private float impactDamageMoment;
    [SerializeField] private float spellVisualOffsetY = 0.82f;
    [SerializeField] private int warningSortingOrder = -2;
    [SerializeField] private int strikeSortingOrder = 40;
    [SerializeField, Min(1)] private int damage = 35;
    [SerializeField, Min(1)] private int hitsToKill = 1;
    [Header("Squad tactics")]
    [SerializeField, Min(0f)] private float sharedStrikeSpacing = 0.55f;
    [SerializeField, Min(0f)] private float blockedCastRetry = 0.25f;
    [Header("Editable sprite frames")]
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private Sprite[] moveFrames;
    [SerializeField] private Sprite[] castStartFrames;
    [SerializeField] private Sprite[] castFrames;
    [SerializeField] private Sprite[] castReleaseFrames;
    [SerializeField] private Sprite[] recoveryFrames;
    [SerializeField] private Sprite[] hurtFrames;
    [SerializeField] private Sprite[] staggerFrames;
    [SerializeField] private Sprite[] deathFrames;
    [SerializeField] private Sprite[] markerFrames;
    [SerializeField] private Sprite[] beamFrames;
    [SerializeField, Min(1f)] private float animationFps = 8f;

    private TopDownPlayer player;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D body;
    private EnemySupportable support;
    private EnemyMovement movement;
    private EnemySquadStage stage;
    private float nextCastAt;
    private float animationTime;
    private bool casting;
    private int hitsRemaining;
    private Sprite[] actionFrames;
    private SpriteRenderer activeMarker;
    private SpriteRenderer activeBeam;

    private void Awake()
    {
        player = FindFirstObjectByType<TopDownPlayer>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();
        support = GetComponent<EnemySupportable>();
        movement = GetComponent<EnemyMovement>() ?? gameObject.AddComponent<EnemyMovement>();
        stage = GetComponentInParent<EnemySquadStage>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        GetComponent<CircleCollider2D>().isTrigger = !blocksPlayer;
        hitsRemaining = hitsToKill;
    }

    private void Update()
    {
        if (player == null || !player.IsAlive) { body.linearVelocity = Vector2.zero; ShowLoop(idleFrames); return; }
        Vector2 offset = (Vector2)player.transform.position - body.position;
        float distance = offset.magnitude;

        if (casting) { body.linearVelocity = Vector2.zero; ShowLoop(actionFrames ?? castFrames); return; }
        ShowLoop(idleFrames);
        if (distance > detectionRadius + (stage != null ? stage.DetectionBonus : 0f))
        { body.linearVelocity = Vector2.zero; return; }

        spriteRenderer.flipX = offset.x < 0f;
        if (distance < retreatDistance)
        {
            movement.Steer(-offset.normalized * retreatSpeed *
                           (stage != null ? stage.MovementScale : 1f) *
                           (support != null ? support.SpeedMultiplier : 1f));
            ShowLoop(moveFrames);
            return;
        }

        body.linearVelocity = Vector2.zero;
        if (Time.time >= nextCastAt)
        {
            Vector2 target = player.transform.position;
            if (EnemyCombatCoordinator.TryReserve(this, target,
                impactRadius + sharedStrikeSpacing, warningSeconds + impactAnimationSeconds))
                StartCoroutine(CastAtPlayer(target));
            else nextCastAt = Time.time + blockedCastRetry;
        }
    }

    private IEnumerator CastAtPlayer(Vector2 lockedPosition)
    {
        casting = true;
        GameSfx.PlayWorld(SfxCue.MageWarning, transform.position);
        actionFrames = castStartFrames;
        body.linearVelocity = Vector2.zero;
        SpriteRenderer marker = activeMarker = CreateEffect("Mage Warning Marker", lockedPosition, warningSortingOrder);

        float effectiveWarning = warningSeconds * (stage != null ?
            Mathf.Lerp(1f, 0.88f, (1f - stage.CooldownScale) / 0.18f) : 1f);
        float elapsed = 0f;
        while (elapsed < effectiveWarning)
        {
            if (elapsed >= effectiveWarning * 0.25f) actionFrames = castFrames;
            if (marker != null && markerFrames.Length > 0)
                marker.sprite = markerFrames[Mathf.Min(markerFrames.Length - 1,
                    Mathf.FloorToInt(elapsed / effectiveWarning * markerFrames.Length))];
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (marker != null) Destroy(marker.gameObject);
        activeMarker = null;
        actionFrames = castReleaseFrames;
        GameSfx.PlayWorld(SfxCue.MageImpact, lockedPosition);
        SpriteRenderer beam = activeBeam = CreateEffect("Mage Sky Spell", lockedPosition + Vector2.up * spellVisualOffsetY, strikeSortingOrder);
        float impactTime = impactAnimationSeconds;
        elapsed = 0f;
        bool dealtDamage = false;
        while (elapsed < impactTime)
        {
            if (beam != null && beamFrames.Length > 0)
                beam.sprite = beamFrames[Mathf.Min(beamFrames.Length - 1,
                    Mathf.FloorToInt(elapsed / impactTime * beamFrames.Length))];
            if (!dealtDamage && elapsed >= impactTime * impactDamageMoment)
            {
                dealtDamage = true;
                if (player != null && player.IsAlive &&
                    Vector2.Distance(player.transform.position, lockedPosition) <= impactRadius)
                    player.ReceiveDamage(damage);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (!dealtDamage && player != null && player.IsAlive &&
            Vector2.Distance(player.transform.position, lockedPosition) <= impactRadius)
            player.ReceiveDamage(damage);
        if (beam != null) Destroy(beam.gameObject);
        activeBeam = null;
        EnemyCombatCoordinator.Release(this);
        actionFrames = recoveryFrames;
        yield return new WaitForSeconds(0.8f);
        nextCastAt = Time.time + cooldownSeconds * (stage != null ? stage.CooldownScale : 1f);
        casting = false;
        actionFrames = null;
    }

    private SpriteRenderer CreateEffect(string objectName, Vector2 position, int sortingOrder)
    {
        GameObject effect = new GameObject(objectName);
        effect.transform.position = position;
        SpriteRenderer renderer = effect.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    private void ShowLoop(Sprite[] frames)
    {
        if (frames == null || frames.Length == 0) return;
        animationTime += Time.deltaTime;
        spriteRenderer.sprite = frames[Mathf.FloorToInt(animationTime * animationFps) % frames.Length];
    }

    public void TakeHit()
    {
        hitsRemaining--;
        if (hitsRemaining <= 0) EnemyDefeat.PlayDeath(this, spriteRenderer, body, deathFrames, 0, deathFrames.Length);
    }

    public int CurrentHits => hitsRemaining;
    public int MaximumHits => hitsToKill;
    public void RestoreOneHit()
    {
        if (enabled && hitsRemaining > 0)
            hitsRemaining = Mathf.Min(hitsToKill, hitsRemaining + 1);
    }

    private void OnDisable()
    {
        EnemyCombatCoordinator.Release(this);
        if (activeMarker != null) Destroy(activeMarker.gameObject);
        if (activeBeam != null) Destroy(activeBeam.gameObject);
        activeMarker = null;
        activeBeam = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.75f, 0.15f, 0.25f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = new Color(1f, 0.7f, 0.2f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, retreatDistance);
    }
}
