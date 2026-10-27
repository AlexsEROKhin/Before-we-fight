using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Import the player's hand-drawn Aseprite frames without replacing the existing
// animation assets, controller, scene objects, colliders, or transforms.
public static class Hueta2PlayerInstaller
{
    private const string Frames = "Assets/Art/Source/Hueta2/Frames";
    private const string Clips = "Assets/Generated/Animations/NewHero/";
    private const string Arena = "Assets/Scenes/Arena.unity";
    private const string PlayerPrefab = "Assets/Prefabs/Player.prefab";

    [MenuItem("Tools/Before We Fight/Install Hueta 2 Player")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before installing Hueta 2.");

        AssetDatabase.Refresh();
        var frames = new Sprite[17];
        for (int i = 0; i < frames.Length; i++)
        {
            string path = $"{Frames}/hueta2_{i}.png";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing Hueta 2 frame: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            // The new art is 15px tall rather than 43px. Keep approximately
            // the same on-screen height without changing scene transforms.
            importer.spritePixelsPerUnit = 48f;
            var textureSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(textureSettings);
            textureSettings.spriteAlignment = (int)SpriteAlignment.Custom;
            textureSettings.spritePivot = new Vector2(0.5f, 0.44f);
            importer.SetTextureSettings(textureSettings);
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
            frames[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (frames[i] == null) throw new InvalidOperationException("Cannot load Hueta 2 sprite: " + path);
        }

        Sprite[] idle = Slice(frames, 0, 5);
        Sprite[] walk = Slice(frames, 5, 5);
        Sprite[] dash = Slice(frames, 10, 3);
        Sprite[] jump = Slice(frames, 13, 4);
        SetClip("NewHero_Idle", idle, 10f, true);
        SetClip("NewHero_Walk_Side", walk, 10f, true);
        SetClip("NewHero_Jump", jump, 10f, false);
        // There is no hit tag in Hueta 2 yet. Use its action/dash drawings
        // temporarily, rather than mixing in the previous character.
        SetClip("NewHero_Hit_Placeholder", dash, 10f, false);

        // Hueta 2 currently has only a right-facing view. All directional and
        // secondary clips must still use the new character, not the old art.
        foreach (string name in new[] { "NewHero_Idle_Up", "NewHero_Idle_Down" })
            SetClip(name, new[] { idle[0] }, 10f, true);
        // These two clips are currently unused but remain editable assets.
        SetClip("NewHero_Turn", new[] { idle[0] }, 10f, false);
        SetClip("NewHero_Turn_Down", new[] { idle[0] }, 10f, false);
        foreach (string name in new[] { "NewHero_Walk_Up", "NewHero_Walk_Down",
                     "NewHero_Walk_UpRight", "NewHero_Walk_DownRight" })
            SetClip(name, walk, 10f, true);
        foreach (string name in new[] { "NewHero_Hit_Up", "NewHero_Hit_Down" })
            SetClip(name, dash, 10f, false);
        SetClip("NewHero_AirAttack_Placeholder", dash, 10f, false);
        SetClip("NewHero_Dash_Placeholder", dash, 10f, false);
        SetClip("NewHero_Land_Placeholder", new[] { idle[0] }, 10f, false);
        SetClip("NewHero_Hurt_Placeholder", new[] { idle[0], idle[0] }, 10f, false);
        SetClip("NewHero_Death_Placeholder", new[] { idle[0], idle[0], idle[0] }, 10f, false);

        // The prefab can be opened and edited normally in Unity afterwards.
        GameObject prefab = PrefabUtility.LoadPrefabContents(PlayerPrefab);
        try
        {
            foreach (SpriteRenderer renderer in prefab.GetComponentsInChildren<SpriteRenderer>(true))
                if (renderer.gameObject.name != "Shadow") renderer.sprite = idle[0];
            var legacy = prefab.GetComponent<Animation>();
            if (legacy != null) legacy.enabled = false;
            PrefabUtility.SaveAsPrefabAsset(prefab, PlayerPrefab);
        }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }

        var scene = EditorSceneManager.OpenScene(Arena, OpenSceneMode.Single);
        var player = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<TopDownPlayer>(true)).FirstOrDefault();
        if (player == null) throw new InvalidOperationException("Arena player not found.");
        var visual = player.transform.Find("PlayerVisual");
        if (visual == null) throw new InvalidOperationException("PlayerVisual not found.");
        var spriteRenderer = visual.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) throw new InvalidOperationException("PlayerVisual SpriteRenderer not found.");
        spriteRenderer.sprite = idle[0];
        player.gameObject.name = "Player - Hueta 2";
        EditorUtility.SetDirty(spriteRenderer);
        EditorUtility.SetDirty(player.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Hueta 2 installed: all player clips now use the new 17-frame character. Transforms and colliders unchanged.");
    }

    private static Sprite[] Slice(Sprite[] frames, int start, int count)
        => frames.Skip(start).Take(count).ToArray();

    private static void SetClip(string name, Sprite[] sprites, float fps, bool loop)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(Clips + name + ".anim");
        if (clip == null) throw new InvalidOperationException("Missing existing player clip: " + name);
        clip.frameRate = fps;
        var keys = sprites.Select((sprite, i) => new ObjectReferenceKeyframe
        {
            time = i / fps,
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
