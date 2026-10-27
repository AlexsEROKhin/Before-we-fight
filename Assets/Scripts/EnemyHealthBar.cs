using UnityEngine;

/// <summary>Shows the enemy's existing hits-to-kill counter; does not own health.</summary>
[DisallowMultipleComponent]
public sealed class EnemyHealthBar : MonoBehaviour
{
    [SerializeField, Min(0f)] private float gapAboveSprite = 0.13f;
    [SerializeField, Min(0.03f)] private float segmentWidth = 0.13f;
    [SerializeField, Min(0.03f)] private float segmentHeight = 0.055f;
    [SerializeField, Min(0f)] private float segmentGap = 0.025f;
    [SerializeField, Min(0.5f)] private float showDistance = 7f;
    [SerializeField] private Color fullColor = new(0.85f, 0.26f, 0.24f, 1f);
    [SerializeField] private Color emptyColor = new(0.22f, 0.14f, 0.14f, 0.85f);

    private static Sprite pixel;
    private IEnemySupportTarget health;
    private MonoBehaviour healthBehaviour;
    private TopDownPlayer player;
    private SpriteRenderer character;
    private TrapperEnemy trapper;
    private SpriteRenderer[] blocks;
    private int lastHits = -1;

    private void Awake()
    {
        foreach (MonoBehaviour behaviour in GetComponents<MonoBehaviour>())
            if (behaviour is IEnemySupportTarget target)
            {
                health = target;
                healthBehaviour = behaviour;
                break;
            }
        if (health == null) { enabled = false; return; }

        player = FindFirstObjectByType<TopDownPlayer>();
        trapper = GetComponent<TrapperEnemy>();
        character = GetComponent<SpriteRenderer>();
        if (character == null) character = GetComponentInChildren<SpriteRenderer>();
        int count = Mathf.Max(1, health.MaximumHits);
        blocks = new SpriteRenderer[count];
        float width = count * segmentWidth + (count - 1) * segmentGap;
        float scaleY = Mathf.Max(0.001f, Mathf.Abs(transform.lossyScale.y));
        float top = character != null ? (character.bounds.max.y - transform.position.y) / scaleY : 0.5f;
        float y = Mathf.Max(0.2f, top) + gapAboveSprite;
        for (int i = 0; i < count; i++)
        {
            GameObject block = new GameObject("HP block " + (i + 1));
            block.transform.SetParent(transform, false);
            block.transform.localPosition = new Vector3(-width * 0.5f + segmentWidth * 0.5f +
                i * (segmentWidth + segmentGap), y, 0f);
            block.transform.localScale = new Vector3(segmentWidth, segmentHeight, 1f);
            SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
            renderer.sprite = GetPixel();
            renderer.sortingLayerID = character != null ? character.sortingLayerID : 0;
            renderer.sortingOrder = character != null ? character.sortingOrder + 1 : 1;
            blocks[i] = renderer;
        }
        Refresh();
    }

    private void LateUpdate()
    {
        if (blocks == null || health == null) return;
        if (health.CurrentHits != lastHits) Refresh();
        if (player == null) player = FindFirstObjectByType<TopDownPlayer>();
        bool visible = healthBehaviour != null && healthBehaviour.isActiveAndEnabled &&
            health.CurrentHits > 0 && (trapper == null || !trapper.IsInvisible) &&
            player != null && player.IsAlive &&
            ((Vector2)(player.transform.position - transform.position)).sqrMagnitude <=
            showDistance * showDistance;
        for (int i = 0; i < blocks.Length; i++)
            if (blocks[i] != null)
            {
                blocks[i].enabled = visible;
                if (character != null) blocks[i].sortingOrder = character.sortingOrder + 1;
            }
    }

    private void Refresh()
    {
        lastHits = health.CurrentHits;
        for (int i = 0; i < blocks.Length; i++)
            if (blocks[i] != null) blocks[i].color = i < lastHits ? fullColor : emptyColor;
    }

    private static Sprite GetPixel()
    {
        if (pixel != null) return pixel;
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        pixel = Sprite.Create(texture, new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f), 1f);
        pixel.name = "Enemy HP pixel";
        return pixel;
    }
}
