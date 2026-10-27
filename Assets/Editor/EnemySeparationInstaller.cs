using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EnemySeparationInstaller
{
    private static readonly string[] Prefabs={"MageCreep","Hookwarden","Whirlraider","Stonecleaver","Blinkshade","Frostbinder","Shadowrazer","Dreadmarksman","WarcryBrute","DeepTideColossus"};
    [MenuItem("Tools/Before We Fight/Install Enemy Separation")]
    public static void Install()
    {
        foreach(string name in Prefabs)
        {
            string path=$"Assets/Prefabs/{name}.prefab";GameObject root=PrefabUtility.LoadPrefabContents(path);
            try
            {
                EnemySeparation separation=root.GetComponent<EnemySeparation>();
                if(separation==null)separation=root.AddComponent<EnemySeparation>();
                if(name=="DeepTideColossus"||name=="Stonecleaver")
                {
                    var so=new SerializedObject(separation);so.FindProperty("separationRadius").floatValue=.78f;so.ApplyModifiedPropertiesWithoutUndo();
                }
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }
            finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        Scene scene=EditorSceneManager.OpenScene("Assets/Scenes/Arena.unity",OpenSceneMode.Single);
        foreach(GameObject root in scene.GetRootGameObjects())InstallOnHierarchy(root);
        EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("Enemy separation installed.");
    }
    private static void InstallOnHierarchy(GameObject go)
    {
        if(IsEnemy(go)&&go.GetComponent<Rigidbody2D>()!=null&&go.GetComponent<EnemySeparation>()==null)go.AddComponent<EnemySeparation>();
        foreach(Transform child in go.transform)InstallOnHierarchy(child.gameObject);
    }
    private static bool IsEnemy(GameObject go)=>go.GetComponent<SimpleEnemy>()!=null||go.GetComponent<MageCreep>()!=null||go.GetComponent<HookwardenEnemy>()!=null||go.GetComponent<WhirlraiderEnemy>()!=null||go.GetComponent<StonecleaverEnemy>()!=null||go.GetComponent<BlinkshadeEnemy>()!=null||go.GetComponent<FrostbinderEnemy>()!=null||go.GetComponent<ShadowrazerEnemy>()!=null||go.GetComponent<DreadmarksmanEnemy>()!=null||go.GetComponent<WarcryBruteEnemy>()!=null||go.GetComponent<DeepTideColossusEnemy>()!=null;
}
