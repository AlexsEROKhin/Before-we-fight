using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class KnightSizeAdjuster
{
    [MenuItem("Tools/Before We Fight/Make Knight Slightly Smaller")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Arena.unity", OpenSceneMode.Single);
        TopDownPlayer player = UnityEngine.Object.FindFirstObjectByType<TopDownPlayer>(FindObjectsInactive.Include);
        if (player == null) throw new InvalidOperationException("Player was not found in Arena.");
        player.transform.localScale = new Vector3(3.4f, 3.4f, 3.4f);
        EditorUtility.SetDirty(player.transform);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Knight scale changed from 4.0 to 3.4.");
    }
}
