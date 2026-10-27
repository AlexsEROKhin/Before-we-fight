using UnityEngine;

/// <summary>Inspector-editable encounter pacing; no health or damage inflation.</summary>
[DisallowMultipleComponent]
public sealed class EnemySquadStage : MonoBehaviour
{
    [SerializeField, Range(1, 10)] private int stage = 1;
    [SerializeField, Range(0.7f, 1.5f)] private float earlyCooldownScale = 1.18f;
    [SerializeField, Range(0.7f, 1.5f)] private float lateCooldownScale = 0.82f;
    [SerializeField, Range(0.7f, 1.5f)] private float earlyMovementScale = 0.95f;
    [SerializeField, Range(0.7f, 1.5f)] private float lateMovementScale = 1.17f;
    [SerializeField, Range(0f, 2f)] private float lateDetectionBonus = 1f;

    private float Progress => (stage - 1f) / 9f;
    public float CooldownScale => Mathf.Lerp(earlyCooldownScale, lateCooldownScale, Progress);
    public float MovementScale => Mathf.Lerp(earlyMovementScale, lateMovementScale, Progress);
    public float DetectionBonus => lateDetectionBonus * Progress;
}
