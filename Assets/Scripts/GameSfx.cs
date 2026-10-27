using UnityEngine;

public enum SfxCue
{
    StepA, StepB, Jump, Dash, SwordWhoosh, SwordHit, HeroHurt, Heal,
    MageWarning, MageImpact, RingAttack, TrapCast, IoLaunch, EnemyStrike
}

/// <summary>Editable sound palette for Arena. World effects are distance-limited and voice-limited.</summary>
[DisallowMultipleComponent]
public sealed class GameSfx : MonoBehaviour
{
    public static GameSfx Instance { get; private set; }

    [Header("Mix")]
    [SerializeField, Range(0f, 1f)] private float masterVolume = 0.65f;
    [SerializeField, Range(0f, 1f)] private float heroVolume = 0.8f;
    [SerializeField, Range(0f, 1f)] private float footstepVolume = 0.45f;
    [SerializeField, Range(0f, 1f)] private float enemyVolume = 0.65f;
    [SerializeField, Min(1f)] private float enemyHearingRadius = 13f;
    [Header("Hero")]
    [SerializeField] private AudioClip stepA;
    [SerializeField] private AudioClip stepB;
    [SerializeField] private AudioClip jump;
    [SerializeField] private AudioClip dash;
    [SerializeField] private AudioClip swordWhoosh;
    [SerializeField] private AudioClip swordHit;
    [SerializeField] private AudioClip heroHurt;
    [SerializeField] private AudioClip heal;
    [Header("Enemies")]
    [SerializeField] private AudioClip mageWarning;
    [SerializeField] private AudioClip mageImpact;
    [SerializeField] private AudioClip ringAttack;
    [SerializeField] private AudioClip trapCast;
    [SerializeField] private AudioClip ioLaunch;
    [SerializeField] private AudioClip enemyStrike;

    private const int EnemyVoices = 6;
    private AudioSource heroSource;
    private AudioSource[] enemySources;
    private TopDownPlayer player;
    private readonly float[] nextWorldCueAt = new float[14];

    private void Awake()
    {
        if (Instance != null && Instance != this) { enabled = false; return; }
        Instance = this;
        player = FindFirstObjectByType<TopDownPlayer>();
        heroSource = NewSource("Hero SFX");
        enemySources = new AudioSource[EnemyVoices];
        for (int i = 0; i < enemySources.Length; i++)
            enemySources[i] = NewSource("Enemy SFX " + (i + 1));
    }

    private AudioSource NewSource(string objectName)
    {
        GameObject child = new GameObject(objectName);
        child.transform.SetParent(transform, false);
        AudioSource source = child.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.loop = false;
        return source;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public static void PlayHero(SfxCue cue)
    {
        if (Instance == null || !Instance.isActiveAndEnabled) return;
        AudioClip clip = Instance.Clip(cue);
        if (clip == null) return;
        float cueVolume = cue == SfxCue.StepA || cue == SfxCue.StepB
            ? Instance.footstepVolume : 1f;
        Instance.heroSource.PlayOneShot(clip,
            Instance.masterVolume * Instance.heroVolume * cueVolume);
        if (cue == SfxCue.HeroHurt || cue == SfxCue.SwordHit)
            PersistentMusic.Duck(0.25f, 0.82f);
    }

    public static void PlayWorld(SfxCue cue, Vector2 position)
    {
        if (Instance == null || !Instance.isActiveAndEnabled) return;
        GameSfx mix = Instance;
        AudioClip clip = mix.Clip(cue);
        if (clip == null) return;
        if (mix.player == null) mix.player = FindFirstObjectByType<TopDownPlayer>();
        if (mix.player == null) return;
        float distance = Vector2.Distance(position, mix.player.transform.position);
        if (distance >= mix.enemyHearingRadius) return;
        int index = (int)cue;
        if (Time.time < mix.nextWorldCueAt[index]) return;
        foreach (AudioSource source in mix.enemySources)
        {
            if (source.isPlaying) continue;
            float attenuation = Mathf.Lerp(1f, 0f, distance / mix.enemyHearingRadius);
            source.PlayOneShot(clip, mix.masterVolume * mix.enemyVolume * attenuation);
            if (cue == SfxCue.MageImpact || cue == SfxCue.RingAttack)
                PersistentMusic.Duck(0.2f, 0.82f);
            mix.nextWorldCueAt[index] = Time.time + 0.07f;
            return;
        }
    }

    private AudioClip Clip(SfxCue cue) => cue switch
    {
        SfxCue.StepA => stepA,
        SfxCue.StepB => stepB,
        SfxCue.Jump => jump,
        SfxCue.Dash => dash,
        SfxCue.SwordWhoosh => swordWhoosh,
        SfxCue.SwordHit => swordHit,
        SfxCue.HeroHurt => heroHurt,
        SfxCue.Heal => heal,
        SfxCue.MageWarning => mageWarning,
        SfxCue.MageImpact => mageImpact,
        SfxCue.RingAttack => ringAttack,
        SfxCue.TrapCast => trapCast,
        SfxCue.IoLaunch => ioLaunch,
        SfxCue.EnemyStrike => enemyStrike,
        _ => null
    };
}
