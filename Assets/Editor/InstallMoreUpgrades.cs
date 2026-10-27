using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Adds editable reward entries to the existing profiles and Arena HUD.
public static class InstallMoreUpgrades
{
    private struct Entry
    {
        public ChestUpgradeKind Kind;
        public string File;
        public string Title;
        public string Description;

        public Entry(ChestUpgradeKind kind, string file, string title, string description)
        {
            Kind = kind;
            File = file;
            Title = title;
            Description = description;
        }
    }

    private static readonly Entry[] Entries =
    {
        new Entry(ChestUpgradeKind.SteelRhythm, "steel_rhythm.png", "STEEL RHYTHM", "Sword attacks 10% faster per level"),
        new Entry(ChestUpgradeKind.IronSkin, "iron_skin.png", "IRON SKIN", "Take 10% less damage per level"),
        new Entry(ChestUpgradeKind.Momentum, "momentum.png", "MOMENTUM", "Move 15% faster for 3s after a kill"),
        new Entry(ChestUpgradeKind.SpinMastery, "spin_mastery.png", "SPIN MASTERY", "Q gains reach and 1s less cooldown")
    };

    [MenuItem("Tools/Before We Fight/Install More Upgrades")]
    public static void Install()
    {
        if (EditorApplication.isPlaying || SceneManager.GetActiveScene().name != "Arena")
        {
            Debug.LogError("Stop Play mode and open Arena before installing upgrades.");
            return;
        }

        foreach (Entry entry in Entries)
        {
            string path = "Assets/Art/Abilities/" + entry.File;
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) { Debug.LogError("Missing icon: " + path); return; }
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        string[] profiles = { "AbilityRewards", "AltarRewards", "BossRewards" };
        foreach (string name in profiles)
        {
            string path = "Assets/Art/Abilities/" + name + ".asset";
            ChestRewardProfile profile = AssetDatabase.LoadAssetAtPath<ChestRewardProfile>(path);
            if (profile == null) { Debug.LogError("Missing profile: " + path); return; }
            SerializedObject serialized = new SerializedObject(profile);
            SerializedProperty choices = serialized.FindProperty("choices");
            foreach (Entry entry in Entries)
            {
                bool exists = false;
                for (int i = 0; i < choices.arraySize; i++)
                    if (choices.GetArrayElementAtIndex(i).FindPropertyRelative("kind").enumValueIndex == (int)entry.Kind)
                        exists = true;
                if (exists) continue;
                int index = choices.arraySize;
                choices.InsertArrayElementAtIndex(index);
                SerializedProperty choice = choices.GetArrayElementAtIndex(index);
                choice.FindPropertyRelative("kind").enumValueIndex = (int)entry.Kind;
                choice.FindPropertyRelative("cardSprite").objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Abilities/" + entry.File);
                choice.FindPropertyRelative("displayName").stringValue = entry.Title;
                choice.FindPropertyRelative("description").stringValue = entry.Description;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        UpgradeBarUI bar = Object.FindFirstObjectByType<UpgradeBarUI>(FindObjectsInactive.Include);
        if (bar == null) { Debug.LogError("Arena Upgrade Bar was not found."); return; }
        SerializedObject barObject = new SerializedObject(bar);
        SerializedProperty art = barObject.FindProperty("art");
        foreach (Entry entry in Entries)
        {
            bool exists = false;
            for (int i = 0; i < art.arraySize; i++)
                if (art.GetArrayElementAtIndex(i).FindPropertyRelative("kind").enumValueIndex == (int)entry.Kind)
                    exists = true;
            if (exists) continue;
            int index = art.arraySize;
            art.InsertArrayElementAtIndex(index);
            SerializedProperty item = art.GetArrayElementAtIndex(index);
            item.FindPropertyRelative("kind").enumValueIndex = (int)entry.Kind;
            item.FindPropertyRelative("icon").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Abilities/" + entry.File);
            item.FindPropertyRelative("title").stringValue = entry.Title;
            item.FindPropertyRelative("description").stringValue = entry.Description;
        }
        barObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(bar);

        Transform q = bar.transform.Find("Active Skill Q");
        if (q != null && q.TryGetComponent(out UpgradeSlotHover hover))
        {
            hover.ConfigureSkill(bar, "SWORD SPIN  Q", "Sword sweep. Spin Mastery adds reach and cuts cooldown.");
            EditorUtility.SetDirty(hover);
        }
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log("Installed four upgrades in editable profiles and Arena HUD.");
    }
}
