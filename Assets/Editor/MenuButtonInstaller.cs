using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MenuButtonInstaller
{
    private const string TexturePath = "Assets/Art/Imported/menu.png";
    private const string FrameSetPath = "Assets/Generated/UserUpdates/MenuFrames.asset";

    [MenuItem("Tools/Before We Fight/Install Aseprite MENU Button")]
    public static void Install()
    {
        Sprite[] frames = ImportFrames();
        SpriteFrameSet set = CreateOrUpdateFrameSet(frames);

        InstallInScene("Assets/Scenes/Arena.unity", frames[0], set);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Aseprite MENU button installed in Arena: click animation plays once before loading Title.");
    }

    private static Sprite[] ImportFrames()
    {
        AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceSynchronousImport);
        TextureImporter importer = AssetImporter.GetAtPath(TexturePath) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("Missing texture: " + TexturePath);

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 64f;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.isReadable = true;
        importer.maxTextureSize = 2048;

        SpriteMetaData[] slices = new SpriteMetaData[5];
        for (int i = 0; i < slices.Length; i++)
        {
            slices[i] = new SpriteMetaData
            {
                name = $"Menu_{i:000}",
                rect = new Rect(i * 128, 0, 128, 64),
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f)
            };
        }

#pragma warning disable CS0618
        importer.spritesheet = slices;
#pragma warning restore CS0618
        importer.SaveAndReimport();

        Sprite[] frames = AssetDatabase.LoadAllAssetsAtPath(TexturePath)
            .OfType<Sprite>()
            .OrderBy(sprite => sprite.name, StringComparer.Ordinal)
            .ToArray();
        if (frames.Length != 5) throw new InvalidOperationException("Expected 5 MENU frames, found " + frames.Length);
        return frames;
    }

    private static SpriteFrameSet CreateOrUpdateFrameSet(Sprite[] frames)
    {
        SpriteFrameSet set = AssetDatabase.LoadAssetAtPath<SpriteFrameSet>(FrameSetPath);
        if (set == null)
        {
            set = ScriptableObject.CreateInstance<SpriteFrameSet>();
            AssetDatabase.CreateAsset(set, FrameSetPath);
        }
        set.SetFrames(frames);
        EditorUtility.SetDirty(set);
        return set;
    }

    private static void InstallInScene(string scenePath, Sprite firstFrame, SpriteFrameSet set)
    {
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        GameObject buttonObject = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .Select(transform => transform.gameObject)
            .FirstOrDefault(go => go.name == "MENU Button");
        if (buttonObject == null) throw new InvalidOperationException("MENU Button not found in " + scenePath);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(154f, 62f);
        rect.anchoredPosition = new Vector2(-95f, 47f);

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = firstFrame;
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = true;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.93f, 0.78f, 1f);
        colors.pressedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
        colors.disabledColor = Color.white;
        button.colors = colors;

        foreach (Text label in buttonObject.GetComponentsInChildren<Text>(true))
            UnityEngine.Object.DestroyImmediate(label.gameObject);

        UIImageFrameAnimator animator = buttonObject.GetComponent<UIImageFrameAnimator>();
        if (animator == null) animator = buttonObject.AddComponent<UIImageFrameAnimator>();
        SerializedObject serialized = new SerializedObject(animator);
        serialized.FindProperty("frameSet").objectReferenceValue = set;
        serialized.FindProperty("targetImage").objectReferenceValue = image;
        serialized.FindProperty("framesPerSecond").floatValue = 12.5f;
        serialized.FindProperty("playOnEnable").boolValue = false;
        serialized.FindProperty("pauseWhilePointerOver").boolValue = false;
        serialized.FindProperty("hoverAlphaThreshold").floatValue = 0.1f;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        DelayedSceneButton sceneButton = buttonObject.GetComponent<DelayedSceneButton>();
        if (sceneButton == null) throw new InvalidOperationException("MENU Button has no DelayedSceneButton in " + scenePath);
        SerializedObject serializedSceneButton = new SerializedObject(sceneButton);
        serializedSceneButton.FindProperty("animateBeforeLoading").boolValue = true;
        serializedSceneButton.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(buttonObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
