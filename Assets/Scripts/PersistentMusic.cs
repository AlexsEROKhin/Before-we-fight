using UnityEngine;

/// <summary>One editable background-music source shared by the title and arena.</summary>
[DisallowMultipleComponent]
public sealed class PersistentMusic : MonoBehaviour
{
    [SerializeField] private AudioClip music;
    [SerializeField, Range(0f, 1f)] private float volume = 0.22f;

    private static PersistentMusic current;
    private AudioSource source;
    private float duckUntil;
    private float duckFactor = 1f;

    private void Awake()
    {
        if (current != null && current != this)
        {
            Destroy(gameObject);
            return;
        }

        current = this;
        DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();
        if (source == null) source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.volume = volume;
        source.clip = music;
        if (music != null) source.Play();
    }

    public static void Duck(float seconds, float factor)
    {
        if (current == null) return;
        current.duckUntil = Mathf.Max(current.duckUntil, Time.unscaledTime + seconds);
        current.duckFactor = Mathf.Min(current.duckFactor, Mathf.Clamp01(factor));
    }

    private void Update()
    {
        if (source == null) return;
        bool ducked = Time.unscaledTime < duckUntil;
        float target = volume * (ducked ? duckFactor : 1f);
        source.volume = Mathf.MoveTowards(source.volume, target, Time.unscaledDeltaTime * 0.7f);
        if (!ducked) duckFactor = 1f;
    }

    private void OnDestroy()
    {
        if (current == this) current = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetInstance() => current = null;
}
