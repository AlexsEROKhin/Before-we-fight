using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Full-arena camera view with small, editable pixel-art markers.</summary>
[DisallowMultipleComponent]
public sealed class ArenaMinimap : MonoBehaviour
{
    [Header("Scene references")]
    [SerializeField] private Camera mapCamera;
    [SerializeField] private RawImage mapImage;
    [SerializeField] private RectTransform playerMarker;
    [SerializeField] private RectTransform enemyMarkerTemplate;
    [SerializeField] private Transform player;

    [Header("Editable marker art")]
    [SerializeField] private Sprite heroIcon;
    [SerializeField] private Sprite mageIcon;
    [SerializeField] private Sprite ringIcon;
    [SerializeField] private Sprite ioIcon;
    [SerializeField] private Sprite trapperIcon;
    [SerializeField] private Sprite otherEnemyIcon;

    [Header("Editable arena bounds")]
    [SerializeField] private Vector2 worldMin = new Vector2(-50f, -50f);
    [SerializeField] private Vector2 worldMax = new Vector2(50f, 50f);
    [SerializeField, Min(64)] private int textureResolution = 256;

    private RenderTexture mapTexture;
    private readonly List<EnemyMarker> enemyMarkers = new List<EnemyMarker>();
    private float nextEnemyScanAt;

    private sealed class EnemyMarker
    {
        public EnemyHitFeedback Enemy;
        public RectTransform Marker;
        public TrapperEnemy Trapper;
    }

    private void Awake()
    {
        if (mapCamera == null || mapImage == null || playerMarker == null ||
            enemyMarkerTemplate == null || player == null)
        {
            Debug.LogWarning("Arena minimap is missing an editable scene reference.", this);
            enabled = false;
            return;
        }

        int resolution = Mathf.Clamp(textureResolution, 64, 1024);
        mapTexture = new RenderTexture(resolution, resolution, 16, RenderTextureFormat.ARGB32)
        {
            name = "Arena Minimap (runtime)",
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            useMipMap = false,
            autoGenerateMips = false
        };
        mapTexture.Create();

        Vector2 center = (worldMin + worldMax) * 0.5f;
        mapCamera.orthographic = true;
        mapCamera.orthographicSize = Mathf.Max(worldMax.x - worldMin.x,
            worldMax.y - worldMin.y) * 0.5f + 0.5f;
        mapCamera.transform.position = new Vector3(center.x, center.y, -10f);
        mapCamera.targetTexture = mapTexture;
        mapCamera.enabled = true;
        mapImage.texture = mapTexture;
        mapImage.raycastTarget = false;
        Image heroImage = playerMarker.GetComponent<Image>();
        if (heroImage != null && heroIcon != null)
        {
            heroImage.sprite = heroIcon;
            heroImage.color = Color.white;
            heroImage.preserveAspect = true;
        }
        SyncEnemyMarkers();
    }

    private void LateUpdate()
    {
        if (player == null || mapImage == null || mapCamera == null) return;
        Rect mapRect = mapImage.rectTransform.rect;
        playerMarker.anchoredPosition = MarkerPosition(player.position, mapRect);

        if (Time.unscaledTime >= nextEnemyScanAt)
        {
            SyncEnemyMarkers();
            nextEnemyScanAt = Time.unscaledTime + 1f;
        }

        for (int i = enemyMarkers.Count - 1; i >= 0; i--)
        {
            EnemyMarker entry = enemyMarkers[i];
            if (entry.Enemy == null)
            {
                if (entry.Marker != null) Destroy(entry.Marker.gameObject);
                enemyMarkers.RemoveAt(i);
                continue;
            }
            bool visible = entry.Trapper == null || !entry.Trapper.IsInvisible;
            if (entry.Marker.gameObject.activeSelf != visible)
                entry.Marker.gameObject.SetActive(visible);
            if (!visible) continue;
            entry.Marker.anchoredPosition = MarkerPosition(entry.Enemy.transform.position, mapRect);
        }
    }

    private Vector2 MarkerPosition(Vector3 worldPosition, Rect mapRect)
    {
        Vector3 viewport = mapCamera.WorldToViewportPoint(worldPosition);
        return new Vector2(
            (Mathf.Clamp01(viewport.x) - 0.5f) * mapRect.width,
            (Mathf.Clamp01(viewport.y) - 0.5f) * mapRect.height);
    }

    private void SyncEnemyMarkers()
    {
        EnemyHitFeedback[] enemies = FindObjectsByType<EnemyHitFeedback>(FindObjectsSortMode.None);
        foreach (EnemyHitFeedback enemy in enemies)
        {
            bool found = false;
            foreach (EnemyMarker entry in enemyMarkers)
            {
                if (entry.Enemy != enemy) continue;
                found = true;
                break;
            }
            if (found) continue;

            RectTransform marker = Instantiate(enemyMarkerTemplate, mapImage.rectTransform, false);
            marker.name = "Enemy Marker - " + enemy.name;
            Image markerImage = marker.GetComponent<Image>();
            if (markerImage != null)
            {
                Sprite icon = IconFor(enemy);
                if (icon != null) markerImage.sprite = icon;
                markerImage.color = Color.white;
                markerImage.preserveAspect = true;
            }
            marker.gameObject.SetActive(true);
            marker.SetAsFirstSibling(); // Keep the hero marker readable above enemy markers.
            enemyMarkers.Add(new EnemyMarker
            { Enemy = enemy, Marker = marker, Trapper = enemy.GetComponent<TrapperEnemy>() });
        }
    }

    private Sprite IconFor(EnemyHitFeedback enemy)
    {
        if (enemy.GetComponent<MageCreep>() != null) return mageIcon;
        if (enemy.GetComponent<RingCreep>() != null) return ringIcon;
        if (enemy.GetComponent<IoEnemy>() != null) return ioIcon;
        if (enemy.GetComponent<TrapperEnemy>() != null) return trapperIcon;
        return otherEnemyIcon;
    }

    private void OnDestroy()
    {
        if (mapCamera != null)
        {
            mapCamera.targetTexture = null;
            mapCamera.enabled = false;
        }
        if (mapImage != null) mapImage.texture = null;
        if (mapTexture != null)
        {
            mapTexture.Release();
            Destroy(mapTexture);
        }
    }
}
