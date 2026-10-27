using System.Collections.Generic;
using UnityEngine;

/// <summary>Small, allocation-free reservation board for dangerous ground attacks.</summary>
public static class EnemyCombatCoordinator
{
    private struct Reservation
    {
        public int owner;
        public Vector2 position;
        public float radius;
        public float expires;
    }

    private static readonly List<Reservation> reservations = new(32);
    private static readonly RaycastHit2D[] sightHits = new RaycastHit2D[16];

    public static bool HasClearShot(Vector2 start, Vector2 end)
    {
        int count = Physics2D.LinecastNonAlloc(start, end, sightHits);
        for (int i = 0; i < count; i++)
        {
            Collider2D hit = sightHits[i].collider;
            if (hit == null || hit.isTrigger || hit.GetComponent<TopDownPlayer>() != null ||
                hit.GetComponent<EnemySeparation>() != null) continue;
            return false;
        }
        return true;
    }

    public static bool TryReserve(Object attacker, Vector2 position, float radius, float seconds)
    {
        float now = Time.time;
        for (int i = reservations.Count - 1; i >= 0; i--)
        {
            Reservation old = reservations[i];
            if (old.expires <= now) { reservations.RemoveAt(i); continue; }
            if (old.owner != attacker.GetInstanceID() &&
                (old.position - position).sqrMagnitude <
                (old.radius + radius) * (old.radius + radius)) return false;
        }
        reservations.Add(new Reservation
        {
            owner = attacker.GetInstanceID(), position = position,
            radius = radius, expires = now + Mathf.Max(0.1f, seconds)
        });
        return true;
    }

    public static void Release(Object attacker)
    {
        int id = attacker.GetInstanceID();
        for (int i = reservations.Count - 1; i >= 0; i--)
            if (reservations[i].owner == id) reservations.RemoveAt(i);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Clear() => reservations.Clear();
}
