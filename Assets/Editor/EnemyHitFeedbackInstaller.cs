using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EnemyHitFeedbackInstaller
{
    private static readonly string[] Prefabs={"MageCreep","Hookwarden","Whirlraider","Stonecleaver","Blinkshade","Frostbinder","Shadowrazer","Dreadmarksman","WarcryBrute","DeepTideColossus"};

    [MenuItem("Tools/Before We Fight/Install Enemy Hit Feedback")]
    public static void Install()
    {
        foreach(string name in Prefabs)
        {
            string path=$"Assets/Prefabs/{name}.prefab";
            GameObject root=PrefabUtility.LoadPrefabContents(path);
            try{if(root.GetComponent<EnemyHitFeedback>()==null)root.AddComponent<EnemyHitFeedback>();PrefabUtility.SaveAsPrefabAsset(root,path);}
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        Scene scene=EditorSceneManager.OpenScene("Assets/Scenes/Arena.unity",OpenSceneMode.Single);
        foreach(GameObject root in scene.GetRootGameObjects())InstallOnHierarchy(root);
        EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("Enemy hit feedback installed.");
    }

    private static void InstallOnHierarchy(GameObject go)
    {
        if(IsEnemy(go)&&go.GetComponent<EnemyHitFeedback>()==null)go.AddComponent<EnemyHitFeedback>();
        foreach(Transform child in go.transform)InstallOnHierarchy(child.gameObject);
    }

    private static bool IsEnemy(GameObject go)=>go.GetComponent<SimpleEnemy>()!=null||go.GetComponent<MageCreep>()!=null||go.GetComponent<HookwardenEnemy>()!=null||go.GetComponent<WhirlraiderEnemy>()!=null||go.GetComponent<StonecleaverEnemy>()!=null||go.GetComponent<BlinkshadeEnemy>()!=null||go.GetComponent<FrostbinderEnemy>()!=null||go.GetComponent<ShadowrazerEnemy>()!=null||go.GetComponent<DreadmarksmanEnemy>()!=null||go.GetComponent<WarcryBruteEnemy>()!=null||go.GetComponent<DeepTideColossusEnemy>()!=null;
}
