using UnityEngine;

// Place this on an editable enemy camp and assign its members in the Inspector.
[DisallowMultipleComponent]
public sealed class EnemyCampGate : MonoBehaviour
{
    [SerializeField] private EnemyHitFeedback[] enemies;

    public int RemainingEnemies
    {
        get
        {
            int remaining = 0;
            if (enemies == null) return remaining;
            foreach (EnemyHitFeedback enemy in enemies)
                if (enemy != null && enemy.gameObject.activeInHierarchy && enemy.enabled)
                    remaining++;
            return remaining;
        }
    }

    public bool IsCleared => enemies != null && enemies.Length > 0 && RemainingEnemies == 0;
}
