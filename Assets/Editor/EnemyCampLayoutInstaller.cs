using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EnemyCampLayoutInstaller
{
    private sealed class Camp
    {
        public string name,prefab;
        public Vector2 position;
        public Camp(string name,string prefab,Vector2 position){this.name=name;this.prefab=prefab;this.position=position;}
    }

    [MenuItem("Tools/Before We Fight/Arrange Enemy Camps")]
    public static void Install()
    {
        Scene scene=EditorSceneManager.OpenScene("Assets/Scenes/Arena.unity",OpenSceneMode.Single);
        GameObject oldCamp=GameObject.Find("Enemy Camps");
        if(oldCamp!=null)
        {
            // Reward objects are edited in the scene. Keep them when camps are rearranged.
            foreach(RewardChest reward in oldCamp.GetComponentsInChildren<RewardChest>(true))
                reward.transform.SetParent(null,true);
            UnityEngine.Object.DestroyImmediate(oldCamp);
        }
        foreach(GameObject root in scene.GetRootGameObjects().Where(IsManagedEnemy).ToArray())UnityEngine.Object.DestroyImmediate(root);

        Camp[] camps={
            new("Skycaller","MageCreep",new Vector2(-32,26)),
            new("Hookwarden","Hookwarden",new Vector2(-16,26)),
            new("Whirlraider","Whirlraider",new Vector2(0,26)),
            new("Stonecleaver","Stonecleaver",new Vector2(16,26)),
            new("Blinkshade","Blinkshade",new Vector2(32,26)),
            new("Frostbinder","Frostbinder",new Vector2(-32,-26)),
            new("Shadowrazer","Shadowrazer",new Vector2(-16,-26)),
            new("Dreadmarksman","Dreadmarksman",new Vector2(0,-26)),
            new("Warcry Brute","WarcryBrute",new Vector2(16,-26)),
            new("DeepTide Colossus","DeepTideColossus",new Vector2(32,-26))
        };
        Vector2[] offsets={new Vector2(-1.25f,-.55f),new Vector2(1.25f,-.55f),new Vector2(0,1.15f)};
        GameObject container=new("Enemy Camps");SceneManager.MoveGameObjectToScene(container,scene);
        foreach(Camp camp in camps)
        {
            GameObject prefab=AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/{camp.prefab}.prefab");
            if(prefab==null)throw new InvalidOperationException($"Missing prefab: {camp.prefab}");
            GameObject campObject=new($"{camp.name} Camp");campObject.transform.SetParent(container.transform,false);campObject.transform.position=camp.position;
            for(int i=0;i<3;i++)
            {
                GameObject enemy=(GameObject)PrefabUtility.InstantiatePrefab(prefab,campObject.transform);
                enemy.name=$"{camp.name} {i+1:00}";enemy.transform.localPosition=offsets[i];
            }
        }
        EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        InstallCampRewards.Install();
        Debug.Log("Enemy camps arranged: 10 camps, 3 enemies in each.");
    }

    private static bool IsManagedEnemy(GameObject go)=>go.GetComponent<MageCreep>()!=null||go.GetComponent<HookwardenEnemy>()!=null||go.GetComponent<WhirlraiderEnemy>()!=null||go.GetComponent<StonecleaverEnemy>()!=null||go.GetComponent<BlinkshadeEnemy>()!=null||go.GetComponent<FrostbinderEnemy>()!=null||go.GetComponent<ShadowrazerEnemy>()!=null||go.GetComponent<DreadmarksmanEnemy>()!=null||go.GetComponent<WarcryBruteEnemy>()!=null||go.GetComponent<DeepTideColossusEnemy>()!=null;
}
