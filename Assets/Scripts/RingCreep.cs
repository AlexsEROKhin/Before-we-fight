using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D))]
public sealed class RingCreep : MonoBehaviour, IEnemySupportTarget
{
    private const float MovementSkin = 0.04f;

    [Header("Movement and attack")]
    [SerializeField, Min(0.5f)] private float detectionRadius = 5f;
    [SerializeField, Min(0f)] private float moveSpeed = 2.2f;
    [SerializeField, Min(0.1f)] private float attackStartRadius = 0.78f;
    [SerializeField, Min(0.1f)] private float damageRadius = 0.85f;
    [SerializeField, Min(0f)] private float attackCooldown = 1.1f;
    [SerializeField, Min(1)] private int damage = 30;
    [SerializeField, Min(1)] private int hitsToKill = 2;
    [SerializeField, Min(0.05f)] private float windupSeconds = 0.22f;
    [Header("Crowd movement")]
    [SerializeField, Min(0.3f)] private float neighbourRadius = 0.9f;
    [SerializeField, Min(0f)] private float neighbourAvoidance = 1.25f;
    [Header("Editable frames")]
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private Sprite[] attackFrames;
    [SerializeField, Min(1f)] private float animationFps = 10f;

    private TopDownPlayer player;
    private Collider2D playerCollider;
    private CircleCollider2D ownCollider;
    private SpriteRenderer visual;
    private Rigidbody2D body;
    private EnemySupportable support;
    private EnemySquadStage stage;
    private int hitsRemaining;
    private float idleTime;
    private float nextAttackAt;
    private bool attacking;
    private readonly List<RaycastHit2D> movementHits = new(8);
    private readonly Collider2D[] neighbours = new Collider2D[16];

    private void Awake()
    {
        player = FindFirstObjectByType<TopDownPlayer>();
        if (player != null) playerCollider = player.GetComponent<Collider2D>();
        visual = GetComponent<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();
        support = GetComponent<EnemySupportable>();
        stage = GetComponentInParent<EnemySquadStage>();
        ownCollider = GetComponent<CircleCollider2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.useFullKinematicContacts = true;
        body.gravityScale = 0f;
        body.freezeRotation = true;
        ownCollider.isTrigger = false;
        hitsRemaining = hitsToKill;
    }

    private void Update()
    {
        if (attacking || hitsRemaining <= 0) return;

        ShowIdle();
        if (player == null || !player.IsAlive) return;
        Vector2 offset = (Vector2)player.transform.position - body.position;
        float alert = detectionRadius + (stage != null ? stage.DetectionBonus : 0f);
        if (offset.sqrMagnitude <= alert * alert &&
            Mathf.Abs(offset.x) > 0.05f)
            visual.flipX = offset.x > 0f;
        if (Time.time >= nextAttackAt && CanReachPlayer(attackStartRadius + 0.04f))
            StartCoroutine(Attack());
    }

    private void FixedUpdate()
    {
        if (attacking || hitsRemaining <= 0)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 offset = player != null && player.IsAlive
            ? (Vector2)player.transform.position - body.position : Vector2.zero;
        float distance = offset.magnitude;
        bool inAttackRange = player != null && player.IsAlive &&
                             CanReachPlayer(attackStartRadius + 0.04f);
        if (inAttackRange && Time.time >= nextAttackAt) return;
        bool chasing = player != null && player.IsAlive &&
                       distance > 0.001f &&
                       distance <= detectionRadius + (stage != null ? stage.DetectionBonus : 0f) &&
                       !inAttackRange;
        Vector2 avoidance = GetNeighbourAvoidance();
        if (!chasing && avoidance.sqrMagnitude < 0.0001f) return;

        Vector2 desired = avoidance * neighbourAvoidance;
        if (chasing)
        {
            Vector2 towardPlayer = offset / distance;
            Vector2 lateralAvoidance = avoidance - towardPlayer * Vector2.Dot(avoidance, towardPlayer);
            desired = towardPlayer + lateralAvoidance * neighbourAvoidance;
        }
        if (desired.sqrMagnitude < 0.0001f) return;
        Vector2 direction = desired.normalized;
        float step = moveSpeed * (stage != null ? stage.MovementScale : 1f) *
                     (support != null ? support.SpeedMultiplier : 1f) *
                     Time.fixedDeltaTime * (chasing ? 1f : 0.65f);
        if (chasing)
        {
            float towardPlayer = Vector2.Dot(direction, offset / distance);
            if (towardPlayer > 0f)
                step = Mathf.Min(step, Mathf.Max(0f, distance - attackStartRadius) / towardPlayer);
        }
        Vector2 movement = ClearMovement(direction, step, out RaycastHit2D obstacle);
        if (movement.sqrMagnitude < 0.000001f && obstacle.collider != null)
        {
            Vector2 tangent = new(-obstacle.normal.y, obstacle.normal.x);
            float towardPlayer = Vector2.Dot(tangent, offset);
            if (towardPlayer < -0.001f ||
                (Mathf.Abs(towardPlayer) <= 0.001f && (GetInstanceID() & 1) == 0))
                tangent = -tangent;
            movement = ClearMovement(tangent, step, out _);
        }
        if (movement.sqrMagnitude > 0f)
            body.MovePosition(body.position + movement);
    }

    private Vector2 GetNeighbourAvoidance()
    {
        int count = Physics2D.OverlapCircleNonAlloc(body.position, neighbourRadius, neighbours);
        Vector2 avoidance = Vector2.zero;
        for (int i = 0; i < count; i++)
        {
            Collider2D other = neighbours[i];
            if (other == null || other.attachedRigidbody == null ||
                other.attachedRigidbody == body ||
                other.GetComponent<EnemySeparation>() == null) continue;

            Vector2 away = body.position - other.attachedRigidbody.position;
            float gap = away.magnitude;
            if (gap >= neighbourRadius) continue;
            if (gap < 0.001f)
            {
                float side = GetInstanceID() < other.GetInstanceID() ? -1f : 1f;
                away = new Vector2(side, 0.4f).normalized;
            }
            else away /= gap;
            avoidance += away * (1f - gap / neighbourRadius);
        }
        return avoidance;
    }

    private Vector2 ClearMovement(Vector2 direction, float step, out RaycastHit2D obstacle)
    {
        obstacle = default;
        movementHits.Clear();
        body.Cast(direction, movementHits, step + MovementSkin);
        foreach (RaycastHit2D hit in movementHits)
        {
            if (hit.collider == null || hit.collider.isTrigger ||
                hit.collider.GetComponent<RingCreep>() != null) continue;
            obstacle = hit;
            step = Mathf.Min(step, Mathf.Max(0f, hit.distance - MovementSkin));
            break;
        }
        return direction * step;
    }

    private IEnumerator Attack()
    {
        attacking = true;
        body.linearVelocity = Vector2.zero;
        float frameSeconds = 1f / animationFps;
        // Telegraph the ring before the first damaging frame.
        if (attackFrames != null && attackFrames.Length > 0 && attackFrames[0] != null)
            visual.sprite = attackFrames[0];
        yield return new WaitForSeconds(windupSeconds * (stage != null ?
            Mathf.Clamp(stage.CooldownScale, 0.82f, 1.18f) : 1f));
        GameSfx.PlayWorld(SfxCue.RingAttack, transform.position);
        bool damaged = false;
        for (int i = 0; i < attackFrames.Length; i++)
        {
            if (attackFrames[i] != null) visual.sprite = attackFrames[i];
            if (!damaged && i >= attackFrames.Length / 2 && player != null && player.IsAlive &&
                CanReachPlayer(damageRadius))
            {
                damaged = true;
                player.ReceiveDamage(damage);
            }
            yield return new WaitForSeconds(frameSeconds);
        }
        nextAttackAt = Time.time + attackCooldown * (stage != null ? stage.CooldownScale : 1f);
        attacking = false;
    }

    private bool CanReachPlayer(float centreRadius)
    {
        if (player == null || !player.IsAlive) return false;
        if (((Vector2)player.transform.position - body.position).sqrMagnitude <=
            centreRadius * centreRadius) return true;
        if (playerCollider == null || ownCollider == null) return false;
        ColliderDistance2D gap = ownCollider.Distance(playerCollider);
        return gap.isValid && gap.distance <= 0.08f;
    }

    private void ShowIdle()
    {
        if (idleFrames == null || idleFrames.Length == 0) return;
        idleTime += Time.deltaTime;
        visual.sprite = idleFrames[Mathf.FloorToInt(idleTime * animationFps) % idleFrames.Length];
    }

    public void TakeHit()
    {
        if (hitsRemaining <= 0) return;
        hitsRemaining--;
        if (hitsRemaining == 0)
            EnemyDefeat.PlayDeath(this, visual, body, null, 0, 0);
    }

    public int CurrentHits => hitsRemaining;
    public int MaximumHits => hitsToKill;
    public void RestoreOneHit()
    {
        if (enabled && hitsRemaining > 0)
            hitsRemaining = Mathf.Min(hitsToKill, hitsRemaining + 1);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.7f, 0.3f, 0.9f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, damageRadius);
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.45f);
        Gizmos.DrawWireSphere(transform.position, neighbourRadius);
    }
}
