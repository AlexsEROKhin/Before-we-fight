using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class KnightDirectionalSpriteInstaller
{
    private const string SourcePath = "Assets/Art/Source/Knight/Directional_Manual";
    private const string ClipPath = "Assets/Generated/Animations/Knight";
    private const string ArenaPath = "Assets/Scenes/Arena.unity";

    [MenuItem("Tools/Before We Fight/Install Knight Up Down Animations")]
    public static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play Mode before installing the knight animations.");

        Scene scene = SceneManager.GetSceneByPath(ArenaPath);
        if (!scene.IsValid() || !scene.isLoaded)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            scene = EditorSceneManager.OpenScene(ArenaPath, OpenSceneMode.Single);
        }
        TopDownPlayer player = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<TopDownPlayer>(true))
            .FirstOrDefault();
        if (player == null) throw new InvalidOperationException("Player was not found in Arena.");
        Animator animator = player.GetComponent<Animator>();
        AnimatorController controller = animator != null ? animator.runtimeAnimatorController as AnimatorController : null;
        if (controller == null || controller.layers.Length == 0)
            throw new InvalidOperationException("The Arena player needs an editable Animator Controller.");

        AssetDatabase.Refresh();
        ImportSprites();
        Sprite[] down = LoadFrames("down");
        Sprite[] up = LoadFrames("up");
        if (down.Length != 8 || up.Length != 8 || down.Any(frame => frame == null) || up.Any(frame => frame == null))
            throw new InvalidOperationException($"Expected eight frames per direction, got down={down.Length}, up={up.Length}.");

        AnimationClip idleDown = CreateClip("Knight_Idle_Down", new[] { LoadIdle("down") }, 1f, true);
        AnimationClip walkDown = CreateClip("Knight_Walk_Down", down, 8f, true);
        AnimationClip idleUp = CreateClip("Knight_Idle_Up", new[] { LoadIdle("up") }, 1f, true);
        AnimationClip walkUp = CreateClip("Knight_Walk_Up", up, 8f, true);

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        foreach (AnimationClip clip in new[] { idleDown, walkDown, idleUp, walkUp })
        {
            AnimatorState state = machine.states.Select(child => child.state).FirstOrDefault(candidate => candidate.name == clip.name);
            if (state == null) state = machine.AddState(clip.name);
            Undo.RecordObject(state, "Assign knight directional animation");
            state.motion = clip;
            EditorUtility.SetDirty(state);
        }
        EditorUtility.SetDirty(machine);
        EditorUtility.SetDirty(controller);

        SerializedObject serialized = new(player);
        serialized.FindProperty("idleDown").objectReferenceValue = idleDown;
        serialized.FindProperty("walkDown").objectReferenceValue = walkDown;
        serialized.FindProperty("idleUp").objectReferenceValue = idleUp;
        serialized.FindProperty("walkUp").objectReferenceValue = walkUp;
        serialized.ApplyModifiedProperties();
        EditorUtility.SetDirty(player);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Knight up/down facing installed: two idle clips and two 8-frame walking loops.");
    }

    private static void ImportSprites()
    {
        string[] paths = new[] { "down", "up" }.SelectMany(direction =>
            new[] { $"{SourcePath}/idle_{direction}.png" }.Concat(
                Enumerable.Range(1, 8).Select(index => $"{SourcePath}/walk_{direction}_{index:00}.png"))).ToArray();
        foreach (string path in paths)
        {
            if (!File.Exists(path)) throw new InvalidOperationException($"Missing source frame: {path}");
        }
        foreach (string path in paths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidOperationException($"Could not import source frame: {path}");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 80f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
    }

    private static Sprite[] LoadFrames(string direction)
    {
        return Enumerable.Range(1, 8)
            .Select(index => AssetDatabase.LoadAssetAtPath<Sprite>($"{SourcePath}/walk_{direction}_{index:00}.png"))
            .ToArray();
    }

    private static Sprite LoadIdle(string direction)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>($"{SourcePath}/idle_{direction}.png");
    }

    private static AnimationClip CreateClip(string name, Sprite[] frames, float fps, bool loop)
    {
        if (frames.Any(frame => frame == null)) throw new InvalidOperationException($"Missing source frame for {name}.");
        string assetPath = $"{ClipPath}/{name}.anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
        bool isNew = clip == null;
        if (isNew) clip = new AnimationClip();
        else Undo.RecordObject(clip, "Update knight directional animation");
        clip.name = name;
        clip.frameRate = fps;
        ObjectReferenceKeyframe[] keys = frames.Select((frame, index) => new ObjectReferenceKeyframe
        {
            time = index / fps,
            value = frame
        }).ToArray();
        if (loop)
            keys = keys.Concat(new[] { new ObjectReferenceKeyframe { time = frames.Length / fps, value = frames[0] } }).ToArray();
        EditorCurveBinding binding = new()
        {
            path = "PlayerVisual",
            type = typeof(SpriteRenderer),
            propertyName = "m_Sprite"
        };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        if (isNew) AssetDatabase.CreateAsset(clip, assetPath);
        else EditorUtility.SetDirty(clip);
        return clip;
    }
}
