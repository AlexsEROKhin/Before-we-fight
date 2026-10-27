using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Fragile support creep: a stationary warning and a persistent resin stain.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
public sealed class TrapperEnemy : MonoBehaviour, IEnemySupportTarget
{
    [Header("Health and movement")]
    [SerializeField, Min(1)] private int hitsToKill = 2;
    [SerializeField, Min(0.1f)] private float movementSpeed = 1.2f;
    [SerializeField, Min(0.1f)] private float preferredDistance = 5f;
    [SerializeField, Min(0.1f)] private float distanceBand = 1f;
    [SerializeField, Min(0.1f)] private float detectionRange = 8f;
    [Header("Invisibility after a non-lethal hit")]
    [SerializeField, Min(0.1f)] private float invisibilitySeconds = 4f;
    [SerializeField, Min(0f)] private float invisibilityCooldown = 8f;
    [SerializeField, Range(0f, 1f)] private float invisibleAlpha = 0.12f;
    [SerializeField, Min(1f)] private float retreatSpeedMultiplier = 1.5f;
    [Header("Resin trap")]
    [SerializeField] private StickyPuddle puddlePrefab;
    [SerializeField] private SpriteRenderer groundSurface;
    [SerializeField, Min(0.1f)] private float abilityRange = 7f;
    [SerializeField, Min(0.1f)] private float warningSeconds = 0.7f;
    [SerializeField, Range(0.05f, 1f)] private float movementMultiplier = 0.75f;
    [SerializeField, Min(0.1f)] private float attackCooldown = 5f;
    [SerializeField, Min(0f)] private float initialAttackDelay = 1.5f;
    [SerializeField, Min(0f)] private float recoverySeconds = 0.3f;
    [Header("Editable 32x32 animation frames")]
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private Sprite[] walkFrames;
    [SerializeField] private Sprite[] castFrames;
    [SerializeField] private Sprite[] hitFrames;
    [SerializeField] private Sprite[] deathFrames;
    [SerializeField, Min(1f)] private float animationFps = 8f;

    private readonly List<RaycastHit2D> movementHits = new(8);
    private readonly Collider2D[] targetHits = new Collider2D[12];
    private Rigidbody2D body;
    private CircleCollider2D hitbox;
    private SpriteRenderer visual;
    private TopDownPlayer player;
    private EnemySupportable support;
    private StickyPuddle puddle;
    private Coroutine casting;
    private int hitsRemaining;
    private float nextAttackAt;
    private float recoveryUntil;
    private float animationTime;
    private float hitUntil;
    private float hitAnimationTime;
    private bool walking;
    private bool invisible;
    private float invisibleUntil;
    private float nextInvisibilityAt;

    public int CurrentHits => hitsRemaining;
    public int MaximumHits => hitsToKill;
    public bool IsCasting => casting != null;
    public bool IsInvisible => invisible;
    public StickyPuddle CurrentPuddle => puddle;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.freezeRotation = true;
        hitbox = GetComponent<CircleCollider2D>();
        hitbox.isTrigger = true;
        visual = GetComponent<SpriteRenderer>();
        player = FindFirstObjectByType<TopDownPlayer>();
        support = GetComponent<EnemySupportable>();
        hitsRemaining = hitsToKill;
        nextAttackAt = Time.time + initialAttackDelay;
        if (groundSurface == null)
            foreach (SpriteRenderer candidate in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
                if (candidate.name.StartsWith("Ground (tiled")) { groundSurface = candidate; break; }
    }

    private void Update()
    {
        if (hitsRemaining <= 0) return;
        if (invisible && Time.time >= invisibleUntil) Reveal();
        animationTime += Time.deltaTime;
        bool reacting = Time.time < hitUntil;
        Sprite[] frames = reacting ? hitFrames : casting != null ? castFrames : walking ? walkFrames : idleFrames;
        if (frames != null && frames.Length > 0)
        {
            float clipTime = reacting ? hitAnimationTime : animationTime;
            int frame = reacting
                ? Mathf.Min(frames.Length - 1, Mathf.FloorToInt(clipTime * animationFps))
                : Mathf.FloorToInt(clipTime * animationFps) % frames.Length;
            visual.sprite = frames[frame];
        }
        if (reacting) hitAnimationTime += Time.deltaTime;
        if (invisible || player == null || !player.IsAlive || casting != null ||
            Time.time < recoveryUntil) return;
        Vector2 offset = (Vector2)player.transform.position - body.position;
        if (offset.sqrMagnitude > abilityRange * abilityRange || Time.time < nextAttackAt ||
            puddlePrefab == null) return;
        Vector2 lockedTarget = player.transform.position;
        if (!ValidGround(lockedTarget)) { nextAttackAt = Time.time + 0.5f; return; }
        if (!EnemyCombatCoordinator.TryReserve(this, lockedTarget, 0.65f, warningSeconds))
        { nextAttackAt = Time.time + 0.5f; return; }
        casting = StartCoroutine(Cast(lockedTarget));
    }

    private void FixedUpdate()
    {
        walking = false;
        if (hitsRemaining <= 0 || casting != null || (!invisible && Time.time < recoveryUntil) ||
            player == null || !player.IsAlive) return;
        Vector2 toPlayer = (Vector2)player.transform.position - body.position;
        float distance = toPlayer.magnitude;
        if (distance < 0.01f || distance > detectionRange) return;
        Vector2 desired = Vector2.zero;
        if (invisible && distance < preferredDistance + distanceBand) desired = -toPlayer / distance;
        else if (invisible) return;
        else if (distance < preferredDistance - distanceBand) desired = -toPlayer / distance;
        else if (distance > preferredDistance + distanceBand) desired = toPlayer / distance;
        if (desired == Vector2.zero) return;
        visual.flipX = toPlayer.x < 0f;
        float speed = movementSpeed * (invisible ? retreatSpeedMultiplier :
            support != null ? support.SpeedMultiplier : 1f);
        Vector2 step = desired * speed * Time.fixedDeltaTime;
        float stepLength = step.magnitude;
        movementHits.Clear();
        body.Cast(desired, movementHits, stepLength + 0.025f);
        foreach (RaycastHit2D hit in movementHits)
            if (hit.collider != null && !hit.collider.isTrigger)
            { step *= Mathf.Clamp01((hit.distance - 0.025f) / stepLength); break; }
        if (step.sqrMagnitude < 0.000001f) return;
        body.MovePosition(body.position + step);
        walking = true;
    }

    private IEnumerator Cast(Vector2 lockedTarget)
    {
        visual.flipX = lockedTarget.x < transform.position.x;
        GameSfx.PlayWorld(SfxCue.TrapCast, transform.position);
        walking = false;
        animationTime = 0f;
        puddle = Instantiate(puddlePrefab, lockedTarget, Quaternion.identity);
        puddle.name = "Sticky resin - " + name;
        puddle.Begin(warningSeconds, movementMultiplier);
        yield return new WaitForSeconds(warningSeconds);
        EnemyCombatCoordinator.Release(this);
        nextAttackAt = Time.time + attackCooldown;
        recoveryUntil = Time.time + recoverySeconds;
        casting = null;
    }

    private bool ValidGround(Vector2 position)
    {
        if (groundSurface == null) return false;
        Bounds ground = groundSurface.bounds;
        const float margin = 0.65f;
        if (position.x < ground.min.x + margin || position.x > ground.max.x - margin ||
            position.y < ground.min.y + margin || position.y > ground.max.y - margin) return false;
        int count = Physics2D.OverlapCircleNonAlloc(position, 0.55f, targetHits);
        for (int i = 0; i < count; i++)
        {
            Collider2D candidate = targetHits[i];
            if (candidate == null) continue;
            if (candidate.TryGetComponent(out StickyPuddle existing) && existing.IsActivePuddle)
                return false; // Do not stack permanent stains on the same spot.
            if (!candidate.isTrigger && candidate.attachedRigidbody == null) return false;
        }
        return true;
    }

    public void TakeHit()
    {
        if (hitsRemaining <= 0 || invisible) return;
        hitsRemaining--;
        if (hitsRemaining > 0)
        {
            hitUntil = Time.time + 0.24f;
            hitAnimationTime = 0f;
            if (Time.time >= nextInvisibilityAt) EnterInvisibility();
            return;
        }
        if (casting != null) { StopCoroutine(casting); casting = null; }
        EnemyCombatCoordinator.Release(this);
        if (puddle != null) puddle.CancelWarning();
        EnemyDefeat.PlayDeath(this, visual, body, deathFrames, 0,
            deathFrames != null ? deathFrames.Length : 0);
    }

    public void RestoreOneHit()
    {
        if (enabled && hitsRemaining > 0)
            hitsRemaining = Mathf.Min(hitsToKill, hitsRemaining + 1);
    }

    private void EnterInvisibility()
    {
        if (casting != null) { StopCoroutine(casting); casting = null; }
        EnemyCombatCoordinator.Release(this);
        if (puddle != null) puddle.CancelWarning();
        invisible = true;
        invisibleUntil = Time.time + invisibilitySeconds;
        nextInvisibilityAt = Time.time + invisibilityCooldown;
        nextAttackAt = Mathf.Max(nextAttackAt, invisibleUntil + recoverySeconds);
        if (support != null) support.SuspendIoSupport();
        SetInvisibleAlpha();
    }

    private void Reveal()
    {
        invisible = false;
        Color color = visual.color;
        color.a = 1f;
        visual.color = color;
        recoveryUntil = Time.time + recoverySeconds;
    }

    private void LateUpdate()
    {
        // Hit feedback can restore the sprite color; keep stealth opaque only on reveal.
        if (invisible) SetInvisibleAlpha();
    }

    private void SetInvisibleAlpha()
    {
        Color color = visual.color;
        color.a = invisibleAlpha;
        visual.color = color;
    }

    private void OnDisable()
    {
        EnemyCombatCoordinator.Release(this);
        if (puddle != null) puddle.CancelWarning();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.7f, 0.2f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, abilityRange);
    }
}
