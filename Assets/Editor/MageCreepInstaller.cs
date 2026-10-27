using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MageCreepInstaller
{
    [MenuItem("Tools/Before We Fight/Install Mage Creeps")]
    public static void Install()
    {
        Sprite[] mage = ImportStrip("Assets/Art/Imported/Enemies/skycaller_animated.png", "Skycaller", 30, 32, 32);
        Sprite[] marker = ImportStrip("Assets/Art/Imported/mage_target_marker.png", "MageMarker", 4, 32, 16);
        Sprite[] beam = ImportStrip("Assets/Art/Imported/mage_sky_spell.png", "MageBeam", 3, 16, 64);
        GameObject prefab = BuildPrefab(mage, marker, beam);
        PlaceInArena(prefab);
        AssetDatabase.SaveAssets();
        Debug.Log("Mage creeps installed: idle detection, retreat at close range, stationary 2-second cast, warning marker and sky impact.");
    }

    private static Sprite[] ImportStrip(string path, string prefix, int count, int width, int height)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("Missing " + path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 32f;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        SpriteMetaData[] slices = new SpriteMetaData[count];
        for (int i = 0; i < count; i++)
            slices[i] = new SpriteMetaData { name = $"{prefix}_{i:000}", rect = new Rect(i * width, 0, width, height), alignment = (int)SpriteAlignment.Center, pivot = new Vector2(0.5f, 0.5f) };
#pragma warning disable CS0618
        importer.spritesheet = slices;
#pragma warning restore CS0618
        importer.SaveAndReimport();
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name, StringComparer.Ordinal).ToArray();
    }

    private static GameObject BuildPrefab(Sprite[] mage, Sprite[] marker, Sprite[] beam)
    {
        const string prefabPath = "Assets/Prefabs/MageCreep.prefab";
        GameObject root = new GameObject("Mage Creep", typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(MageCreep));
        SpriteRenderer renderer = root.GetComponent<SpriteRenderer>();
        renderer.sprite = mage[0];
        renderer.sortingOrder = 22;
        Rigidbody2D body = root.GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.linearDamping = 8f;
        body.freezeRotation = true;
        CircleCollider2D collider = root.GetComponent<CircleCollider2D>();
        collider.radius = 0.3f;
        collider.offset = new Vector2(0f, -0.05f);
        collider.isTrigger = true;

        MageCreep behaviour = root.GetComponent<MageCreep>();
        SerializedObject serialized = new SerializedObject(behaviour);
        SetSprites(serialized.FindProperty("idleFrames"), mage[0..4]);
        SetSprites(serialized.FindProperty("moveFrames"), mage[4..8]);
        SetSprites(serialized.FindProperty("castStartFrames"), mage[8..11]);
        SetSprites(serialized.FindProperty("castFrames"), mage[11..15]);
        SetSprites(serialized.FindProperty("castReleaseFrames"), mage[15..18]);
        SetSprites(serialized.FindProperty("recoveryFrames"), mage[18..21]);
        SetSprites(serialized.FindProperty("hurtFrames"), mage[21..23]);
        SetSprites(serialized.FindProperty("staggerFrames"), mage[23..25]);
        SetSprites(serialized.FindProperty("deathFrames"), mage[25..30]);
        SetSprites(serialized.FindProperty("markerFrames"), marker);
        SetSprites(serialized.FindProperty("beamFrames"), beam);
        serialized.FindProperty("warningSeconds").floatValue = 1f;
        serialized.FindProperty("cooldownSeconds").floatValue = 2.5f;
        serialized.FindProperty("damage").intValue = 2;
        serialized.FindProperty("hitsToKill").intValue = 3;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        UnityEngine.Object.DestroyImmediate(root);
        return prefab;
    }

    private static void SetSprites(SerializedProperty property, Sprite[] sprites)
    {
        property.arraySize = sprites.Length;
        for (int i = 0; i < sprites.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
    }

    private static void PlaceInArena(GameObject prefab)
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Arena.unity", OpenSceneMode.Single);
        foreach (GameObject root in scene.GetRootGameObjects().Where(go => go.name.StartsWith("Mage Creep ")).ToArray())
            UnityEngine.Object.DestroyImmediate(root);

        Vector2[] positions = { new Vector2(6f, 2.5f), new Vector2(-6.5f, -2.5f), new Vector2(2f, -6f) };
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            instance.name = $"Mage Creep {i + 1:00}";
            instance.transform.position = positions[i];
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
