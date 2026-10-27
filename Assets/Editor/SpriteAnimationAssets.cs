using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Asset-only maintenance: does not create/edit scenes or enter Play Mode.
public static class SpriteAnimationAssets
{
    [MenuItem("Tools/Before We Fight/Prepare Animator Assets")]
    private static void Prepare()
    {
        MakeController("Hand_Intro", new[] { "Hand_Intro" });
        MakeController("Hand_Slash", new[] { "Hand_Slash" });
        MakeController("Player", new[] { "Player_Idle", "Player_Walk", "Player_Hit", "Player_Jump" });
        AssetDatabase.SaveAssets();
        Debug.Log("Animator assets are ready. Assign the controllers through the Inspector.");
    }

    private static void MakeController(string name, string[] clips)
    {
        const string folder = "Assets/Generated/Animations/";
        string path = folder + name + ".controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        for (int i = 0; i < clips.Length; i++)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(folder + clips[i] + ".anim");
            if (clip == null) throw new System.InvalidOperationException("Missing clip: " + clips[i]);
            var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            var curves = new ObjectReferenceKeyframe[bindings.Length][];
            for (int j = 0; j < bindings.Length; j++)
                curves[j] = AnimationUtility.GetObjectReferenceCurve(clip, bindings[j]);
            clip.legacy = false;
            for (int j = 0; j < bindings.Length; j++)
                AnimationUtility.SetObjectReferenceCurve(clip, bindings[j], curves[j]);
            EditorUtility.SetDirty(clip);
            AnimatorState state = null;
            foreach (var child in machine.states)
                if (child.state.name == clips[i]) state = child.state;
            if (state == null) state = machine.AddState(clips[i], new Vector3(260, 50 + i * 90, 0));
            state.motion = clip;
            state.writeDefaultValues = false;
            if (i == 0) machine.defaultState = state;
        }
        EditorUtility.SetDirty(controller);
    }
}
