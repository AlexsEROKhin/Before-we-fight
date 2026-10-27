using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class TitleResponsiveLayoutInstaller
{
    [MenuItem("Tools/Before We Fight/Fix Responsive Title Layout")]
    public static void Install()
    {
        Scene scene=EditorSceneManager.OpenScene("Assets/Scenes/Title.unity",OpenSceneMode.Single);
        GameObject logo=GameObject.Find("Game Title Logo");
        GameObject play=GameObject.Find("PLAY Button");
        if(logo==null||play==null)throw new System.InvalidOperationException("Game Title Logo or PLAY Button is missing.");

        RectTransform logoRect=logo.GetComponent<RectTransform>();
        logoRect.anchorMin=logoRect.anchorMax=new Vector2(.5f,.69f);
        logoRect.pivot=new Vector2(.5f,.5f);
        logoRect.anchoredPosition=Vector2.zero;
        logoRect.sizeDelta=new Vector2(340,340);
        logoRect.localScale=Vector3.one;

        RectTransform playRect=play.GetComponent<RectTransform>();
        playRect.anchorMin=playRect.anchorMax=new Vector2(.5f,.20f);
        playRect.pivot=new Vector2(.5f,.5f);
        playRect.anchoredPosition=Vector2.zero;
        playRect.sizeDelta=new Vector2(114,58);
        playRect.localScale=Vector3.one;

        Transform visual=play.transform.Find("Animated Visual");
        if(visual!=null)
        {
            RectTransform visualRect=(RectTransform)visual;
            visualRect.anchorMin=visualRect.anchorMax=new Vector2(.5f,.5f);
            visualRect.pivot=new Vector2(.5f,.5f);
            visualRect.anchoredPosition=Vector2.zero;
            visualRect.sizeDelta=new Vector2(256,256);
            visualRect.localScale=Vector3.one;
        }

        Canvas parentCanvas=play.GetComponentInParent<Canvas>();
        if(parentCanvas!=null)
        {
            CanvasScaler scaler=parentCanvas.GetComponent<CanvasScaler>();
            if(scaler!=null){scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;scaler.matchWidthOrHeight=.5f;}
        }
        EditorUtility.SetDirty(logoRect);EditorUtility.SetDirty(playRect);
        EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("Responsive title layout installed.");
    }
}
