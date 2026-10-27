using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ExperienceProgressInstaller
{
    [MenuItem("Tools/Before We Fight/Connect Experience Progress")]
    public static void Install()
    {
        Scene scene=EditorSceneManager.OpenScene("Assets/Scenes/Arena.unity",OpenSceneMode.Single);
        TopDownPlayer player=Object.FindFirstObjectByType<TopDownPlayer>();
        GameObject fill=GameObject.Find("Level Fill");
        SpriteFrameSet frames=AssetDatabase.LoadAssetAtPath<SpriteFrameSet>("Assets/Generated/UserUpdates/ExperienceFrames.asset");
        if(player==null||fill==null||frames==null)throw new System.InvalidOperationException("Player, Level Fill or ExperienceFrames is missing.");
        Image image=fill.GetComponent<Image>();
        if(image==null)throw new System.InvalidOperationException("Level Fill has no Image component.");
        var so=new SerializedObject(player);
        so.FindProperty("experienceSpriteImage").objectReferenceValue=image;
        so.FindProperty("experienceFrames").objectReferenceValue=frames;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(player);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Experience progress connected to Arena HUD.");
    }
}
