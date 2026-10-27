using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class TitleBattleBackgroundInstaller
{
    private const string BackgroundPath="Assets/Art/Imported/title_battle_background.png";
    [MenuItem("Tools/Before We Fight/Install Battle Title Background")]
    public static void Install()
    {
        AssetDatabase.ImportAsset(BackgroundPath,ImportAssetOptions.ForceSynchronousImport);
        TextureImporter importer=(TextureImporter)AssetImporter.GetAtPath(BackgroundPath);
        importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=100;
        importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.SaveAndReimport();
        Sprite background=AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);

        Scene scene=EditorSceneManager.OpenScene("Assets/Scenes/Title.unity",OpenSceneMode.Single);
        GameObject logo=GameObject.Find("Game Title Logo");if(logo==null)throw new System.InvalidOperationException("Game Title Logo is missing.");
        Transform parent=logo.transform.parent;
        Transform old=parent.Find("Battle Background");if(old!=null)Object.DestroyImmediate(old.gameObject);
        old=parent.Find("Battle Background Shade");if(old!=null)Object.DestroyImmediate(old.gameObject);

        GameObject bg=new("Battle Background",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));bg.transform.SetParent(parent,false);bg.transform.SetAsFirstSibling();Stretch((RectTransform)bg.transform);
        Image bgImage=bg.GetComponent<Image>();bgImage.sprite=background;bgImage.preserveAspect=false;bgImage.raycastTarget=false;
        GameObject shade=new("Battle Background Shade",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));shade.transform.SetParent(parent,false);shade.transform.SetSiblingIndex(1);Stretch((RectTransform)shade.transform);
        Image shadeImage=shade.GetComponent<Image>();shadeImage.color=new Color(.03f,.015f,.025f,.24f);shadeImage.raycastTarget=false;

        RectTransform logoRect=logo.GetComponent<RectTransform>();logoRect.anchorMin=logoRect.anchorMax=new Vector2(.5f,.70f);logoRect.anchoredPosition=Vector2.zero;logoRect.sizeDelta=new Vector2(430,430);logoRect.localScale=Vector3.one;
        Outline outline=logo.GetComponent<Outline>();if(outline==null)outline=logo.AddComponent<Outline>();outline.effectColor=new Color(.015f,.01f,.015f,.95f);outline.effectDistance=new Vector2(4,-4);outline.useGraphicAlpha=true;
        Shadow shadow=null;foreach(Shadow candidate in logo.GetComponents<Shadow>())if(candidate.GetType()==typeof(Shadow)){shadow=candidate;break;}if(shadow==null)shadow=logo.AddComponent<Shadow>();shadow.effectColor=new Color(.18f,.02f,.02f,.75f);shadow.effectDistance=new Vector2(7,-7);shadow.useGraphicAlpha=true;

        GameObject play=GameObject.Find("PLAY Button");if(play!=null){RectTransform r=play.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(.5f,.17f);r.anchoredPosition=Vector2.zero;}
        EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("Battle background and improved title installed.");
    }
    private static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;}
}
