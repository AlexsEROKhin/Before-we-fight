using System.Collections;
using UnityEngine;

public static class EnemyDefeat
{
    public static void RewardAndDestroy(GameObject enemy, int experience = 1)
    {
        if (enemy == null || !enemy.activeSelf) return;
        // Destruction happens at frame end; deactivate now so repeated hits
        // cannot award experience or healing twice in that same frame.
        enemy.SetActive(false);
        TopDownPlayer player = Object.FindFirstObjectByType<TopDownPlayer>();
        if (player != null) player.RewardEnemyDefeat(experience);
        Object.Destroy(enemy);
    }

    public static void PlayDeath(MonoBehaviour enemy, SpriteRenderer visual, Rigidbody2D body, Sprite[] frames, int start, int count, int experience = 1)
    {
        if (enemy == null || !enemy.enabled) return;
        enemy.StopAllCoroutines();
        enemy.StartCoroutine(DeathRoutine(enemy, visual, body, frames, start, count, experience));
    }

    private static IEnumerator DeathRoutine(MonoBehaviour enemy, SpriteRenderer visual, Rigidbody2D body, Sprite[] frames, int start, int count, int experience)
    {
        enemy.enabled = false;
        EnemyHitFeedback hitFeedback = enemy.GetComponent<EnemyHitFeedback>();
        if (hitFeedback != null) hitFeedback.enabled = false;
        foreach (Collider2D collider in enemy.GetComponents<Collider2D>()) collider.enabled = false;
        if (body != null) { body.linearVelocity = Vector2.zero; body.simulated = false; }
        TopDownPlayer player = Object.FindFirstObjectByType<TopDownPlayer>();
        if (player != null) player.RewardEnemyDefeat(experience);
        if (visual != null && frames != null && frames.Length >= start + count)
            for (int i = 0; i < count; i++) { visual.sprite = frames[start + i]; yield return new WaitForSeconds(.1f); }
        if (visual != null)
        {
            Color original = visual.color;
            float elapsed = 0f;
            while (elapsed < 0.16f)
            {
                float t = elapsed / 0.16f;
                visual.color = new Color(original.r, original.g, original.b, original.a * (1f - t));
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        Object.Destroy(enemy.gameObject);
    }
}
