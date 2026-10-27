using System.Collections.Generic;
using System.Collections;
using UnityEngine;

/// <summary>Floating Io support enemy. Bombs and tether are separate components.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(IoTether))]
public sealed class IoEnemy : MonoBehaviour
{
    private static readonly HashSet<IoEnemy> activeIos = new();

    [Header("Health and flight")]
    [SerializeField, Min(1)] private int maximumHealth = 80;
    [SerializeField, Min(0.1f)] private float movementSpeed = 1.45f;
    [SerializeField, Min(0.1f)] private float preferredDistance = 4f;
    [SerializeField, Min(0.1f)] private float distanceBand = 1f;
    [SerializeField, Min(0.01f)] private float steeringSmoothTime = 0.28f;
    [SerializeField, Min(0.5f), Tooltip("Minimum centre-to-centre spacing between living Io enemies.")]
    private float minimumIoDistance = 2.4f;
    [SerializeField, Min(0f)] private float hoverAmplitude = 0.08f;
    [SerializeField, Min(0.1f)] private float hoverFrequency = 1.8f;
    [SerializeField, Min(0.1f)] private float activationRange = 8f;
    [Header("Idle sprites (existing art)")]
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField, Min(1f)] private float idleFps = 7f;
    [Header("Orbiting bombs")]
    [SerializeField] private IoBomb bombPrefab;
    [SerializeField, Range(1, 8)] private int bombCount = 3;
    [SerializeField, Min(0.1f)] private float orbitRadius = 1.5f;
    [SerializeField, Min(0f)] private float orbitSpeed = 120f;
    [SerializeField, Min(0.1f)] private float attackCooldown = 4f;
    [SerializeField, Min(0.1f)] private float bombRespawnTime = 5f;
    [SerializeField, Min(1)] private int swordDamage = 10;

    private Rigidbody2D body;
    private TopDownPlayer player;
    private IoTether tether;
    private IoBomb[] orbitBombs;
    private float[] respawnAt;
    private Vector3 visualRest;
    private Vector2 steeringVelocity;
    private Vector2 currentMovement;
    private float orbitAngle;
    private float nextAttackAt;
    private int health;
    private bool dying;
    private bool separatingFromIo;
    private readonly List<RaycastHit2D> movementHits = new(8);

    public bool IsAlive => isActiveAndEnabled && !dying && health > 0;
    public int MaximumHealth => maximumHealth;
    public int CurrentHealth => health;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.freezeRotation = true;
        GetComponent<CircleCollider2D>().isTrigger = true;
        player = FindFirstObjectByType<TopDownPlayer>();
        tether = GetComponent<IoTether>();
        if (visual == null) visual = GetComponentInChildren<SpriteRenderer>();
        if (visual != null) visualRest = visual.transform.localPosition;
        health = maximumHealth;
        orbitBombs = new IoBomb[bombCount];
        respawnAt = new float[bombCount];
        for (int i = 0; i < bombCount; i++) CreateBomb(i);
        nextAttackAt = Time.time + attackCooldown;
    }

    private void OnEnable() => activeIos.Add(this);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ClearRegistry() => activeIos.Clear();

    private void Update()
    {
        if (!IsAlive) return;
        if (visual != null)
        {
            if (idleFrames != null && idleFrames.Length > 0)
                visual.sprite = idleFrames[Mathf.FloorToInt(Time.time * idleFps) % idleFrames.Length];
            visual.transform.localPosition = visualRest + Vector3.up *
                (Mathf.Sin(Time.time * hoverFrequency * Mathf.PI * 2f) * hoverAmplitude);
        }
        if (player == null || !player.IsAlive) return;
        if (((Vector2)(player.transform.position - transform.position)).sqrMagnitude >
            activationRange * activationRange) return;
        if (Time.time >= nextAttackAt && TryLaunchBomb())
            nextAttackAt = Time.time + attackCooldown;
    }

    private void FixedUpdate()
    {
        if (!IsAlive || body == null) return;
        Vector2 desired = Vector2.zero;
        if (player != null && player.IsAlive)
        {
            Vector2 toPlayer = (Vector2)player.transform.position - body.position;
            float distance = toPlayer.magnitude;
            if (distance > 0.001f && distance <= activationRange)
            {
                if (distance < preferredDistance - distanceBand) desired = -toPlayer.normalized;
                else if (distance > preferredDistance + distanceBand) desired = toPlayer.normalized;
            }
        }

        EnemySupportable ally = tether != null ? tether.Target : null;
        if (ally != null && ally.IsAlive)
        {
            Vector2 toAlly = (Vector2)ally.transform.position - body.position;
            float keepRange = Mathf.Max(0.5f, tether.AllyDetectionRange * 0.65f);
            if (toAlly.magnitude > keepRange)
                desired += toAlly.normalized * (desired == Vector2.zero ? 1f : 0.55f);
        }

        // Io colliders are triggers, so physics alone cannot keep two healers apart.
        Vector2 spacing = Vector2.zero;
        Vector2 nearestAway = Vector2.zero;
        float nearestDistance = float.PositiveInfinity;
        float releaseDistance = minimumIoDistance + 0.35f;
        foreach (IoEnemy other in activeIos)
        {
            if (other == null || other == this || !other.IsAlive) continue;
            Vector2 away = body.position - (Vector2)other.transform.position;
            float distance = away.magnitude;
            if (distance >= releaseDistance) continue;
            if (distance < 0.001f)
            {
                float side = GetInstanceID() < other.GetInstanceID() ? -1f : 1f;
                away = new Vector2(side, 0.4f).normalized;
                distance = 0f;
            }
            else away /= distance;
            spacing += away * (1f - distance / releaseDistance);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestAway = away;
            }
        }
        separatingFromIo = nearestDistance < minimumIoDistance ||
            (separatingFromIo && nearestDistance < releaseDistance);
        if (separatingFromIo)
            desired = spacing.sqrMagnitude > 0.0001f ? spacing.normalized : nearestAway;

        Vector2 targetMovement = Vector2.ClampMagnitude(desired, 1f) * movementSpeed;
        currentMovement = Vector2.SmoothDamp(currentMovement, targetMovement,
            ref steeringVelocity, steeringSmoothTime, movementSpeed * 5f, Time.fixedDeltaTime);
        Vector2 movement = currentMovement * Time.fixedDeltaTime;
        float distanceToMove = movement.magnitude;
        if (distanceToMove > 0.0001f)
        {
            movementHits.Clear();
            body.Cast(movement / distanceToMove, movementHits, distanceToMove + 0.025f);
            foreach (RaycastHit2D hit in movementHits)
            {
                if (hit.collider == null || hit.collider.isTrigger) continue;
                movement *= Mathf.Clamp01((hit.distance - 0.025f) / distanceToMove);
                break;
            }
        }
        body.MovePosition(body.position + movement);

        orbitAngle += orbitSpeed * Time.fixedDeltaTime;
        for (int i = 0; i < orbitBombs.Length; i++)
        {
            if (orbitBombs[i] == null)
            {
                if (respawnAt[i] > 0f && Time.time >= respawnAt[i]) CreateBomb(i);
                continue;
            }
            float degrees = orbitAngle + 360f * i / bombCount;
            float angle = degrees * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * orbitRadius;
            orbitBombs[i].SetOrbitPosition(body.position + offset);
        }
    }

    private void CreateBomb(int slot)
    {
        if (bombPrefab == null) return;
        float angle = (orbitAngle + 360f * slot / bombCount) * Mathf.Deg2Rad;
        Vector2 position = (Vector2)transform.position +
            new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * orbitRadius;
        IoBomb bomb = Instantiate(bombPrefab, position, Quaternion.identity);
        bomb.name = "Io Orbit Bomb " + (slot + 1);
        bomb.Bind(this, slot);
        bomb.SetOrbitPosition(position);
        orbitBombs[slot] = bomb;
        respawnAt[slot] = 0f;
    }

    private bool TryLaunchBomb()
    {
        if (player == null || !player.IsAlive) return false;
        int chosen = -1;
        float nearest = float.PositiveInfinity;
        for (int i = 0; i < orbitBombs.Length; i++)
        {
            IoBomb bomb = orbitBombs[i];
            if (bomb == null || !bomb.IsOrbiting) continue;
            float distance = ((Vector2)(bomb.transform.position - player.transform.position)).sqrMagnitude;
            if (distance >= nearest) continue;
            nearest = distance;
            chosen = i;
        }
        if (chosen < 0) return false;
        IoBomb selected = orbitBombs[chosen];
        Vector2 target = player.transform.position;
        if (!EnemyCombatCoordinator.TryReserve(selected, target, selected.ExplosionRadius,
            selected.EstimatedTimeToExplosion)) return false;
        if (!selected.Launch(target))
        {
            EnemyCombatCoordinator.Release(selected);
            return false;
        }
        orbitBombs[chosen] = null;
        // Count respawn from the completed explosion, not from launch.
        respawnAt[chosen] = float.PositiveInfinity;
        return true;
    }

    public void NotifyBombFinished(int slot)
    {
        if (!IsAlive || slot < 0 || slot >= respawnAt.Length || orbitBombs[slot] != null) return;
        respawnAt[slot] = Time.time + bombRespawnTime;
    }

    public void TakeHit()
    {
        if (!IsAlive) return;
        health = Mathf.Max(0, health - swordDamage);
        if (health > 0) return;
        dying = true;
        if (tether != null) tether.BreakLink();
        CancelBombs();
        GetComponent<CircleCollider2D>().enabled = false;
        StartCoroutine(DeathFade());
    }

    private IEnumerator DeathFade()
    {
        if (visual != null)
        {
            Color start = visual.color;
            float elapsed = 0f;
            while (elapsed < 0.24f)
            {
                float t = elapsed / 0.24f;
                visual.color = new Color(Mathf.Lerp(start.r, 0.45f, t),
                    Mathf.Lerp(start.g, 1f, t), 1f, start.a * (1f - t));
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        EnemyDefeat.RewardAndDestroy(gameObject, 2);
    }

    private void CancelBombs()
    {
        if (orbitBombs != null)
            foreach (IoBomb bomb in orbitBombs)
                if (bomb != null) bomb.Cancel();
        foreach (IoBomb bomb in FindObjectsByType<IoBomb>(FindObjectsSortMode.None))
            if (bomb != null && bomb.transform != transform && bomb.Owner == this) bomb.Cancel();
    }

    private void OnDisable()
    {
        activeIos.Remove(this);
        separatingFromIo = false;
        if (visual != null) visual.transform.localPosition = visualRest;
        if (tether != null) tether.BreakLink();
        CancelBombs();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.45f, 1f, 0.75f, 0.45f);
        Gizmos.DrawWireSphere(transform.position, minimumIoDistance);
        Gizmos.color = new Color(0.3f, 0.85f, 1f, 0.55f);
        Gizmos.DrawWireSphere(transform.position, orbitRadius);
        Gizmos.color = new Color(1f, 0.35f, 0.45f, 0.45f);
        Gizmos.DrawWireSphere(transform.position, preferredDistance);
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, activationRange);
    }
}
