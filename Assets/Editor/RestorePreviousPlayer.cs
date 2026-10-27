using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RestorePreviousPlayer
{
    [MenuItem("Tools/Before We Fight/Restore Previous Player")]
    public static void Restore()
    {
        const string scenePath = "Assets/Scenes/Arena.unity";
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        RuntimeAnimatorController controller = Load<RuntimeAnimatorController>("Assets/Generated/Animations/Player.controller");
        AnimationClip idle = Load<AnimationClip>("Assets/Generated/Animations/Player_Idle.anim");
        AnimationClip walk = Load<AnimationClip>("Assets/Generated/Animations/Player_Walk.anim");
        AnimationClip hit = Load<AnimationClip>("Assets/Generated/Animations/Player_Hit.anim");
        AnimationClip jump = Load<AnimationClip>("Assets/Generated/Animations/Player_Jump.anim");
        if (playerPrefab == null) throw new InvalidOperationException("Player prefab is missing.");

        Sprite originalSprite = playerPrefab.GetComponent<SpriteRenderer>().sprite;
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        TopDownPlayer player = UnityEngine.Object.FindFirstObjectByType<TopDownPlayer>(FindObjectsInactive.Include);
        if (player == null) throw new InvalidOperationException("Player was not found in Arena.");

        SerializedObject serializedPlayer = new(player);
        serializedPlayer.FindProperty("idle").objectReferenceValue = idle;
        serializedPlayer.FindProperty("walk").objectReferenceValue = walk;
        serializedPlayer.FindProperty("hit").objectReferenceValue = hit;
        serializedPlayer.FindProperty("jump").objectReferenceValue = jump;
        serializedPlayer.ApplyModifiedPropertiesWithoutUndo();

        SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
        renderer.sprite = originalSprite;
        renderer.color = Color.white;
        renderer.sortingOrder = 20;
        player.GetComponent<Animator>().runtimeAnimatorController = controller;
        player.transform.localScale = new Vector3(4f, 4f, 4f);
        player.gameObject.name = "Player (editable prefab instance)";

        EditorUtility.SetDirty(player.gameObject);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Previous player restored in Arena with its original animation set and scale.");
    }

    private static T Load<T>(string path) where T : UnityEngine.Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null) throw new InvalidOperationException("Required asset is missing: " + path);
        return asset;
    }
}
