using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Updates only idle, movement and jump. Other Hueta 2 action clips are left alone.
public static class PlayerGifAnimationInstaller
{
    private const string FrameRoot = "Assets/Art/Source/PlayerGifUpdates/Frames";
    private const string ClipRoot = "Assets/Generated/Animations/NewHero/";
    private const string PrefabPath = "Assets/Prefabs/Player.prefab";
    private const string ArenaPath = "Assets/Scenes/Arena.unity";

    [MenuItem("Tools/Before We Fight/Install Idle Run Jump GIFs")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before updating the player GIFs.");

        AssetDatabase.Refresh();
        Sprite[] idle = Import("Idle32px", 10);
        Sprite[] run = Import("Run32px", 12);
        Sprite[] jump = Import("Jump32px", 5);

        SetClip("NewHero_Idle", idle, 12.5f, true);
        SetClip("NewHero_Idle_Up", idle, 12.5f, true);
        SetClip("NewHero_Idle_Down", idle, 12.5f, true);
        foreach (string name in new[] { "NewHero_Walk_Side", "NewHero_Walk_Up",
                     "NewHero_Walk_Down", "NewHero_Walk_UpRight", "NewHero_Walk_DownRight" })
            SetClip(name, run, 1000f / 70f, true);
        SetClip("NewHero_Jump", jump, 12.5f, false);

        // Preserve every transform and collider; update only the preview sprite.
        GameObject prefab = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            foreach (SpriteRenderer renderer in prefab.GetComponentsInChildren<SpriteRenderer>(true))
                if (renderer.gameObject.name != "Shadow") renderer.sprite = idle[0];
            PrefabUtility.SaveAsPrefabAsset(prefab, PrefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }

        var scene = EditorSceneManager.OpenScene(ArenaPath, OpenSceneMode.Single);
        var player = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<TopDownPlayer>(true)).FirstOrDefault();
        if (player == null) throw new InvalidOperationException("Arena player not found.");
        var visual = player.transform.Find("PlayerVisual");
        if (visual == null) throw new InvalidOperationException("PlayerVisual not found.");
        var rendererInScene = visual.GetComponent<SpriteRenderer>();
        if (rendererInScene == null) throw new InvalidOperationException("PlayerVisual SpriteRenderer not found.");
        rendererInScene.sprite = idle[0];
        EditorUtility.SetDirty(rendererInScene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Player GIFs installed: idle, movement, and jump updated; action clips and transforms unchanged.");
    }

    private static Sprite[] Import(string name, int count)
    {
        var sprites = new Sprite[count];
        for (int i = 0; i < count; i++)
        {
            string path = $"{FrameRoot}/{name}_{i}.png";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing GIF frame: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            // 23px drawn height at 72 PPU matches the existing 15px at 48 PPU.
            importer.spritePixelsPerUnit = 72f;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(0.5f, 0.43f);
            importer.SetTextureSettings(settings);
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprites[i] == null) throw new InvalidOperationException("Cannot load GIF frame: " + path);
        }
        return sprites;
    }

    private static void SetClip(string name, Sprite[] sprites, float fps, bool loop)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipRoot + name + ".anim");
        if (clip == null) throw new InvalidOperationException("Missing existing player clip: " + name);
        clip.frameRate = fps;
        var keys = sprites.Select((sprite, index) => new ObjectReferenceKeyframe
        {
            time = index / fps,
            value = sprite
        }).ToArray();
        AnimationUtility.SetObjectReferenceCurve(clip, new EditorCurveBinding
        {
            path = "PlayerVisual",
            type = typeof(SpriteRenderer),
            propertyName = "m_Sprite"
        }, keys);
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
    }
}
