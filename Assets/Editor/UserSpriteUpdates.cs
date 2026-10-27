using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Asset-only importer. It never creates or edits a scene and never enters Play Mode.
public static class UserSpriteUpdates
{
    private const string Imported = "Assets/Art/Imported/";
    private const string Generated = "Assets/Generated/UserUpdates/";

    [MenuItem("Tools/Before We Fight/Prepare User Sprite Updates")]
    private static void Prepare()
    {
        EnsureFolder("Assets/Generated", "UserUpdates");

        Sprite[] player = ImportStrip("player_updated", "PlayerUpdated", 30);
        Sprite[] tree = ImportStrip("tree", "Tree", 12);
        Sprite[] health = ImportStrip("health", "Health", 11);
        Sprite[] experience = ImportStrip("experience", "Experience", 9);
        Sprite[] play = ImportStrip("play", "Play", 5);

        MakeClip("Assets/Generated/Animations/Player_Idle.anim", player[0..6], true, 10f);
        MakeClip("Assets/Generated/Animations/Player_Hit.anim", player[6..14], false, 10f);
        MakeClip("Assets/Generated/Animations/Player_Walk.anim", player[14..21], true, 10f);
        MakeClip("Assets/Generated/Animations/Player_Jump.anim", player[21..30], false, 10f);

        MakeFrameSet("TreeFrames", tree);
        MakeFrameSet("HealthFrames", health);
        MakeFrameSet("ExperienceFrames", experience);
        MakeFrameSet("PlayFrames", play);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("User sprite updates prepared: player, tree, health, experience and play.");
    }

    [MenuItem("Tools/Before We Fight/Replace Player With New Sprite")]
    public static void ReplacePlayerWithNewSprite()
    {
        Sprite[] player = ImportStrip("new_player", "NewPlayer", 36);

        // Frame ranges come directly from the tags in new sprite.aseprite.
        MakeClip("Assets/Generated/Animations/Player_Idle.anim", player[0..6], true, 10f);
        MakeClip("Assets/Generated/Animations/Player_Walk.anim", player[6..14], true, 10f);
        MakeClip("Assets/Generated/Animations/Player_Hit.anim", player[14..21], false, 15f);
        MakeClip("Assets/Generated/Animations/Player_Jump.anim", player[21..30], false, 10f);
        MakeClip("Assets/Generated/Animations/Player_IdleWithArm.anim", player[30..36], true, 10f);

        string prefabPath = "Assets/Prefabs/Player.prefab";
        GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            SpriteRenderer renderer = prefab.GetComponent<SpriteRenderer>();
            if (renderer == null) throw new InvalidOperationException("Player prefab has no SpriteRenderer.");
            renderer.sprite = player[0];
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefab);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("New player installed: 36 frames, 5 editable animation clips, Point filtering, no compression.");
    }

    [MenuItem("Tools/Before We Fight/Replace Play Button Sprite")]
    public static void ReplacePlayButtonSprite()
    {
        EnsureFolder("Assets/Generated", "UserUpdates");
        Sprite[] play = ImportStrip("play", "Play", 5);
        MakeFrameSet("PlayFrames", play);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Play button sprite updated: 5 frames, Point filtering, no compression.");
    }

    [MenuItem("Tools/Before We Fight/Replace Experience Sprite")]
    public static void ReplaceExperienceSprite()
    {
        EnsureFolder("Assets/Generated", "UserUpdates");
        Sprite[] experience = ImportStrip("experience", "Experience", 9);
        MakeFrameSet("ExperienceFrames", experience);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Experience sprite updated: 9 frames, Point filtering, no compression.");
    }

    [MenuItem("Tools/Before We Fight/Animate Selected Play Button")]
    public static void AnimateSelectedPlayButton()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null || selected.GetComponent<UnityEngine.UI.Button>() == null)
            throw new InvalidOperationException("Select the PLAY Button object in the Title scene first.");

        Sprite[] playFramesArray = ImportStrip("play", "Play", 5);
        MakeFrameSet("PlayFrames", playFramesArray);

        UIImageFrameAnimator animator = selected.GetComponent<UIImageFrameAnimator>();
        if (animator == null) animator = Undo.AddComponent<UIImageFrameAnimator>(selected);

        SpriteFrameSet playFrames = AssetDatabase.LoadAssetAtPath<SpriteFrameSet>(Generated + "PlayFrames.asset");
        if (playFrames == null) throw new InvalidOperationException("Missing PlayFrames.asset. Replace the Play button sprite first.");

        SerializedObject serializedAnimator = new SerializedObject(animator);
        serializedAnimator.FindProperty("frameSet").objectReferenceValue = playFrames;
        serializedAnimator.FindProperty("framesPerSecond").floatValue = 10f;
        serializedAnimator.FindProperty("playOnEnable").boolValue = true;
        serializedAnimator.FindProperty("pauseWhilePointerOver").boolValue = true;
        RectTransform rectTransform = selected.GetComponent<RectTransform>();
        Undo.RecordObject(rectTransform, "Fit PLAY button bounds");
        rectTransform.sizeDelta = new Vector2(82f, 44f);

        Transform oldVisual = selected.transform.Find("Animated Visual");
        GameObject visualObject = oldVisual != null ? oldVisual.gameObject : new GameObject("Animated Visual", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        if (oldVisual == null)
        {
            Undo.RegisterCreatedObjectUndo(visualObject, "Create PLAY visual");
            visualObject.transform.SetParent(selected.transform, false);
        }

        RectTransform visualRect = visualObject.GetComponent<RectTransform>();
        visualRect.anchorMin = visualRect.anchorMax = visualRect.pivot = new Vector2(0.5f, 0.5f);
        visualRect.anchoredPosition = Vector2.zero;
        visualRect.sizeDelta = new Vector2(350f, 185f);
        visualRect.localScale = Vector3.one;

        UnityEngine.UI.Image visualImage = visualObject.GetComponent<UnityEngine.UI.Image>();
        visualImage.sprite = playFramesArray[0];
        visualImage.color = Color.white;
        visualImage.preserveAspect = true;
        visualImage.raycastTarget = false;

        UnityEngine.UI.Image hitImage = selected.GetComponent<UnityEngine.UI.Image>();
        hitImage.sprite = null;
        hitImage.color = new Color(1f, 1f, 1f, 0.001f);
        hitImage.preserveAspect = false;
        hitImage.raycastTarget = true;

        UnityEngine.UI.Button button = selected.GetComponent<UnityEngine.UI.Button>();
        button.targetGraphic = visualImage;

        serializedAnimator.FindProperty("targetImage").objectReferenceValue = visualImage;
        serializedAnimator.FindProperty("hoverAlphaThreshold").floatValue = 0f;
        serializedAnimator.ApplyModifiedProperties();

        EditorUtility.SetDirty(selected);
        AssetDatabase.SaveAssets();
        Debug.Log("Play button fixed: visible art preserved, hover/click RectTransform is 82x44.");
    }

    [MenuItem("Tools/Before We Fight/Prepare Custom Game Cursor")]
    public static void PrepareCustomGameCursor()
    {
        const string path = "Assets/Resources/UI/custom_cursor.png";
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("Missing cursor texture: " + path);

        importer.textureType = TextureImporterType.Cursor;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 64;
        importer.SaveAndReimport();

        Debug.Log("Custom pixel cursor prepared: 40x51, upper-left hotspot, Point filtering.");
    }

    private static Sprite[] ImportStrip(string file, string prefix, int count)
    {
        string path = Imported + file + ".png";
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null) throw new InvalidOperationException("Missing texture: " + path);

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 128f;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 8192;

        SpriteMetaData[] slices = new SpriteMetaData[count];
        for (int i = 0; i < count; i++)
        {
            slices[i] = new SpriteMetaData
            {
                name = $"{prefix}_{i:000}",
                rect = new Rect(i * 128, 0, 128, 128),
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f)
            };
        }

#pragma warning disable CS0618
        importer.spritesheet = slices;
#pragma warning restore CS0618
        importer.SaveAndReimport();

        return AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .OrderBy(sprite => sprite.name, StringComparer.Ordinal)
            .ToArray();
    }

    private static void MakeClip(string path, Sprite[] frames, bool loop, float fps)
    {
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, path);
        }

        clip.frameRate = fps;
        EditorCurveBinding binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = string.Empty,
            propertyName = "m_Sprite"
        };
        ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[frames.Length];
        for (int i = 0; i < frames.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i] };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
    }

    private static void MakeFrameSet(string name, Sprite[] frames)
    {
        string path = Generated + name + ".asset";
        SpriteFrameSet set = AssetDatabase.LoadAssetAtPath<SpriteFrameSet>(path);
        if (set == null)
        {
            set = ScriptableObject.CreateInstance<SpriteFrameSet>();
            AssetDatabase.CreateAsset(set, path);
        }
        set.SetFrames(frames);
        EditorUtility.SetDirty(set);
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
    }
}
