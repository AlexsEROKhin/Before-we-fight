using UnityEngine;

[CreateAssetMenu(menuName = "Before We Fight/Sprite Frame Set")]
public sealed class SpriteFrameSet : ScriptableObject
{
    [SerializeField] private Sprite[] frames;

    public int Count => frames == null ? 0 : frames.Length;

    public Sprite GetFrame(int index)
    {
        if (Count == 0) return null;
        return frames[Mathf.Clamp(index, 0, Count - 1)];
    }

#if UNITY_EDITOR
    public void SetFrames(Sprite[] value) => frames = value;
#endif
}
