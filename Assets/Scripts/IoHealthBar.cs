using UnityEngine;

/// <summary>One small pixel block per sword hit, shown when Io is in combat.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(IoEnemy))]
public sealed class IoHealthBar : MonoBehaviour
{
    [SerializeField] private Vector2 localOffset = new(0f, 0.95f);
    [SerializeField, Min(0.03f)] private float segmentWidth = 0.11f;
    [SerializeField, Min(0.03f)] private float segmentHeight = 0.055f;
    [SerializeField, Min(0f)] private float segmentGap = 0.025f;
    [SerializeField, Min(1)] private int healthPerSegment = 10;
    [SerializeField, Min(0.5f)] private float showDistance = 7f;
    [SerializeField] private Color fullColor = new(0.36f, 0.94f, 0.92f, 1f);
    [SerializeField] private Color emptyColor = new(0.1f, 0.19f, 0.24f, 0.85f);

    private static Sprite pixel;
    private IoEnemy io;
    private TopDownPlayer player;
    private SpriteRenderer character;
    private SpriteRenderer[] blocks;
    private int lastHealth = -1;

    private void Awake()
    {
        io = GetComponent<IoEnemy>();
        player = FindFirstObjectByType<TopDownPlayer>();
        int count = Mathf.Max(1, Mathf.CeilToInt((float)io.MaximumHealth / healthPerSegment));
        blocks = new SpriteRenderer[count];
        float totalWidth = count * segmentWidth + (count - 1) * segmentGap;
        Transform visual = transform.Find("IoVisual (editable)");
        character = visual != null ? visual.GetComponent<SpriteRenderer>() : null;
        for (int i = 0; i < count; i++)
        {
            GameObject block = new GameObject("HP block " + (i + 1));
            block.transform.SetParent(transform, false);
            block.transform.localPosition = new Vector3(localOffset.x - totalWidth * 0.5f +
                segmentWidth * 0.5f + i * (segmentWidth + segmentGap), localOffset.y, 0f);
            block.transform.localScale = new Vector3(segmentWidth, segmentHeight, 1f);
            SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
            renderer.sprite = GetPixel();
            renderer.sortingOrder = character != null ? character.sortingOrder + 1 : 25;
            blocks[i] = renderer;
        }
        Refresh();
    }

    private void LateUpdate()
    {
        if (blocks == null || io == null) return;
        if (io.CurrentHealth != lastHealth) Refresh();
        if (player == null) player = FindFirstObjectByType<TopDownPlayer>();
        bool visible = io.IsAlive && player != null && player.IsAlive &&
            ((Vector2)(player.transform.position - transform.position)).sqrMagnitude <=
            showDistance * showDistance;
        for (int i = 0; i < blocks.Length; i++)
            if (blocks[i] != null)
            {
                blocks[i].enabled = visible;
                if (character != null) blocks[i].sortingOrder = character.sortingOrder + 1;
            }
    }

    public void Refresh()
    {
        if (io == null || blocks == null) return;
        lastHealth = io.CurrentHealth;
        for (int i = 0; i < blocks.Length; i++)
            if (blocks[i] != null)
                blocks[i].color = i * healthPerSegment < lastHealth ? fullColor : emptyColor;
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
        pixel.name = "Io HP pixel";
        return pixel;
    }
}
