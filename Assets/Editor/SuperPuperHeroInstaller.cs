using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Reuses the Arena player's existing clips and controller so scene references survive.
// The previous Aseprite source and its exported frames remain untouched.
public static class SuperPuperHeroInstaller
{
    private const string FrameRoot = "Assets/Art/Source/SuperPuperHero/Frames";
    private const string HitFrameRoot = "Assets/Art/Source/SuperPuperHero/UserHit";
    private const string DirectionalHitRoot = "Assets/Art/Source/SuperPuperHero/DirectionalHit";
    private const string ClipRoot = "Assets/Generated/Animations/NewHero";
    private const string ControllerPath = "Assets/Generated/Controllers/NewHero.controller";
    private const string ArenaPath = "Assets/Scenes/Arena.unity";
    private const int FrameCount = 42;

    [MenuItem("Tools/Before We Fight/Install Saved Diagonal Walks")]
    public static void InstallSavedDirections()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before installing the diagonal walks.");

        AssetDatabase.Refresh();
        for (int index = 34; index < FrameCount; index++)
        {
            string path = FramePath(index);
            if (!File.Exists(path)) throw new InvalidOperationException("Missing saved diagonal frame: " + path);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing sprite importer: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 128f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        Sprite[] downRightFrames = Enumerable.Range(34, 4)
            .Select(index => AssetDatabase.LoadAssetAtPath<Sprite>(FramePath(index))).ToArray();
        Sprite[] upRightFrames = Enumerable.Range(38, 4)
            .Select(index => AssetDatabase.LoadAssetAtPath<Sprite>(FramePath(index))).ToArray();
        if (downRightFrames.Any(frame => frame == null) || upRightFrames.Any(frame => frame == null))
            throw new InvalidOperationException("Unity could not import the saved diagonal sprites.");

        AnimationClip downRight = MakeClip("NewHero_Walk_DownRight", downRightFrames, true);
        AnimationClip upRight = MakeClip("NewHero_Walk_UpRight", upRightFrames, true);
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null) throw new InvalidOperationException("NewHero controller was not found.");
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        foreach (AnimationClip clip in new[] { downRight, upRight })
        {
            AnimatorState state = machine.states.Select(child => child.state)
                .FirstOrDefault(candidate => candidate.name == clip.name);
            if (state == null) state = machine.AddState(clip.name);
            state.motion = clip;
            EditorUtility.SetDirty(state);
        }
        EditorUtility.SetDirty(machine);
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("Saved user-drawn diagonal walks installed: right-down frames 34-37, right-up frames 38-41.");
    }

    [MenuItem("Tools/Before We Fight/Remove Hero Turns")]
    public static void RemoveTurns()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before removing the turn animations.");
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null) throw new InvalidOperationException("NewHero controller was not found.");
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        RemoveTurnStates(machine);
        EditorUtility.SetDirty(machine);
        EditorUtility.SetDirty(controller);
        Scene scene = SceneManager.GetSceneByPath(ArenaPath);
        if (!scene.IsValid() || !scene.isLoaded)
            throw new InvalidOperationException("Open Arena before removing the hero turns.");
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Hero turns removed from gameplay and Animator. Source Aseprite frames remain available.");
    }

    [MenuItem("Tools/Before We Fight/Install Super Puper Hero")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before changing the player sprite.");

        AssetDatabase.Refresh();
        if (Enumerable.Range(0, FrameCount).Any(index => !File.Exists(FramePath(index))))
            throw new InvalidOperationException("All 42 exported Super Puper Hero frames are required.");

        foreach (int index in Enumerable.Range(0, FrameCount))
        {
            string path = FramePath(index);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing sprite importer: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 128f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
        foreach (int index in Enumerable.Range(0, 5))
        {
            string path = $"{HitFrameRoot}/hit_{index}.png";
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing user-drawn hit sprite: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 128f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
        foreach (string direction in new[] { "up", "down" })
        foreach (int index in Enumerable.Range(0, 5))
        {
            string path = $"{DirectionalHitRoot}/hit_{direction}_{index}.png";
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing user-drawn directional hit sprite: " + path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 128f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        Sprite[] frames = Enumerable.Range(0, FrameCount)
            .Select(index => AssetDatabase.LoadAssetAtPath<Sprite>(FramePath(index))).ToArray();
        if (frames.Any(frame => frame == null))
            throw new InvalidOperationException("Unity could not load all 42 Super Puper Hero sprites.");
        Sprite[] hitFrames = Enumerable.Range(0, 5)
            .Select(index => AssetDatabase.LoadAssetAtPath<Sprite>($"{HitFrameRoot}/hit_{index}.png")).ToArray();
        if (hitFrames.Any(frame => frame == null))
            throw new InvalidOperationException("Unity could not load the five user-drawn hit sprites.");
        Sprite[] hitUpFrames = Enumerable.Range(0, 5)
            .Select(index => AssetDatabase.LoadAssetAtPath<Sprite>($"{DirectionalHitRoot}/hit_up_{index}.png")).ToArray();
        Sprite[] hitDownFrames = Enumerable.Range(0, 5)
            .Select(index => AssetDatabase.LoadAssetAtPath<Sprite>($"{DirectionalHitRoot}/hit_down_{index}.png")).ToArray();
        if (hitUpFrames.Any(frame => frame == null) || hitDownFrames.Any(frame => frame == null))
            throw new InvalidOperationException("Unity could not load the user-drawn up/down hit sprites.");

        // Zero-based frame ranges for the tags used in-game. Turn frames stay in
        // the Aseprite source, but are intentionally not played by the character.
        AnimationClip idle = MakeClip("NewHero_Idle", frames[0..5], true);
        AnimationClip walkSide = MakeClip("NewHero_Walk_Side", frames[9..15], true);
        AnimationClip walkDown = MakeClip("NewHero_Walk_Down", frames[19..24], true);
        AnimationClip walkUp = MakeClip("NewHero_Walk_Up", frames[24..28], true);
        AnimationClip walkDownRight = MakeClip("NewHero_Walk_DownRight", frames[34..38], true);
        AnimationClip walkUpRight = MakeClip("NewHero_Walk_UpRight", frames[38..42], true);
        AnimationClip jump = MakeClip("NewHero_Jump", frames[28..34], false);
        AnimationClip idleUp = MakeClip("NewHero_Idle_Up", new[] { frames[24] }, true);
        AnimationClip idleDown = MakeClip("NewHero_Idle_Down", new[] { frames[0] }, true);

        // Preserve the existing clip asset and controller state so Arena's hit
        // reference remains valid. The other actions still use placeholder poses.
        AnimationClip hit = MakeClip("NewHero_Hit_Placeholder", hitFrames, false, 100f / 7f);
        AnimationClip hitUp = MakeClip("NewHero_Hit_Up", hitUpFrames, false, 100f / 7f);
        AnimationClip hitDown = MakeClip("NewHero_Hit_Down", hitDownFrames, false, 100f / 7f);
        AnimationClip land = MakeHold("NewHero_Land_Placeholder", frames[0], 0.1f);
        AnimationClip airAttack = MakeHold("NewHero_AirAttack_Placeholder", frames[9], 0.2f);
        AnimationClip dash = MakeHold("NewHero_Dash_Placeholder", frames[9], 0.2f);
        AnimationClip hurt = MakeHold("NewHero_Hurt_Placeholder", frames[5], 0.2f);
        AnimationClip death = MakeHold("NewHero_Death_Placeholder", frames[0], 0.3f);

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null) throw new InvalidOperationException("NewHero controller was not found.");
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimationClip[] clips = { idle, walkSide, walkDown, walkUp, walkDownRight, walkUpRight,
            jump, idleUp, idleDown, hit, hitUp, hitDown, land, airAttack, dash, hurt, death };
        foreach (AnimationClip clip in clips)
        {
            AnimatorState state = machine.states.Select(child => child.state)
                .FirstOrDefault(candidate => candidate.name == clip.name);
            if (state == null) state = machine.AddState(clip.name);
            state.motion = clip;
            EditorUtility.SetDirty(state);
        }
        RemoveTurnStates(machine);
        machine.defaultState = machine.states.Select(child => child.state)
            .First(state => state.name == idle.name);
        EditorUtility.SetDirty(machine);
        EditorUtility.SetDirty(controller);

        Scene scene = SceneManager.GetSceneByPath(ArenaPath);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            if (!Application.isBatchMode)
                throw new InvalidOperationException("Open Arena before installing the new hero.");
            scene = EditorSceneManager.OpenScene(ArenaPath, OpenSceneMode.Single);
        }
        TopDownPlayer player = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<TopDownPlayer>(true)).FirstOrDefault();
        if (player == null) throw new InvalidOperationException("Arena player was not found.");
        Transform visual = player.transform.Find("PlayerVisual");
        if (visual == null) throw new InvalidOperationException("PlayerVisual was not found.");
        SpriteRenderer renderer = visual.GetComponent<SpriteRenderer>();
        if (renderer == null) throw new InvalidOperationException("PlayerVisual needs a SpriteRenderer.");

        renderer.sprite = frames[0];
        renderer.flipX = false;
        renderer.color = Color.white;
        // The new drawing is 32px tall versus 44px before. Match the old on-screen
        // height while keeping the player's movement and collider unchanged.
        visual.localScale = new Vector3(1.8f, 1.8f, 1f);
        Vector3 position = visual.localPosition;
        visual.localPosition = new Vector3(position.x, -0.066f, position.z);
        player.gameObject.name = "Player - Super Puper Hero";
        EditorUtility.SetDirty(player);
        EditorUtility.SetDirty(renderer);
        EditorUtility.SetDirty(visual);
        EditorUtility.SetDirty(player.gameObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Super Puper Hero installed: 42 frames including the saved diagonal walks; turn animations are kept only in Aseprite source.");
    }

    private static void RemoveTurnStates(AnimatorStateMachine machine)
    {
        foreach (AnimatorState state in machine.states.Select(child => child.state)
                     .Where(candidate => candidate.name == "NewHero_Turn" || candidate.name == "NewHero_Turn_Down")
                     .ToArray())
            machine.RemoveState(state);
    }

    private static string FramePath(int index) => $"{FrameRoot}/frame_{index}.png";

    private static AnimationClip MakeHold(string name, Sprite sprite, float duration)
    {
        AnimationClip clip = MakeClip(name, new[] { sprite, sprite }, false);
        AnimationUtility.SetObjectReferenceCurve(clip, SpriteBinding(), new[]
        {
            new ObjectReferenceKeyframe { time = 0f, value = sprite },
            new ObjectReferenceKeyframe { time = duration - 0.1f, value = sprite }
        });
        EditorUtility.SetDirty(clip);
        return clip;
    }

    private static AnimationClip MakeClip(string name, Sprite[] frames, bool loop, float frameRate = 10f)
    {
        string path = $"{ClipRoot}/{name}.anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        bool isNew = clip == null;
        if (isNew) clip = new AnimationClip();
        clip.name = name;
        clip.frameRate = frameRate;
        ObjectReferenceKeyframe[] keys = frames.Select((sprite, index) => new ObjectReferenceKeyframe
        {
            time = index / frameRate,
            value = sprite
        }).ToArray();
        AnimationUtility.SetObjectReferenceCurve(clip, SpriteBinding(), keys);
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        if (isNew) AssetDatabase.CreateAsset(clip, path);
        else EditorUtility.SetDirty(clip);
        return clip;
    }

    private static EditorCurveBinding SpriteBinding() => new EditorCurveBinding
    {
        path = "PlayerVisual",
        type = typeof(SpriteRenderer),
        propertyName = "m_Sprite"
    };
}
