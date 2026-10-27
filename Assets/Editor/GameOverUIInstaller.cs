using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GameOverUIInstaller
{
    [MenuItem("Tools/Before We Fight/Install Game Over UI")]
    public static void Install()
    {
        Scene scene=EditorSceneManager.OpenScene("Assets/Scenes/Arena.unity",OpenSceneMode.Single);
        GameObject canvas=GameObject.Find("Arena HUD Canvas");TopDownPlayer player=Object.FindFirstObjectByType<TopDownPlayer>();
        if(canvas==null||player==null)throw new System.InvalidOperationException("Arena HUD Canvas or Player is missing.");
        Transform old=canvas.transform.Find("Game Over Panel");if(old!=null)Object.DestroyImmediate(old.gameObject);
        Font font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        GameObject panel=UI("Game Over Panel",canvas.transform);Stretch(panel.GetComponent<RectTransform>());Image shade=panel.AddComponent<Image>();shade.color=new Color(.035f,.025f,.04f,.88f);GameOverUI controller=panel.AddComponent<GameOverUI>();
        GameObject title=Text("DEFEAT",panel.transform,font,34,Color.white);Rect(title,new Vector2(0,65),new Vector2(260,55));
        GameObject retry=Button("RETRY Button","RETRY",panel.transform,font,new Vector2(0,0));
        GameObject menu=Button("MENU Button","MENU",panel.transform,font,new Vector2(0,-58));
        UnityEventTools.AddPersistentListener(retry.GetComponent<Button>().onClick,controller.RestartArena);
        UnityEventTools.AddPersistentListener(menu.GetComponent<Button>().onClick,controller.ReturnToMenu);
        var so=new SerializedObject(player);so.FindProperty("gameOverPanel").objectReferenceValue=panel;so.ApplyModifiedPropertiesWithoutUndo();
        panel.SetActive(false);EditorUtility.SetDirty(player);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("Game Over UI installed in Arena.");
    }
    static GameObject UI(string name,Transform parent){var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);return go;}
    static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;}
    static void Rect(GameObject go,Vector2 pos,Vector2 size){var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.anchoredPosition=pos;r.sizeDelta=size;}
    static GameObject Text(string value,Transform parent,Font font,int size,Color color){var go=UI(value,parent);var t=go.AddComponent<Text>();t.text=value;t.font=font;t.fontSize=size;t.alignment=TextAnchor.MiddleCenter;t.color=color;t.raycastTarget=false;return go;}
    static GameObject Button(string name,string label,Transform parent,Font font,Vector2 pos){var go=UI(name,parent);Rect(go,pos,new Vector2(190,46));var image=go.AddComponent<Image>();image.color=new Color(.22f,.11f,.16f,1f);var button=go.AddComponent<Button>();var colors=button.colors;colors.highlightedColor=new Color(.48f,.22f,.28f,1f);colors.pressedColor=new Color(.7f,.28f,.3f,1f);button.colors=colors;GameObject text=Text(label,go.transform,font,20,Color.white);Stretch(text.GetComponent<RectTransform>());return go;}
}
