using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ReplacePlayerWithAshenWanderer
{
    private const string AnimationFolder = "Assets/Generated/Animations/AshenWanderer/";
    private const string ControllerPath = "Assets/Generated/Controllers/AshenWanderer.controller";
    private const string SpriteSheetPath = "Assets/Art/Imported/ashen_wanderer_sheet.png";

    [MenuItem("Tools/Before We Fight/Replace Current Player With Ashen Wanderer")]
    public static void Replace()
    {
        AnimationClip idle = Load<AnimationClip>(AnimationFolder + "AshenWanderer_Idle.anim");
        AnimationClip walk = Load<AnimationClip>(AnimationFolder + "AshenWanderer_Walk.anim");
        AnimationClip attack = Load<AnimationClip>(AnimationFolder + "AshenWanderer_Attack.anim");
        AnimationClip jump = Load<AnimationClip>(AnimationFolder + "AshenWanderer_Jump.anim");
        RuntimeAnimatorController controller = Load<RuntimeAnimatorController>(ControllerPath);
        Sprite firstFrame = LoadFirstSprite();

        string[] scenePaths =
        {
            "Assets/Scenes/Arena.unity",
            "Assets/Scenes/Game.unity",
            "Assets/Scenes/Intro.unity",
            "Assets/Scenes/Title.unity"
        };

        int updatedPlayers = 0;
        foreach (string scenePath in scenePaths)
        {
            if (!System.IO.File.Exists(scenePath)) continue;
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            TopDownPlayer[] players = UnityEngine.Object.FindObjectsByType<TopDownPlayer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (TopDownPlayer player in players)
            {
                SerializedObject serializedPlayer = new(player);
                serializedPlayer.FindProperty("idle").objectReferenceValue = idle;
                serializedPlayer.FindProperty("walk").objectReferenceValue = walk;
                serializedPlayer.FindProperty("hit").objectReferenceValue = attack;
                serializedPlayer.FindProperty("jump").objectReferenceValue = jump;
                serializedPlayer.ApplyModifiedPropertiesWithoutUndo();

                Animator animator = player.GetComponent<Animator>();
                if (animator == null) animator = player.gameObject.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;

                SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
                if (renderer == null) renderer = player.gameObject.AddComponent<SpriteRenderer>();
                renderer.sprite = firstFrame;
                renderer.color = Color.white;
                renderer.sortingOrder = 25;

                Animation legacyAnimation = player.GetComponent<Animation>();
                if (legacyAnimation != null) legacyAnimation.enabled = false;

                // The previous 30px hero used a scale of 4. The new frames have a larger
                // canvas and need this scale to stay close to the intended in-game size.
                player.transform.localScale = new Vector3(2.4f, 2.4f, 1f);
                player.gameObject.name = "Player - Ashen Wanderer";
                EditorUtility.SetDirty(player.gameObject);
                updatedPlayers++;
            }

            if (players.Length > 0) EditorSceneManager.SaveScene(scene);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Ashen Wanderer replaced the current player in {updatedPlayers} scene instance(s). Existing UI and gameplay references were preserved.");
    }

    private static T Load<T>(string path) where T : UnityEngine.Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null) throw new InvalidOperationException("Required Ashen Wanderer asset is missing: " + path);
        return asset;
    }

    private static Sprite LoadFirstSprite()
    {
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(SpriteSheetPath);
        foreach (UnityEngine.Object asset in assets)
            if (asset is Sprite sprite && sprite.name.EndsWith("_00", StringComparison.Ordinal)) return sprite;
        foreach (UnityEngine.Object asset in assets)
            if (asset is Sprite sprite) return sprite;
        throw new InvalidOperationException("No sprites were found in " + SpriteSheetPath);
    }
}
