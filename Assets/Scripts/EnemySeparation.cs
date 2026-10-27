using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)] // Apply crowd correction after each enemy's own movement.
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class EnemySeparation : MonoBehaviour
{
    [SerializeField, Min(.1f)] private float separationRadius = 1.1f;
    [SerializeField, Min(0f)] private float separationStrength = 3f;
    [SerializeField, Min(1)] private int maxNeighbours = 8;
    [Header("Obstacle steering")]
    [SerializeField, Min(0.1f)] private float obstacleLookAhead = 0.24f;
    [SerializeField, Min(0f)] private float obstacleClearance = 0.04f;
    private readonly Collider2D[] neighbours = new Collider2D[64];
    private readonly List<RaycastHit2D> movementHits = new(8);
    private Rigidbody2D body;
    private Collider2D ownCollider;
    private EnemyHitFeedback ownFeedback;
    private IoEnemy ownIo;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        ownCollider = GetComponent<Collider2D>();
        ownFeedback = GetComponent<EnemyHitFeedback>();
        ownIo = GetComponent<IoEnemy>();
        // Io and Trapper have their own kinematic movement and support rules.
        // They still need spacing, but must not gain the other enemies' helpers.
        if (ownIo == null && GetComponent<TrapperEnemy>() == null)
        {
            EnemyMovement movement = GetComponent<EnemyMovement>() ?? gameObject.AddComponent<EnemyMovement>();
            movement.Configure(obstacleLookAhead, obstacleClearance);
            if (GetComponent<EnemySupportable>() == null) gameObject.AddComponent<EnemySupportable>();
        }
    }

    private void FixedUpdate()
    {
        if (body == null || !body.simulated || ownFeedback == null || !ownFeedback.isActiveAndEnabled ||
            (ownIo != null && !ownIo.IsAlive)) return;
        int count = Physics2D.OverlapCircleNonAlloc(body.position,separationRadius,neighbours);
        Vector2 push = Vector2.zero;
        Vector2 strongest = Vector2.zero;
        float strongestPenetration = 0f;
        int used = 0;
        for (int i=0;i<count && used<maxNeighbours;i++)
        {
            Collider2D otherCollider=neighbours[i];
            if(otherCollider==null||otherCollider==ownCollider)continue;
            Rigidbody2D otherBody = otherCollider.attachedRigidbody;
            if (otherBody == null || otherBody == body) continue;
            EnemyHitFeedback otherFeedback = otherBody.GetComponent<EnemyHitFeedback>();
            if (otherFeedback == null || !otherFeedback.isActiveAndEnabled) continue;
            IoEnemy otherIo = otherBody.GetComponent<IoEnemy>();
            if (otherIo != null && !otherIo.IsAlive) continue;
            Vector2 delta=body.position-otherBody.position;
            float distance=delta.magnitude;
            if(distance<.001f)
            {
                // Each pair gets a stable, opposite direction even if both
                // spawn at precisely the same point.
                int firstId = Mathf.Min(body.GetInstanceID(), otherBody.GetInstanceID());
                uint pairHash = unchecked((uint)firstId * 2654435761u);
                float angle = (pairHash % 360u) * Mathf.Deg2Rad;
                Vector2 axis = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                delta = body.GetInstanceID() < otherBody.GetInstanceID() ? axis : -axis;
            }
            else delta/=distance;
            float penetration = separationRadius - distance;
            if (penetration <= 0f) continue;
            Vector2 correction = delta * penetration;
            push += correction;
            if (penetration > strongestPenetration)
            {
                strongestPenetration = penetration;
                strongest = correction;
            }
            used++;
        }
        if(used>0)
        {
            // Opposing neighbours can cancel each other out. In that case,
            // resolve at least the deepest overlap instead of staying stacked.
            if (push.sqrMagnitude < 0.0001f) push = strongest;
            float fixedStepScale = Time.fixedDeltaTime / 0.02f;
            Vector2 step = Vector2.ClampMagnitude(
                push * separationStrength * 0.12f * fixedStepScale,
                0.12f * fixedStepScale);
            if(step.sqrMagnitude>0.000001f)
            {
                if(body.bodyType==RigidbodyType2D.Kinematic)
                {
                    float distance=step.magnitude;
                    movementHits.Clear();
                    body.Cast(step/distance,movementHits,distance);
                    foreach(RaycastHit2D hit in movementHits)
                        if(hit.collider!=null&&!hit.collider.isTrigger&&
                           hit.collider.GetComponent<EnemySeparation>()==null)
                            distance=Mathf.Min(distance,Mathf.Max(0f,hit.distance-.01f));
                    body.MovePosition(body.position+step.normalized*distance);
                }
                else
                    body.linearVelocity+=step/Time.fixedDeltaTime;
            }
        }
    }
}
