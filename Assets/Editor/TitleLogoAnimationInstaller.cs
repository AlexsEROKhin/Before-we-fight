using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class TitleLogoAnimationInstaller
{
    private const string IntroPath="Assets/Generated/Animations/TitleLogo_Intro.anim";
    private const string IdlePath="Assets/Generated/Animations/TitleLogo_Idle.anim";
    private const string ControllerPath="Assets/Generated/Controllers/TitleLogo.controller";

    [MenuItem("Tools/Before We Fight/Install Title Logo Animation")]
    public static void Install()
    {
        EnsureFolder("Assets/Generated","Animations");EnsureFolder("Assets/Generated","Controllers");
        AnimationClip intro=MakeIntro();AnimationClip idle=MakeIdle();
        AnimatorController controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if(controller==null)controller=AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        AnimatorStateMachine machine=controller.layers[0].stateMachine;
        foreach(AnimatorState state in machine.states.Length>0?System.Array.ConvertAll(machine.states,x=>x.state):System.Array.Empty<AnimatorState>())machine.RemoveState(state);
        AnimatorState introState=machine.AddState("Intro");introState.motion=intro;machine.defaultState=introState;
        AnimatorState idleState=machine.AddState("Idle");idleState.motion=idle;
        AnimatorStateTransition transition=introState.AddTransition(idleState);transition.hasExitTime=true;transition.exitTime=1f;transition.duration=.08f;transition.hasFixedDuration=true;

        Scene scene=EditorSceneManager.OpenScene("Assets/Scenes/Title.unity",OpenSceneMode.Single);
        GameObject logo=GameObject.Find("Game Title Logo");if(logo==null)throw new System.InvalidOperationException("Game Title Logo is missing.");
        Animator animator=logo.GetComponent<Animator>();if(animator==null)animator=logo.AddComponent<Animator>();animator.runtimeAnimatorController=controller;animator.updateMode=AnimatorUpdateMode.UnscaledTime;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        EditorUtility.SetDirty(animator);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("Title logo intro and idle animation installed.");
    }

    static AnimationClip MakeIntro()
    {
        AnimationClip clip=LoadOrCreate(IntroPath,"TitleLogo_Intro");clip.frameRate=30;
        Set(clip,typeof(RectTransform),"m_LocalScale.x",new Keyframe(0,.72f),new Keyframe(.28f,1.07f),new Keyframe(.58f,1f));
        Set(clip,typeof(RectTransform),"m_LocalScale.y",new Keyframe(0,.72f),new Keyframe(.28f,1.07f),new Keyframe(.58f,1f));
        Set(clip,typeof(RectTransform),"m_AnchoredPosition.y",new Keyframe(0,-22),new Keyframe(.28f,4),new Keyframe(.58f,0));
        Set(clip,typeof(Image),"m_Color.a",new Keyframe(0,0),new Keyframe(.16f,1),new Keyframe(.58f,1));
        SetLoop(clip,false);EditorUtility.SetDirty(clip);return clip;
    }
    static AnimationClip MakeIdle()
    {
        AnimationClip clip=LoadOrCreate(IdlePath,"TitleLogo_Idle");clip.frameRate=30;
        Set(clip,typeof(RectTransform),"m_LocalScale.x",new Keyframe(0,1),new Keyframe(1.2f,1.018f),new Keyframe(2.4f,1));
        Set(clip,typeof(RectTransform),"m_LocalScale.y",new Keyframe(0,1),new Keyframe(1.2f,1.018f),new Keyframe(2.4f,1));
        Set(clip,typeof(RectTransform),"m_AnchoredPosition.y",new Keyframe(0,0),new Keyframe(.6f,3),new Keyframe(1.2f,5),new Keyframe(1.8f,3),new Keyframe(2.4f,0));
        Set(clip,typeof(Image),"m_Color.a",new Keyframe(0,1),new Keyframe(2.4f,1));
        SetLoop(clip,true);EditorUtility.SetDirty(clip);return clip;
    }
    static AnimationClip LoadOrCreate(string path,string name){AnimationClip clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);if(clip==null){clip=new AnimationClip{name=name};AssetDatabase.CreateAsset(clip,path);}return clip;}
    static void Set(AnimationClip clip,System.Type type,string property,params Keyframe[] keys){clip.SetCurve("",type,property,new AnimationCurve(keys));}
    static void SetLoop(AnimationClip clip,bool loop){AnimationClipSettings settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=loop;AnimationUtility.SetAnimationClipSettings(clip,settings);}
    static void EnsureFolder(string parent,string name){string path=$"{parent}/{name}";if(!AssetDatabase.IsValidFolder(path))AssetDatabase.CreateFolder(parent,name);}
}
