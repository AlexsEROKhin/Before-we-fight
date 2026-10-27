using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
public sealed class SimpleEnemy : MonoBehaviour, IEnemySupportTarget
{
    [SerializeField, Min(0.1f)] private float detectionRadius = 3f;
    [SerializeField, Min(0f)] private float moveSpeed = 1.2f;
    [SerializeField, Min(0.1f)] private float attackDistance = 0.65f;
    [SerializeField, Min(0.1f)] private float attackInterval = 0.9f;
    [SerializeField, Min(0.1f)] private float attackWindup = 0.35f;
    [SerializeField, Min(1)] private int damage = 30;
    [SerializeField, Min(1)] private int hitsToKill = 1;

    private TopDownPlayer player;
    private float nextAttackAt;
    private int hitsRemaining;
    private Rigidbody2D body;
    private EnemyTelegraph warning;
    private float attackAt;

    private void Awake()
    {
        player = FindFirstObjectByType<TopDownPlayer>();
        hitsRemaining = hitsToKill;
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        GetComponent<CircleCollider2D>().isTrigger = true;
    }

    private void Update()
    {
        if (player == null || !player.IsAlive) return;

        Vector2 towardPlayer = (Vector2)player.transform.position - (Vector2)transform.position;
        float distance = towardPlayer.magnitude;
        if (distance > detectionRadius)
        {
            if (warning != null) warning.Finish();
            warning = null;
            return;
        }

        if (warning != null)
        {
            if (Time.time < attackAt) return;
            if (distance <= attackDistance) player.ReceiveDamage(damage);
            warning.Finish();
            warning = null;
            nextAttackAt = Time.time + attackInterval;
            return;
        }
        if (distance > attackDistance)
            body.MovePosition(Vector2.MoveTowards(body.position,
                player.transform.position, moveSpeed * Time.deltaTime));
        else if (Time.time >= nextAttackAt)
        {
            warning = EnemyTelegraph.TryBegin(this, body.position, attackDistance,
                attackWindup + 0.05f, new Color(1f, 0.3f, 0.25f, 0.85f));
            if (warning != null) attackAt = Time.time + attackWindup;
            else nextAttackAt = Time.time + 0.2f;
        }
    }

    public void TakeHit()
    {
        hitsRemaining--;
        if (hitsRemaining <= 0) EnemyDefeat.RewardAndDestroy(gameObject);
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
        if (warning != null) warning.Finish();
        warning = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.35f, 0.35f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
