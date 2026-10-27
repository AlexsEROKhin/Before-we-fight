using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class KnightCharacterInstaller
{
    private const string SourceRoot = "Assets/Art/Source/Knight/frames";
    private const string ClipRoot = "Assets/Generated/Animations/Knight";
    private const string ControllerPath = "Assets/Generated/Controllers/Knight.controller";

    [MenuItem("Tools/Before We Fight/Install Knight Character")]
    public static void Install()
    {
        EnsureFolder("Assets/Generated/Animations", "Knight");
        ImportTextures();

        Dictionary<string, AnimationClip> clips = new();
        clips["Idle"] = MakeClip("Knight_Idle", "idle", 9f, true);
        clips["Run"] = MakeClip("Knight_Run", "run", 8f, true);
        clips["Stop"] = MakeClip("Knight_Stop", "stop", 10f, false);
        clips["Jump"] = MakeClip("Knight_Jump", "jump", 11f, false);
        clips["Land"] = MakeClip("Knight_Land", "land", 10f, false);
        clips["Attack"] = MakeClip("Knight_Attack", "attack1_VFX", 15f, false);
        clips["AirAttack"] = MakeClip("Knight_AirAttack", "attack1_jump_VFX", 15f, false);
        clips["Dash"] = MakeClip("Knight_Dash", "dash", 15f, false);
        clips["Hurt1"] = MakeClip("Knight_Hurt1", "hurt1", 12f, false);
        clips["Hurt2"] = MakeClip("Knight_Hurt2", "hurt2", 12f, false);
        clips["Death"] = MakeClip("Knight_Death", "death", 10f, false);

        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (controller != null) AssetDatabase.DeleteAsset(ControllerPath);
        controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        foreach (ChildAnimatorState state in machine.states) machine.RemoveState(state.state);
        foreach (AnimationClip clip in clips.Values)
        {
            AnimatorState state = machine.AddState(clip.name);
            state.motion = clip;
            if (clip == clips["Idle"]) machine.defaultState = state;
        }

        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Arena.unity", OpenSceneMode.Single);
        TopDownPlayer player = UnityEngine.Object.FindFirstObjectByType<TopDownPlayer>(FindObjectsInactive.Include);
        if (player == null) throw new InvalidOperationException("Player was not found in Arena.");
        SerializedObject serializedPlayer = new(player);
        serializedPlayer.FindProperty("idle").objectReferenceValue = clips["Idle"];
        serializedPlayer.FindProperty("walk").objectReferenceValue = clips["Run"];
        serializedPlayer.FindProperty("hit").objectReferenceValue = clips["Attack"];
        serializedPlayer.FindProperty("jump").objectReferenceValue = clips["Jump"];
        serializedPlayer.FindProperty("stop").objectReferenceValue = clips["Stop"];
        serializedPlayer.FindProperty("land").objectReferenceValue = clips["Land"];
        serializedPlayer.FindProperty("airAttack").objectReferenceValue = clips["AirAttack"];
        serializedPlayer.FindProperty("dash").objectReferenceValue = clips["Dash"];
        serializedPlayer.FindProperty("hurt1").objectReferenceValue = clips["Hurt1"];
        serializedPlayer.FindProperty("hurt2").objectReferenceValue = clips["Hurt2"];
        serializedPlayer.FindProperty("death").objectReferenceValue = clips["Death"];
        serializedPlayer.FindProperty("moveSpeed").floatValue = 3.35f;
        serializedPlayer.ApplyModifiedPropertiesWithoutUndo();

        player.GetComponent<Animator>().runtimeAnimatorController = controller;
        SpriteRenderer rootRenderer = player.GetComponent<SpriteRenderer>();
        Transform visual = player.transform.Find("PlayerVisual");
        if (visual == null)
        {
            GameObject visualObject = new("PlayerVisual");
            visual = visualObject.transform;
            visual.SetParent(player.transform, false);
        }
        visual.localPosition = Vector3.zero;
        visual.localRotation = Quaternion.identity;
        visual.localScale = Vector3.one;
        SpriteRenderer renderer = visual.GetComponent<SpriteRenderer>();
        if (renderer == null) renderer = visual.gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = LoadFrames("idle")[0];
        renderer.color = Color.white;
        renderer.sortingOrder = 25;
        if (rootRenderer != null) rootRenderer.enabled = false;
        player.transform.localScale = new Vector3(3.4f, 3.4f, 3.4f);
        player.gameObject.name = "Player - Knight";
        Animation legacy = player.GetComponent<Animation>();
        if (legacy != null) legacy.enabled = false;

        EditorUtility.SetDirty(player.gameObject);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Knight installed: 67 source frames, 11 animation clips, and Arena player replaced while preserving gameplay references.");
    }

    private static void ImportTextures()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { SourceRoot }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
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

    private static AnimationClip MakeClip(string assetName, string folder, float fps, bool loop)
    {
        Sprite[] frames = LoadFrames(folder);
        AnimationClip clip = new() { name = assetName, frameRate = fps };
        ObjectReferenceKeyframe[] keys = frames.Select((sprite, index) => new ObjectReferenceKeyframe
        {
            time = index / fps,
            value = sprite
        }).ToArray();
        EditorCurveBinding binding = new() { path = "PlayerVisual", type = typeof(SpriteRenderer), propertyName = "m_Sprite" };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        string path = $"{ClipRoot}/{assetName}.anim";
        AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (existing != null) AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(clip, path);
        return clip;
    }

    private static Sprite[] LoadFrames(string folder)
    {
        string path = $"{SourceRoot}/{folder}";
        return AssetDatabase.FindAssets("t:Sprite", new[] { path })
            .Select(AssetDatabase.GUIDToAssetPath)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .Select(AssetDatabase.LoadAssetAtPath<Sprite>)
            .Where(s => s != null)
            .ToArray();
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
    }
}
