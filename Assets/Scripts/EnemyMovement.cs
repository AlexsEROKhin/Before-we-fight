using System.Collections.Generic;
using UnityEngine;

/// <summary>Steers velocity-driven enemies around solid world colliders.</summary>
public sealed class EnemyMovement : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float lookAheadSeconds = 0.24f;
    [SerializeField, Min(0f)] private float wallClearance = 0.04f;
    private Rigidbody2D body;
    private EnemySupportable support;
    private bool speedFromOwnScript;
    private readonly List<RaycastHit2D> hits = new(8);

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        support = GetComponent<EnemySupportable>();
        speedFromOwnScript = GetComponent<RingCreep>() != null || GetComponent<MageCreep>() != null;
    }

    public void Configure(float lookAhead, float clearance)
    {
        lookAheadSeconds = Mathf.Max(0.1f, lookAhead);
        wallClearance = Mathf.Max(0f, clearance);
    }

    private void LateUpdate()
    {
        if (support == null) support = GetComponent<EnemySupportable>();
        if (!speedFromOwnScript && support != null && body != null &&
            body.linearVelocity.sqrMagnitude > 0.0001f)
            body.linearVelocity *= support.SpeedMultiplier;
    }

    private void FixedUpdate()
    {
        if (body != null && body.bodyType == RigidbodyType2D.Dynamic &&
            body.linearVelocity.sqrMagnitude > 0.0001f)
            Steer(body.linearVelocity);
    }

    public void Steer(Vector2 desiredVelocity)
    {
        if (body == null) return;
        float speed = desiredVelocity.magnitude;
        if (speed < 0.001f) { body.linearVelocity = Vector2.zero; return; }
        Vector2 direction = desiredVelocity / speed;
        float probe = speed * lookAheadSeconds + wallClearance;
        if (Clear(direction, probe)) { body.linearVelocity = desiredVelocity; return; }
        Vector2 side = new(-direction.y, direction.x);
        float left = Clearance(side, probe);
        float right = Clearance(-side, probe);
        Vector2 tangent = left >= right ? side : -side;
        body.linearVelocity = (direction * 0.3f + tangent * 0.85f).normalized * speed;
    }

    private bool Clear(Vector2 direction, float distance) => Clearance(direction, distance) >= distance;

    private float Clearance(Vector2 direction, float distance)
    {
        hits.Clear();
        body.Cast(direction, hits, distance);
        float nearest = distance;
        foreach (RaycastHit2D hit in hits)
            if (hit.collider != null && !hit.collider.isTrigger && hit.distance < nearest)
                nearest = hit.distance;
        return nearest;
    }
}
