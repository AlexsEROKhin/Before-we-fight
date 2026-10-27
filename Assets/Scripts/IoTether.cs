using UnityEngine;

/// <summary>One live ally link: selection, healing, speed buff and visuals.</summary>
[DisallowMultipleComponent]
public sealed class IoTether : MonoBehaviour
{
    [Header("Support")]
    [SerializeField, Min(0.1f)] private float allyDetectionRange = 5f;
    [SerializeField, Min(0.1f)] private float tetherBreakDistance = 6f;
    [SerializeField, Min(0f)] private float healingPerSecond = 5f;
    [SerializeField, Range(0f, 2f)] private float movementSpeedBonus = 0.25f;
    [SerializeField, Min(0.1f)] private float targetScanSeconds = 0.35f;
    [Header("Editable tether visual")]
    [SerializeField] private Color tetherColor = new(0.25f, 0.85f, 1f, 0.85f);
    [SerializeField, Min(0.005f)] private float tetherWidth = 0.055f;
    [SerializeField, Min(0f)] private float pulseSpeed = 2.5f;

    private IoEnemy owner;
    private TopDownPlayer player;
    private EnemySupportable target;
    private LineRenderer line;
    private LineRenderer pulse;
    private LineRenderer buffRing;
    private Material lineMaterial;
    private float nextScanAt;

    public EnemySupportable Target => target;
    public float AllyDetectionRange => allyDetectionRange;
    public float TetherBreakDistance => tetherBreakDistance;

    private void Awake()
    {
        owner = GetComponent<IoEnemy>();
        player = FindFirstObjectByType<TopDownPlayer>();
        line = GetComponent<LineRenderer>();
        if (line == null) line = gameObject.AddComponent<LineRenderer>();
        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            lineMaterial = new Material(shader) { name = "Io Tether (runtime)" };
            line.material = lineMaterial;
        }
        ConfigureLine(line, tetherWidth, tetherColor);
        GameObject pulseObject = new GameObject("Io Energy Pulse");
        pulseObject.transform.SetParent(transform, false);
        pulse = pulseObject.AddComponent<LineRenderer>();
        if (lineMaterial != null) pulse.sharedMaterial = lineMaterial;
        ConfigureLine(pulse, tetherWidth * 1.7f, Color.white);
        SetVisible(false);
    }

    private void ConfigureLine(LineRenderer renderer, float width, Color color)
    {
        renderer.useWorldSpace = true;
        renderer.positionCount = 2;
        renderer.startWidth = width;
        renderer.endWidth = width;
        renderer.startColor = color;
        renderer.endColor = color;
        renderer.textureMode = LineTextureMode.Stretch;
        renderer.sortingOrder = 20;
    }

    private void Update()
    {
        if (owner == null || !owner.IsAlive)
        {
            BreakLink();
            return;
        }
        if (target != null && (!target.CanReceiveIoSupport ||
            Vector2.Distance(transform.position, target.transform.position) > tetherBreakDistance))
            BreakLink();

        if (Time.time >= nextScanAt)
        {
            nextScanAt = Time.time + targetScanSeconds;
            EnemySupportable best = FindBestAlly();
            if (best != target) SetTarget(best);
        }
        if (target != null && target.CanReceiveIoSupport)
            target.HealFromIo(this, healingPerSecond * Time.deltaTime);
    }

    private EnemySupportable FindBestAlly()
    {
        EnemySupportable best = null;
        float bestScore = float.NegativeInfinity;
        float rangeSq = allyDetectionRange * allyDetectionRange;
        foreach (EnemySupportable candidate in EnemySupportable.ActiveAllies)
        {
            if (candidate == null || !candidate.CanReceiveIoSupport ||
                candidate.IsClaimedByOtherIo(this) ||
                candidate.gameObject == gameObject) continue;
            float distanceSq = ((Vector2)(candidate.transform.position - transform.position)).sqrMagnitude;
            float allowedRangeSq = candidate == target
                ? tetherBreakDistance * tetherBreakDistance : rangeSq;
            if (distanceSq > allowedRangeSq) continue;
            float danger = player != null && player.IsAlive &&
                ((Vector2)(player.transform.position - candidate.transform.position)).sqrMagnitude < 16f ? 1f : 0f;
            float score = danger * 5f + (1f - candidate.HealthFraction) * 4f +
                Mathf.Min(1f, candidate.MaximumHits * 0.12f) -
                Mathf.Sqrt(distanceSq) * 0.08f + (candidate == target ? 0.3f : 0f);
            if (score > bestScore)
            {
                best = candidate;
                bestScore = score;
            }
        }
        return best;
    }

    private void SetTarget(EnemySupportable next)
    {
        BreakLink();
        if (next == null || !next.TryClaimIo(this)) return;
        target = next;
        target.AddSpeedSource(this, movementSpeedBonus);
        GameObject indicator = new GameObject("Io Buff Indicator");
        indicator.transform.SetParent(target.transform, false);
        indicator.transform.localPosition = Vector3.down * 0.08f;
        buffRing = indicator.AddComponent<LineRenderer>();
        buffRing.sharedMaterial = lineMaterial;
        buffRing.useWorldSpace = false;
        buffRing.loop = true;
        buffRing.positionCount = 12;
        buffRing.startWidth = buffRing.endWidth = 0.035f;
        Color buffColor = new Color(0.28f, 0.95f, 0.75f, 0.55f);
        buffRing.startColor = buffRing.endColor = buffColor;
        buffRing.sortingOrder = 18;
        for (int i = 0; i < 12; i++)
        {
            float angle = i * Mathf.PI * 2f / 12f;
            buffRing.SetPosition(i, new Vector3(Mathf.Cos(angle) * 0.38f,
                Mathf.Sin(angle) * 0.18f, 0f));
        }
        SetVisible(true);
    }

    private void LateUpdate()
    {
        if (target == null || !target.CanReceiveIoSupport || owner == null || !owner.IsAlive)
        {
            SetVisible(false);
            return;
        }
        Vector3 start = transform.position + Vector3.up * 0.2f;
        Vector3 end = target.transform.position + Vector3.up * 0.15f;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        float phase = Mathf.Repeat(Time.time * pulseSpeed, 1f);
        pulse.SetPosition(0, Vector3.Lerp(start, end, phase));
        pulse.SetPosition(1, Vector3.Lerp(start, end, Mathf.Min(1f, phase + 0.06f)));
    }

    public void BreakLink()
    {
        if (target != null)
        {
            target.ReleaseIo(this);
            target.RemoveSpeedSource(this);
        }
        if (buffRing != null) Destroy(buffRing.gameObject);
        buffRing = null;
        target = null;
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        if (line != null) line.enabled = visible;
        if (pulse != null) pulse.enabled = visible;
    }

    private void OnDisable() => BreakLink();

    private void OnDestroy()
    {
        BreakLink();
        if (lineMaterial != null) Destroy(lineMaterial);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.25f, 0.85f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, allyDetectionRange);
        Gizmos.color = new Color(1f, 0.75f, 0.2f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, tetherBreakDistance);
    }
}
