using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adapter to the arena enemies' existing hits-to-kill counters. Healing never
/// creates a second HP pool: every 10 healing points restores one missing hit.
/// </summary>
[DisallowMultipleComponent]
public sealed class EnemySupportable : MonoBehaviour
{
    private static readonly HashSet<EnemySupportable> activeAllies = new();
    public static IEnumerable<EnemySupportable> ActiveAllies => activeAllies;
    [SerializeField, Min(1f)] private float healingPointsPerHit = 10f;

    private IEnemySupportTarget health;
    private MonoBehaviour healthBehaviour;
    private TrapperEnemy trapper;
    private float partialHealing;
    private IoTether activeHealer;
    private readonly Dictionary<IoTether, float> speedSources = new();

    public bool IsAlive => isActiveAndEnabled && healthBehaviour != null &&
        healthBehaviour.isActiveAndEnabled && health.CurrentHits > 0;
    public bool CanReceiveIoSupport => IsAlive && (trapper == null || !trapper.IsInvisible);

    public float HealthFraction
    {
        get
        {
            return health != null && health.MaximumHits > 0 ?
                (float)health.CurrentHits / health.MaximumHits : 0f;
        }
    }

    public int MaximumHits => health != null ? health.MaximumHits : 0;

    public bool IsClaimedByOtherIo(IoTether source) =>
        activeHealer != null && activeHealer != source && activeHealer.isActiveAndEnabled;

    public bool TryClaimIo(IoTether source)
    {
        if (source == null || !CanReceiveIoSupport || IsClaimedByOtherIo(source)) return false;
        if (activeHealer != source) partialHealing = 0f;
        activeHealer = source;
        return true;
    }

    public void ReleaseIo(IoTether source)
    {
        if (activeHealer == source) activeHealer = null;
    }

    public float SpeedMultiplier
    {
        get
        {
            if (!CanReceiveIoSupport) return 1f;
            float highestBonus = 0f;
            foreach (KeyValuePair<IoTether, float> source in speedSources)
                if (source.Key != null && source.Key.isActiveAndEnabled)
                    highestBonus = Mathf.Max(highestBonus, source.Value);
            return 1f + highestBonus;
        }
    }

    private void Awake()
    {
        trapper = GetComponent<TrapperEnemy>();
        foreach (MonoBehaviour behaviour in GetComponents<MonoBehaviour>())
            if (behaviour is IEnemySupportTarget target)
            {
                health = target;
                healthBehaviour = behaviour;
                break;
            }
    }

    private void OnEnable() => activeAllies.Add(this);

    public void HealFromIo(IoTether source, float points)
    {
        if (activeHealer != source || !CanReceiveIoSupport || points <= 0f) return;
        if (HealthFraction >= 1f) { partialHealing = 0f; return; }
        partialHealing += points;
        while (partialHealing >= healingPointsPerHit && HealthFraction < 1f)
        {
            partialHealing -= healingPointsPerHit;
            health.RestoreOneHit();
        }
    }

    public void SuspendIoSupport()
    {
        // Do not preserve almost-complete healing through an invisible escape.
        partialHealing = 0f;
    }

    public void AddSpeedSource(IoTether source, float bonus)
    {
        if (source != null) speedSources[source] = Mathf.Max(0f, bonus);
    }

    public void RemoveSpeedSource(IoTether source)
    {
        if (source != null) speedSources.Remove(source);
    }

    private void OnDisable()
    {
        activeAllies.Remove(this);
        activeHealer = null;
        partialHealing = 0f;
        speedSources.Clear();
    }


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ClearRegistry() => activeAllies.Clear();
}
