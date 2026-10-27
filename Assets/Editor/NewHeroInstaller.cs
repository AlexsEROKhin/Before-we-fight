using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Imports only frames from the player's own Aseprite document. The Arena objects,
// gameplay component, HUD, and camera remain editable in the existing scene.
public static class NewHeroInstaller
{
    private const string FrameRoot = "Assets/Art/Source/NewHero/Frames";
    private const string ClipRoot = "Assets/Generated/Animations/NewHero";
    private const string ControllerPath = "Assets/Generated/Controllers/NewHero.controller";
    private const string ArenaPath = "Assets/Scenes/Arena.unity";
    private const int FrameCount = 28;

    [MenuItem("Tools/Before We Fight/Update My Hero Jump")]
    public static void UpdateJump()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before updating the hero jump.");

        AssetDatabase.Refresh();
        if (Enumerable.Range(22, 6).Any(index => !File.Exists(FramePath(index))))
            throw new InvalidOperationException("The jump needs frames 22 through 27 from super new sprait.aseprite.");

        foreach (int index in Enumerable.Range(22, 6)) ConfigureFrame(index);
        Sprite[] jumpFrames = Enumerable.Range(22, 6).Select(LoadFrame).ToArray();
        if (jumpFrames.Any(frame => frame == null))
            throw new InvalidOperationException("Unity could not load the six jump sprites.");

        MigrateJumpClip();
        AnimationClip jump = MakeClip("NewHero_Jump", jumpFrames, 10, false);
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null) throw new InvalidOperationException("NewHero controller was not found.");
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState state = machine.states.Select(child => child.state)
            .FirstOrDefault(candidate => candidate.name == "NewHero_Jump" || candidate.name == "NewHero_Jump_Placeholder");
        if (state == null) state = machine.AddState("NewHero_Jump");
        state.name = jump.name;
        state.motion = jump;
        EditorUtility.SetDirty(state);
        EditorUtility.SetDirty(machine);
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("Updated NewHero jump from six user-drawn Aseprite frames. Existing Arena player reference is preserved.");
    }

    [MenuItem("Tools/Before We Fight/Install My New Hero")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before installing the new hero.");

        AssetDatabase.Refresh();
        if (Enumerable.Range(0, FrameCount).Any(index => !File.Exists(FramePath(index))))
            throw new InvalidOperationException("The new hero needs all 28 exported frames from super new sprait.aseprite.");

        foreach (int index in Enumerable.Range(0, FrameCount)) ConfigureFrame(index);
        Sprite[] frames = Enumerable.Range(0, FrameCount).Select(LoadFrame).ToArray();
        if (frames.Any(frame => frame == null))
            throw new InvalidOperationException("Unity could not load all 28 new hero sprites.");

        if (!AssetDatabase.IsValidFolder(ClipRoot))
            AssetDatabase.CreateFolder("Assets/Generated/Animations", "NewHero");

        // These ranges are the six tags in the user's Aseprite source (zero based).
        AnimationClip idle = MakeClip("NewHero_Idle", frames[0..5], 10, true);
        AnimationClip turn = MakeClip("NewHero_Turn", frames[5..8], 10, false);
        AnimationClip walkSide = MakeClip("NewHero_Walk_Side", frames[8..14], 10, true);
        AnimationClip walkUp = MakeClip("NewHero_Walk_Up", frames[14..18], 10, true);
        AnimationClip walkDown = MakeClip("NewHero_Walk_Down", frames[18..22], 10, true);
        AnimationClip idleUp = MakeClip("NewHero_Idle_Up", new[] { frames[14] }, 10, true);
        AnimationClip idleDown = MakeClip("NewHero_Idle_Down", new[] { frames[0] }, 10, true);

        // The Aseprite document has no separate combat, dash, hurt, or death
        // tags yet. Short holds of its own frames keep gameplay working without
        // showing an unrelated character. The clips can be edited in Unity later.
        AnimationClip hit = MakeHold("NewHero_Hit_Placeholder", frames[8], 0.2f);
        MigrateJumpClip();
        AnimationClip jump = MakeClip("NewHero_Jump", frames[22..28], 10, false);
        AnimationClip land = MakeHold("NewHero_Land_Placeholder", frames[0], 0.1f);
        AnimationClip airAttack = MakeHold("NewHero_AirAttack_Placeholder", frames[8], 0.2f);
        AnimationClip dash = MakeHold("NewHero_Dash_Placeholder", frames[8], 0.2f);
        AnimationClip hurt = MakeHold("NewHero_Hurt_Placeholder", frames[5], 0.2f);
        AnimationClip death = MakeHold("NewHero_Death_Placeholder", frames[0], 0.3f);

        AnimationClip[] clips = { idle, turn, walkSide, walkUp, walkDown, idleUp, idleDown,
            hit, jump, land, airAttack, dash, hurt, death };
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        foreach (AnimationClip clip in clips)
        {
            AnimatorState state = machine.states.Select(child => child.state)
                .FirstOrDefault(candidate => candidate.name == clip.name);
            if (state == null) state = machine.AddState(clip.name);
            state.motion = clip;
            EditorUtility.SetDirty(state);
            if (clip == idle) machine.defaultState = state;
        }
        EditorUtility.SetDirty(machine);
        EditorUtility.SetDirty(controller);

        Scene scene = SceneManager.GetSceneByPath(ArenaPath);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            scene = EditorSceneManager.OpenScene(ArenaPath, OpenSceneMode.Single);
        }
        TopDownPlayer player = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<TopDownPlayer>(true)).FirstOrDefault();
        if (player == null) throw new InvalidOperationException("Player was not found in Arena.");
        Transform visual = player.transform.Find("PlayerVisual");
        if (visual == null) throw new InvalidOperationException("PlayerVisual was not found in Arena.");
        SpriteRenderer renderer = visual.GetComponent<SpriteRenderer>();
        if (renderer == null) throw new InvalidOperationException("PlayerVisual needs SpriteRenderer.");
        Animator animator = player.GetComponent<Animator>();
        if (animator == null) throw new InvalidOperationException("Arena player needs Animator.");

        SerializedObject serialized = new(player);
        SetClip(serialized, "idle", idle);
        SetClip(serialized, "walk", walkSide);
        SetClip(serialized, "hit", hit);
        SetClip(serialized, "jump", jump);
        SetClip(serialized, "land", land);
        SetClip(serialized, "airAttack", airAttack);
        SetClip(serialized, "dash", dash);
        SetClip(serialized, "hurt1", hurt);
        SetClip(serialized, "hurt2", hurt);
        SetClip(serialized, "death", death);
        SetClip(serialized, "idleUp", idleUp);
        SetClip(serialized, "walkUp", walkUp);
        SetClip(serialized, "idleDown", idleDown);
        SetClip(serialized, "walkDown", walkDown);
        serialized.ApplyModifiedProperties();

        animator.runtimeAnimatorController = controller;
        SpriteRenderer rootRenderer = player.GetComponent<SpriteRenderer>();
        if (rootRenderer != null)
        {
            rootRenderer.sprite = null;
            rootRenderer.enabled = false;
            EditorUtility.SetDirty(rootRenderer);
        }
        renderer.sprite = frames[0];
        renderer.flipX = false;
        renderer.color = Color.white;
        // The figure is drawn above the centre of its 128px canvas.
        visual.localPosition = new Vector3(0f, -0.07f, 0f);
        visual.localScale = Vector3.one;
        player.transform.localScale = Vector3.one * 2.8f;
        player.gameObject.name = "Player - New Hero";
        EditorUtility.SetDirty(player);
        EditorUtility.SetDirty(animator);
        EditorUtility.SetDirty(renderer);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("New hero installed from super new sprait.aseprite: 28 source frames, six tagged animations, and same-character action placeholders.");
    }

    private static void ConfigureFrame(int index)
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

    private static void MigrateJumpClip()
    {
        string previous = $"{ClipRoot}/NewHero_Jump_Placeholder.anim";
        string current = $"{ClipRoot}/NewHero_Jump.anim";
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(current) != null ||
            AssetDatabase.LoadAssetAtPath<AnimationClip>(previous) == null) return;
        string error = AssetDatabase.MoveAsset(previous, current);
        if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
    }

    private static void SetClip(SerializedObject target, string name, AnimationClip clip)
    {
        SerializedProperty property = target.FindProperty(name);
        if (property == null) throw new InvalidOperationException("Missing player clip field: " + name);
        property.objectReferenceValue = clip;
    }

    private static string FramePath(int index) => $"{FrameRoot}/frame_{index}.png";
    private static Sprite LoadFrame(int index) => AssetDatabase.LoadAssetAtPath<Sprite>(FramePath(index));

    private static AnimationClip MakeHold(string name, Sprite sprite, float duration)
    {
        AnimationClip clip = MakeClip(name, new[] { sprite, sprite }, 10, false);
        EditorCurveBinding binding = SpriteBinding();
        AnimationUtility.SetObjectReferenceCurve(clip, binding, new[]
        {
            new ObjectReferenceKeyframe { time = 0f, value = sprite },
            new ObjectReferenceKeyframe { time = duration - 0.1f, value = sprite }
        });
        EditorUtility.SetDirty(clip);
        return clip;
    }

    private static AnimationClip MakeClip(string name, Sprite[] frames, float fps, bool loop)
    {
        string path = $"{ClipRoot}/{name}.anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        bool isNew = clip == null;
        if (isNew) clip = new AnimationClip();
        clip.name = name;
        clip.frameRate = fps;
        ObjectReferenceKeyframe[] keys = frames.Select((sprite, index) => new ObjectReferenceKeyframe
        {
            time = index / fps,
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
