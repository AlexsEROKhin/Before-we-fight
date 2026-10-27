using System.Collections;
using UnityEngine;

/// <summary>Editable, reusable arena healing item. Full-health heroes leave it in place.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D))]
public sealed class HealingPickup : MonoBehaviour
{
    [SerializeField, Min(1)] private int healAmount = 80;
    [SerializeField, Min(0f)] private float respawnSeconds = 45f;

    private SpriteRenderer icon;
    private CircleCollider2D trigger;
    private bool available = true;

    private void Awake()
    {
        icon = GetComponent<SpriteRenderer>();
        trigger = GetComponent<CircleCollider2D>();
        trigger.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other) => TryCollect(other);
    private void OnTriggerStay2D(Collider2D other) => TryCollect(other);

    private void TryCollect(Collider2D other)
    {
        if (!available) return;
        TopDownPlayer hero = other.GetComponent<TopDownPlayer>();
        if (hero == null) hero = other.GetComponentInParent<TopDownPlayer>();
        if (hero == null || !hero.TryHeal(healAmount)) return;

        available = false;
        icon.enabled = false;
        trigger.enabled = false;
        if (respawnSeconds > 0f) StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnSeconds);
        available = true;
        icon.enabled = true;
        trigger.enabled = true;
    }
}
