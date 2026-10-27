using UnityEngine;

/// <summary>Short-lived, owner-bound warning ring for a locked ground strike.</summary>
public sealed class EnemyTelegraph : MonoBehaviour
{
    private MonoBehaviour owner;
    private LineRenderer ring;
    private Material material;
    private float endsAt;
    private float radius;

    public static EnemyTelegraph TryBegin(MonoBehaviour attacker, Vector2 position,
        float radius, float lifetime, Color color)
    {
        if (!EnemyCombatCoordinator.TryReserve(attacker, position, radius, lifetime)) return null;
        GameSfx.PlayWorld(SfxCue.EnemyStrike, attacker.transform.position);
        GameObject go = new GameObject(attacker.name + " Warning");
        go.transform.position = position;
        EnemyTelegraph result = go.AddComponent<EnemyTelegraph>();
        result.owner = attacker;
        result.endsAt = Time.time + lifetime;
        result.radius = radius;
        result.ring = go.AddComponent<LineRenderer>();
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            result.material = new Material(shader) { name = "Enemy Warning (runtime)" };
            result.ring.sharedMaterial = result.material;
        }
        result.ring.useWorldSpace = false;
        result.ring.loop = true;
        result.ring.positionCount = 20;
        result.ring.startWidth = result.ring.endWidth = 0.045f;
        result.ring.startColor = result.ring.endColor = color;
        result.ring.sortingOrder = 25;
        for (int i = 0; i < 20; i++)
        {
            float angle = i * Mathf.PI * 2f / 20f;
            result.ring.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
        }
        return result;
    }

    private void Update()
    {
        if (owner == null || !owner.isActiveAndEnabled || Time.time >= endsAt)
        {
            Destroy(gameObject);
            return;
        }
        ring.startWidth = ring.endWidth = 0.04f + Mathf.PingPong(Time.time * 0.12f, 0.025f);
    }

    public void Finish()
    {
        if (owner != null) EnemyCombatCoordinator.Release(owner);
        owner = null;
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (owner != null) EnemyCombatCoordinator.Release(owner);
        if (material != null) Destroy(material);
    }
}
