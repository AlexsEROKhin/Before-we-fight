using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Places the user's Aseprite variants as individually editable scene objects.
public static class InstallUserMapDressing
{
    private const string ArtFolder = "Assets/Art/UserMapDressing";
    private const string RootName = "User Bushes and Stones (editable)";

    [MenuItem("Tools/Before We Fight/Place User Bushes and Stones")]
    public static void Install()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlaying || scene.name != "Arena")
        {
            Debug.LogError("Stop Play mode and open Arena before placing map dressing.");
            return;
        }

        GameObject world = GameObject.Find("WORLD - edit positions here");
        GameObject forest = GameObject.Find("Forest Dressing (editable)");
        if (world == null || forest == null)
        {
            Debug.LogError("Arena WORLD or Forest Dressing is missing.");
            return;
        }
        if (GameObject.Find(RootName) != null)
        {
            Debug.LogWarning("User bushes and stones already exist. Move individual objects in the Hierarchy to edit them.");
            return;
        }

        Sprite[] bushes = ImportVariants("Bush", 12);
        Sprite[] stones = ImportVariants("Stone", 5);
        if (bushes.Length != 12 || stones.Length != 5)
        {
            Debug.LogError("Could not import all 12 bushes and 5 stones.");
            return;
        }

        List<Vector2> trees = forest.GetComponentsInChildren<SpriteRenderer>(true)
            .Select(renderer => (Vector2)renderer.transform.position).ToList();
        List<Vector2> rewards = UnityEngine.Object.FindObjectsByType<RewardChest>(FindObjectsSortMode.None)
            .Select(chest => (Vector2)chest.transform.position).ToList();
        List<Vector2> squads = UnityEngine.Object.FindObjectsByType<EnemyCampGate>(FindObjectsSortMode.None)
            .Select(camp => (Vector2)camp.transform.position).ToList();
        List<Vector2> placed = new List<Vector2>();
        System.Random random = new System.Random(20261027);

        GameObject root = new GameObject(RootName);
        root.transform.SetParent(world.transform, false);
        GameObject bushRoot = new GameObject("Bushes (move or disable individually)");
        bushRoot.transform.SetParent(root.transform, false);
        GameObject stoneRoot = new GameObject("Stones (move or disable individually)");
        stoneRoot.transform.SetParent(root.transform, false);

        int bushCount = Place(230, bushes, "Bush", bushRoot.transform, 0.72f,
            random, placed, trees, rewards, squads, 1.35f);
        int stoneCount = Place(150, stones, "Stone", stoneRoot.transform, 0.58f,
            random, placed, trees, rewards, squads, 1.05f);

        Harmonize(root, forest, bushes, stones);
        AddGroundColliders(root);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"Placed {bushCount} user bushes and {stoneCount} user stones in Arena. Their ground colliders and positions are editable.");
    }

    [MenuItem("Tools/Before We Fight/Add Bush and Stone Colliders")]
    public static void AddCollidersToExisting()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlaying || scene.name != "Arena")
        {
            Debug.LogError("Stop Play mode and open Arena before adding map dressing colliders.");
            return;
        }

        GameObject root = GameObject.Find(RootName);
        if (root == null)
        {
            Debug.LogError("User bushes and stones were not found in Arena.");
            return;
        }

        int added = AddGroundColliders(root);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"Added {added} editable bush and stone colliders. Existing hand-adjusted colliders were kept.");
    }

    private static int AddGroundColliders(GameObject root)
    {
        int added = 0;
        foreach (SpriteRenderer renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
        {
            string spriteName = renderer.sprite != null ? renderer.sprite.name : string.Empty;
            bool bush = spriteName.StartsWith("Bush_", StringComparison.Ordinal);
            bool stone = spriteName.StartsWith("Stone_", StringComparison.Ordinal);
            if (!bush && !stone) continue;
            if (renderer.GetComponent<Collider2D>() != null) continue;

            BoxCollider2D collider = Undo.AddComponent<BoxCollider2D>(renderer.gameObject);
            collider.isTrigger = false;
            // The player's example on Bush 156 blocks only the lower part of the art.
            collider.offset = bush ? new Vector2(-0.01f, -0.3f) : new Vector2(0f, -0.28f);
            collider.size = bush ? new Vector2(0.5f, 0.1f) : new Vector2(0.7f, 0.28f);
            EditorUtility.SetDirty(collider);
            added++;
        }
        return added;
    }

    [MenuItem("Tools/Before We Fight/Harmonize User Map Dressing")]
    public static void HarmonizeExisting()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlaying || scene.name != "Arena")
        {
            Debug.LogError("Stop Play mode and open Arena before harmonizing map dressing.");
            return;
        }

        GameObject root = GameObject.Find(RootName);
        GameObject forest = GameObject.Find("Forest Dressing (editable)");
        if (root == null || forest == null)
        {
            Debug.LogError("User map dressing or forest was not found in Arena.");
            return;
        }

        Sprite[] bushes = ImportVariants("Bush", 12);
        Sprite[] stones = ImportVariants("Stone", 5);
        if (bushes.Length != 12 || stones.Length != 5) return;
        Harmonize(root, forest, bushes, stones);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Harmonized bushes, stones, and tree lighting direction. Their positions remain editable.");
    }

    private static void Harmonize(GameObject root, GameObject forest, Sprite[] bushes, Sprite[] stones)
    {
        // Use the shrubs and neutral rocks whose silhouettes suit the existing forest tree.
        // The colorful source variants remain available in Assets for manual placement.
        int[] bushChoices = { 0, 10 };
        Color32[] bushTints = {
            new Color32(255, 255, 255, 255),
            new Color32(223, 230, 206, 255)
        };
        int[] stoneChoices = { 0, 4 };
        Color32[] stoneTints = {
            new Color32(183, 184, 174, 255),
            new Color32(234, 230, 216, 255)
        };
        System.Random random = new System.Random(82371);
        Transform bushRoot = root.transform.Find("Bushes (move or disable individually)");
        Transform stoneRoot = root.transform.Find("Stones (move or disable individually)");
        if (bushRoot == null || stoneRoot == null) return;

        for (int i = 0; i < bushRoot.childCount; i++)
        {
            Transform item = bushRoot.GetChild(i);
            SpriteRenderer renderer = item.GetComponent<SpriteRenderer>();
            if (renderer == null) continue;
            int choice = random.Next(bushChoices.Length);
            Undo.RecordObject(renderer, "Harmonize bush sprite");
            Undo.RecordObject(item, "Harmonize bush size");
            renderer.sprite = bushes[bushChoices[choice]];
            renderer.color = bushTints[choice];
            renderer.flipX = false;
            item.localScale = Vector3.one * (0.56f + (float)random.NextDouble() * 0.12f);
            item.name = $"Bush {i + 1:000} - {renderer.sprite.name}";
        }
        for (int i = 0; i < stoneRoot.childCount; i++)
        {
            Transform item = stoneRoot.GetChild(i);
            SpriteRenderer renderer = item.GetComponent<SpriteRenderer>();
            if (renderer == null) continue;
            int choice = random.Next(stoneChoices.Length);
            Undo.RecordObject(renderer, "Harmonize stone sprite");
            Undo.RecordObject(item, "Harmonize stone size");
            renderer.sprite = stones[stoneChoices[choice]];
            renderer.color = stoneTints[choice];
            renderer.flipX = false;
            item.localScale = Vector3.one * (0.44f + (float)random.NextDouble() * 0.09f);
            item.name = $"Stone {i + 1:000} - {renderer.sprite.name}";
        }
        foreach (SpriteRenderer renderer in forest.GetComponentsInChildren<SpriteRenderer>(true))
        {
            Undo.RecordObject(renderer, "Align tree lighting direction");
            renderer.flipX = false;
        }
    }

    private static Sprite[] ImportVariants(string prefix, int count)
    {
        Sprite[] variants = new Sprite[count];
        for (int i = 0; i < count; i++)
        {
            string path = $"{ArtFolder}/{prefix}_{i + 1:00}.png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return Array.Empty<Sprite>();
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
            variants[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (variants[i] == null) return Array.Empty<Sprite>();
        }
        return variants;
    }

    private static int Place(int target, Sprite[] sprites, string prefix, Transform parent, float scale,
        System.Random random, List<Vector2> placed, List<Vector2> trees,
        List<Vector2> rewards, List<Vector2> squads, float spacing)
    {
        int count = 0;
        int attempts = 0;
        while (count < target && attempts < target * 120)
        {
            attempts++;
            Vector2 position = new Vector2(
                (float)(random.NextDouble() * 94.0 - 47.0),
                (float)(random.NextDouble() * 94.0 - 47.0));
            if (position.sqrMagnitude < 25f || NearAny(position, trees, 3.4f) ||
                NearAny(position, rewards, 3.2f) || NearAny(position, squads, 5.0f) ||
                NearAny(position, placed, spacing)) continue;

            Sprite sprite = sprites[random.Next(sprites.Length)];
            GameObject item = new GameObject($"{prefix} {count + 1:000} - {sprite.name}");
            item.transform.SetParent(parent, false);
            item.transform.localPosition = position;
            item.transform.localScale = new Vector3(scale, scale, 1f);
            SpriteRenderer renderer = item.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.flipX = random.Next(2) == 0;
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;
            renderer.sortingOrder = 0;
            placed.Add(position);
            count++;
        }
        return count;
    }

    private static bool NearAny(Vector2 point, List<Vector2> positions, float radius)
    {
        float squared = radius * radius;
        foreach (Vector2 position in positions)
            if ((point - position).sqrMagnitude < squared) return true;
        return false;
    }
}
