using System.Collections;
using UnityEngine;

/// <summary>A separate orbiting bomb; after launch it only deals explosion damage.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
public sealed class IoBomb : MonoBehaviour
{
    [Header("Contact")]
    [SerializeField, Min(1)] private int contactDamage = 30;
    [SerializeField, Min(0.05f)] private float contactDamageCooldown = 0.8f;
    [Header("Explosion")]
    [SerializeField, Min(1)] private int explosionDamage = 60;
    [SerializeField, Min(0.1f)] private float explosionRadius = 1.5f;
    [SerializeField, Min(0.1f)] private float explosionDelay = 0.6f;
    [SerializeField, Min(0.1f)] private float travelSpeed = 5f;
    [SerializeField] private Color orbitColor = new(0.45f, 0.95f, 1f, 1f);
    [SerializeField] private Color warningColor = new(1f, 0.2f, 0.28f, 1f);

    private static Sprite placeholderSprite;
    private Rigidbody2D body;
    private CircleCollider2D hitbox;
    private SpriteRenderer visual;
    private LineRenderer warningRing;
    private Material ringMaterial;
    private TrailRenderer trail;
    private IoEnemy owner;
    private int orbitSlot;
    private Vector2 orbitPosition;
    private Vector2 lockedTarget;
    private float nextContactAt;
    private float warningEndsAt;
    private bool orbiting;
    private bool flying;
    private bool warning;
    private bool exploded;

    public bool IsOrbiting => orbiting && !exploded;
    public IoEnemy Owner => owner;
    public float ExplosionRadius => explosionRadius;
    public float EstimatedTimeToExplosion => explosionDelay + 1.2f;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.freezeRotation = true;
        hitbox = GetComponent<CircleCollider2D>();
        hitbox.isTrigger = true;
        hitbox.radius = 0.18f;
        visual = GetComponent<SpriteRenderer>();
        if (visual.sprite == null) visual.sprite = GetPlaceholderSprite();
        visual.color = orbitColor;
        visual.sortingOrder = 24;

        warningRing = gameObject.AddComponent<LineRenderer>();
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            ringMaterial = new Material(shader) { name = "Io Bomb Warning (runtime)" };
            warningRing.sharedMaterial = ringMaterial;
        }
        warningRing.useWorldSpace = false;
        warningRing.loop = true;
        warningRing.positionCount = 32;
        warningRing.startWidth = 0.045f;
        warningRing.endWidth = 0.045f;
        warningRing.startColor = warningColor;
        warningRing.endColor = warningColor;
        warningRing.sortingOrder = 23;
        for (int i = 0; i < 32; i++)
        {
            float angle = i * Mathf.PI * 2f / 32f;
            warningRing.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * explosionRadius);
        }
        warningRing.enabled = false;
        trail = gameObject.AddComponent<TrailRenderer>();
        trail.sharedMaterial = ringMaterial;
        trail.time = 0.16f;
        trail.startWidth = 0.12f;
        trail.endWidth = 0f;
        trail.startColor = orbitColor * new Color(1f, 1f, 1f, 0.55f);
        trail.endColor = new Color(orbitColor.r, orbitColor.g, orbitColor.b, 0f);
        trail.sortingOrder = 22;
    }

    private static Sprite GetPlaceholderSprite()
    {
        if (placeholderSprite != null) return placeholderSprite;
        // Temporary eight-pixel circle; no sprite file or supplied art is changed.
        Texture2D texture = new Texture2D(8, 8, TextureFormat.RGBA32, false)
        {
            name = "Io Bomb Placeholder (runtime)",
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                float dx = x - 3.5f, dy = y - 3.5f;
                texture.SetPixel(x, y, dx * dx + dy * dy <= 12f ? Color.white : Color.clear);
            }
        texture.Apply();
        placeholderSprite = Sprite.Create(texture, new Rect(0, 0, 8, 8),
            new Vector2(0.5f, 0.5f), 16f);
        placeholderSprite.name = "Io Bomb Placeholder";
        return placeholderSprite;
    }

    public void Bind(IoEnemy newOwner, int slot)
    {
        owner = newOwner;
        orbitSlot = slot;
        orbiting = true;
        flying = false;
        warning = false;
        exploded = false;
        warningRing.enabled = false;
        visual.color = orbitColor;
        if (trail != null) trail.Clear();
    }

    public void SetOrbitPosition(Vector2 position)
    {
        if (orbiting) orbitPosition = position;
    }

    public bool Launch(Vector2 target)
    {
        if (!orbiting || owner == null || !owner.IsAlive) return false;
        orbiting = false;
        flying = true;
        GameSfx.PlayWorld(SfxCue.IoLaunch, transform.position);
        lockedTarget = target;
        hitbox.enabled = false; // Never contact-damage during flight or warning.
        visual.color = warningColor;
        transform.SetParent(null, true);
        return true;
    }

    private void FixedUpdate()
    {
        if (exploded || body == null) return;
        if (orbiting)
        {
            body.MovePosition(orbitPosition);
        }
        else if (flying)
        {
            Vector2 next = Vector2.MoveTowards(body.position, lockedTarget,
                travelSpeed * Time.fixedDeltaTime);
            body.MovePosition(next);
            if ((next - lockedTarget).sqrMagnitude <= 0.01f)
            {
                flying = false;
                warning = true;
                warningEndsAt = Time.time + explosionDelay;
                warningRing.enabled = true;
            }
        }
    }

    private void Update()
    {
        if (!warning || exploded) return;
        float remaining = Mathf.Clamp01((warningEndsAt - Time.time) / explosionDelay);
        visual.color = Color.Lerp(Color.white, warningColor, remaining);
        warningRing.startWidth = warningRing.endWidth = 0.045f + (1f - remaining) * 0.035f;
        if (Time.time >= warningEndsAt) Explode();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!orbiting || exploded || Time.time < nextContactAt) return;
        TopDownPlayer player = other.GetComponent<TopDownPlayer>();
        if (player == null || !player.IsAlive) return;
        nextContactAt = Time.time + contactDamageCooldown;
        player.ReceiveDamage(contactDamage);
        StartCoroutine(ContactFlash());
    }

    private IEnumerator ContactFlash()
    {
        visual.color = Color.white;
        yield return new WaitForSeconds(0.09f);
        if (visual != null && orbiting) visual.color = orbitColor;
    }

    private void Explode()
    {
        if (exploded) return;
        exploded = true;
        GameSfx.PlayWorld(SfxCue.MageImpact, transform.position);
        warning = false;
        warningRing.enabled = false;
        EnemyCombatCoordinator.Release(this);
        if (owner != null && owner.IsAlive) owner.NotifyBombFinished(orbitSlot);
        // Only one damage application, even if the player has several colliders.
        TopDownPlayer player = FindFirstObjectByType<TopDownPlayer>();
        if (player != null && player.IsAlive)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            float extraRadius = playerCollider != null ? playerCollider.bounds.extents.magnitude : 0f;
            if (Vector2.Distance(body.position, player.transform.position) <= explosionRadius + extraRadius)
                player.ReceiveDamage(explosionDamage);
        }
        StartCoroutine(ImpactFlash());
    }

    private IEnumerator ImpactFlash()
    {
        if (trail != null) trail.emitting = false;
        warningRing.enabled = true;
        warningRing.startColor = warningRing.endColor = orbitColor;
        visual.color = Color.white;
        float elapsed = 0f;
        while (elapsed < 0.16f)
        {
            float t = elapsed / 0.16f;
            visual.transform.localScale = Vector3.one * Mathf.Lerp(1.4f, 0.3f, t);
            warningRing.startWidth = warningRing.endWidth = Mathf.Lerp(0.11f, 0f, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

    public void Cancel()
    {
        if (exploded) return;
        exploded = true;
        orbiting = flying = warning = false;
        if (hitbox != null) hitbox.enabled = false;
        if (warningRing != null) warningRing.enabled = false;
        EnemyCombatCoordinator.Release(this);
        enabled = false;
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        EnemyCombatCoordinator.Release(this);
        if (!exploded && owner != null && owner.IsAlive)
            owner.NotifyBombFinished(orbitSlot);
        if (ringMaterial != null) Destroy(ringMaterial);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = warningColor;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
