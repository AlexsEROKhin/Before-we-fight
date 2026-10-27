using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Keeps reward objects, their enemy groups, and the lock condition editable in Arena.
public static class InstallCampRewards
{
    private struct Placement
    {
        public string Camp;
        public string Template;
        public string Name;

        public Placement(string camp, string template, string name)
        {
            Camp = camp;
            Template = template;
            Name = name;
        }
    }

    private static readonly Placement[] Placements =
    {
        new Placement("Squad 01 - 2 mages, 1 ring, Trapper, Io", "Rare Chest - abilities (editable)", "Rare Chest - abilities (editable)"),
        new Placement("Squad 02 - 2 mages, 1 ring, Trapper, Io", "Reward Chest (editable)", "Reward Chest (editable)"),
        new Placement("Squad 03 - 2 mages, 1 ring, Trapper, Io", "Power Altar - abilities (editable)", "Power Altar - abilities (editable)"),
        new Placement("Squad 04 - 2 mages, 1 ring, Trapper, Io", "Reward Chest (editable)", "Reward Chest - Squad 04 (editable)"),
        new Placement("Squad 05 - 2 mages, 1 ring, Trapper, Io", "Rare Chest - abilities (editable)", "Rare Chest - Squad 05 (editable)"),
        new Placement("Squad 06 - 2 mages, 1 ring, Trapper, Io", "Power Altar - abilities (editable)", "Power Altar - Squad 06 (editable)"),
        new Placement("Squad 07 - 2 mages, 1 ring, Trapper, Io", "Rare Chest - abilities (editable)", "Rare Chest - Squad 07 (editable)"),
        new Placement("Squad 08 - 2 mages, 1 ring, Trapper, Io", "Reward Chest (editable)", "Reward Chest - Squad 08 (editable)"),
        new Placement("Squad 09 - 2 mages, 1 ring, Trapper, Io", "Power Altar - abilities (editable)", "Power Altar - Squad 09 (editable)"),
        new Placement("Squad 10 - 2 mages, 1 ring, Trapper, Io", "Boss Chest - abilities (editable)", "Boss Chest - abilities (editable)")
    };

    [MenuItem("Tools/Before We Fight/Place Rewards in Enemy Camps")]
    public static void Install()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlaying || scene.name != "Arena")
        {
            Debug.LogError("Stop Play mode and open Arena before placing camp rewards.");
            return;
        }

        foreach (Placement placement in Placements)
        {
            if (GameObject.Find(placement.Camp) == null || GameObject.Find(placement.Template) == null)
            {
                Debug.LogError("Missing camp or reward template for " + placement.Camp);
                return;
            }
        }

        // A previous layout left an unused gate on the empty legacy camp.
        GameObject emptyLegacyCamp = GameObject.Find("Skycaller Camp");
        if (emptyLegacyCamp != null && emptyLegacyCamp.GetComponentsInChildren<EnemyHitFeedback>(true).Length == 0)
        {
            EnemyCampGate unusedGate = emptyLegacyCamp.GetComponent<EnemyCampGate>();
            if (unusedGate != null) Object.DestroyImmediate(unusedGate);
        }

        foreach (Placement placement in Placements)
        {
            GameObject camp = GameObject.Find(placement.Camp);
            EnemyCampGate gate = camp.GetComponent<EnemyCampGate>();
            if (gate == null) gate = camp.AddComponent<EnemyCampGate>();
            EnemyHitFeedback[] members = camp.GetComponentsInChildren<EnemyHitFeedback>(true);
            if (members.Length == 0)
            {
                Debug.LogError("Camp has no enemies: " + placement.Camp);
                return;
            }
            SerializedObject gateObject = new SerializedObject(gate);
            SerializedProperty enemies = gateObject.FindProperty("enemies");
            enemies.arraySize = members.Length;
            for (int i = 0; i < members.Length; i++)
                enemies.GetArrayElementAtIndex(i).objectReferenceValue = members[i];
            gateObject.ApplyModifiedPropertiesWithoutUndo();

            GameObject reward = GameObject.Find(placement.Name);
            if (reward == null)
            {
                reward = Object.Instantiate(GameObject.Find(placement.Template));
                reward.name = placement.Name;
            }
            reward.transform.SetParent(camp.transform, false);
            reward.transform.localPosition = new Vector3(0f, -3.1f, 0f);
            reward.transform.localRotation = Quaternion.identity;
            RewardChest chest = reward.GetComponent<RewardChest>();
            if (chest == null) { Debug.LogError("RewardChest component missing on " + reward.name); return; }
            SerializedObject chestObject = new SerializedObject(chest);
            chestObject.FindProperty("enemyCamp").objectReferenceValue = gate;
            chestObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(camp);
            EditorUtility.SetDirty(gate);
            EditorUtility.SetDirty(reward);
            EditorUtility.SetDirty(chest);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Placed 10 squad rewards: 3 common chests, 3 rare chests, 3 altars, and 1 boss chest. Each unlocks when its own squad is defeated.");
    }
}
