/// <summary>Existing hit counters exposed to Io without a second health system.</summary>
public interface IEnemySupportTarget
{
    int CurrentHits { get; }
    int MaximumHits { get; }
    void RestoreOneHit();
}
