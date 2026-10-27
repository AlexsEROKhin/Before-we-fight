using System.Collections;
using UnityEngine;

/// <summary>A warning followed by a permanent resin stain with gentle contact damage.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public sealed class StickyPuddle : MonoBehaviour
{
    [SerializeField] private SpriteRenderer visual;
    [SerializeField] private Sprite warningSprite;
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Color warningColor = new(1f, 1f, 1f, 0.9f);
    [SerializeField] private Color puddleColor = Color.white;
    [SerializeField, Min(0f)] private float slowAfterExitSeconds = 3f;
    [SerializeField, Min(1)] private int damagePerTick = 1;
    [SerializeField, Min(0.1f)] private float damageInterval = 1f;

    private BoxCollider2D area;
    private TopDownPlayer occupant;
    private float slowMultiplier;
    private float nextDamageAt;
    private bool active;
    private Vector3 visualScale;

    public bool IsWarning => !active && this != null;
    public bool IsActivePuddle => active;
    public Vector2 TargetPosition => transform.position;

    private void Awake()
    {
        area = GetComponent<BoxCollider2D>();
        area.isTrigger = true;
        area.enabled = false;
        if (visual == null) visual = GetComponentInChildren<SpriteRenderer>();
        if (visual != null) visualScale = visual.transform.localScale;
    }

    public void Begin(float warningSeconds, float movementMultiplier)
    {
        slowMultiplier = Mathf.Clamp(movementMultiplier, 0.05f, 1f);
        if (visual != null && warningSprite != null) visual.sprite = warningSprite;
        StartCoroutine(WarningThenActive(Mathf.Max(0.1f, warningSeconds)));
    }

    public void CancelWarning()
    {
        if (!active) Destroy(gameObject);
    }

    private IEnumerator WarningThenActive(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            if (visual != null)
            {
                Color c = warningColor;
                c.a *= 0.7f + 0.3f * Mathf.PingPong(Time.time * 5f, 1f);
                visual.color = c;
                visual.transform.localScale = visualScale *
                    (0.92f + 0.06f * Mathf.PingPong(Time.time * 4f, 1f));
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        active = true;
        GameSfx.PlayWorld(SfxCue.TrapCast, transform.position);
        if (visual != null)
        {
            if (activeSprite != null) visual.sprite = activeSprite;
            visual.color = puddleColor;
            visual.transform.localScale = visualScale;
        }
        area.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other) => Touch(other);
    private void OnTriggerStay2D(Collider2D other) => Touch(other);

    private void Touch(Collider2D other)
    {
        if (!active) return;
        TopDownPlayer player = other.GetComponent<TopDownPlayer>();
        if (player == null || !player.IsAlive) return;
        if (occupant != player)
        {
            occupant = player;
            nextDamageAt = Time.time + damageInterval;
        }
        player.SetAreaSlow(this, slowMultiplier);
        if (Time.time >= nextDamageAt)
        {
            nextDamageAt = Time.time + damageInterval;
            player.ReceiveDamage(damagePerTick);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (occupant != null && other.GetComponent<TopDownPlayer>() == occupant) ClearSlow(true);
    }

    private void ClearSlow(bool linger)
    {
        if (occupant != null)
        {
            occupant.RemoveAreaSlow(this);
            if (linger && active) occupant.ApplyLingeringSlow(slowMultiplier, slowAfterExitSeconds);
        }
        occupant = null;
        nextDamageAt = 0f;
    }

    private void OnDisable() => ClearSlow(active);
    private void OnDestroy() => ClearSlow(active);
}
