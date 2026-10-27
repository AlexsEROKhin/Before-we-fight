using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ArenaEnemyRemover
{
    [MenuItem("Tools/Before We Fight/Remove All Enemies From Arena")]
    public static void RemoveAll()
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/Arena.unity", OpenSceneMode.Single);
        HashSet<GameObject> targets = new();
        Collect<SimpleEnemy>(targets);
        Collect<MageCreep>(targets);
        Collect<HookwardenEnemy>(targets);
        Collect<WhirlraiderEnemy>(targets);
        Collect<StonecleaverEnemy>(targets);
        Collect<BlinkshadeEnemy>(targets);
        Collect<FrostbinderEnemy>(targets);
        Collect<ShadowrazerEnemy>(targets);
        Collect<DreadmarksmanEnemy>(targets);
        Collect<WarcryBruteEnemy>(targets);
        Collect<DeepTideColossusEnemy>(targets);

        int count = targets.Count;
        foreach (GameObject target in targets) Object.DestroyImmediate(target);

        // Remove empty enemy group containers, but keep reusable prefabs and scripts in Assets.
        foreach (GameObject root in scene.GetRootGameObjects())
            RemoveEmptyEnemyGroups(root.transform);

        EditorSceneManager.SaveScene(scene);
        Debug.Log($"Removed {count} enemy instances from Arena. Enemy prefabs and scripts were preserved.");
    }

    private static void Collect<T>(HashSet<GameObject> targets) where T : Component
    {
        foreach (T component in Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            targets.Add(component.gameObject);
    }

    private static void RemoveEmptyEnemyGroups(Transform current)
    {
        for (int i = current.childCount - 1; i >= 0; i--) RemoveEmptyEnemyGroups(current.GetChild(i));
        if (current.childCount == 0 &&
            current.gameObject.name.ToLowerInvariant().Contains("enem") &&
            current.GetComponents<Component>().Length == 1)
            Object.DestroyImmediate(current.gameObject);
    }
}
