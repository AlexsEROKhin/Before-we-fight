using UnityEditor;
using UnityEngine;

// Editor-only convenience commands. The arena still uses an ordinary editable Ground prefab.
public static class SetArenaGround
{
    private const string PrefabPath = "Assets/Prefabs/Ground.prefab";
    private const string GrassPath = "Assets/Art/Environment/forest_grass.png";
    private const string SoilPath = "Assets/Art/Environment/forest_floor.png";

    [MenuItem("Tools/Before We Fight/Use Green Grass Ground")]
    public static void UseGrass() => Apply(GrassPath);

    [MenuItem("Tools/Before We Fight/Use Brown Soil Ground")]
    public static void UseSoil() => Apply(SoilPath);

    private static void Apply(string imagePath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(imagePath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError("Ground image was not found: " + imagePath);
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 32;
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.wrapMode = TextureWrapMode.Repeat;
        importer.SaveAndReimport();

        Sprite groundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);
        if (groundSprite == null)
        {
            Debug.LogError("Ground image did not import as a sprite: " + imagePath);
            return;
        }

        GameObject prefab = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            SpriteRenderer renderer = prefab.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                Debug.LogError("Ground prefab has no Sprite Renderer.");
                return;
            }
            renderer.sprite = groundSprite;
            PrefabUtility.SaveAsPrefabAsset(prefab, PrefabPath);
            Debug.Log("Ground now uses " + groundSprite.name + ". Its size remains editable in Arena.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefab);
        }
    }
}
