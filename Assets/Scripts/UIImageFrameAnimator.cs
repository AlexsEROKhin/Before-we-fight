using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
public sealed class UIImageFrameAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private SpriteFrameSet frameSet;
    [SerializeField] private Image targetImage;
    [SerializeField, Min(0.1f)] private float framesPerSecond = 10f;
    [SerializeField] private bool playOnEnable = true;
    [SerializeField] private bool pauseWhilePointerOver = true;
    [SerializeField, Range(0f, 1f)] private float hoverAlphaThreshold = 0.1f;

    private Image image;
    private int frameIndex;
    private float elapsed;
    private bool playing;
    private bool pointerOver;
    private bool playOnce;
    private Action onPlaybackComplete;

    private void Awake()
    {
        image = targetImage != null ? targetImage : GetComponent<Image>();
        Image hitImage = GetComponent<Image>();
        if (hitImage.sprite != null) hitImage.alphaHitTestMinimumThreshold = hoverAlphaThreshold;
    }

    private void OnEnable()
    {
        frameIndex = 0;
        elapsed = 0f;
        playing = playOnEnable;
        playOnce = false;
        onPlaybackComplete = null;
        pointerOver = false;
        if (image == null) image = targetImage != null ? targetImage : GetComponent<Image>();
        ShowCurrentFrame();
    }

    private void Update()
    {
        if (!playing || (pauseWhilePointerOver && pointerOver) || frameSet == null || frameSet.Count < 2) return;

        float frameDuration = 1f / framesPerSecond;
        elapsed += Time.unscaledDeltaTime;
        while (elapsed >= frameDuration)
        {
            elapsed -= frameDuration;
            if (playOnce && frameIndex >= frameSet.Count - 1)
            {
                playing = false;
                playOnce = false;
                Action callback = onPlaybackComplete;
                onPlaybackComplete = null;
                callback?.Invoke();
                return;
            }

            frameIndex = (frameIndex + 1) % frameSet.Count;
            ShowCurrentFrame();
        }
    }

    public void PlayOnce(Action completed = null)
    {
        if (frameSet == null || frameSet.Count == 0)
        {
            completed?.Invoke();
            return;
        }

        frameIndex = 0;
        elapsed = 0f;
        pointerOver = false;
        playOnce = true;
        playing = true;
        onPlaybackComplete = completed;
        ShowCurrentFrame();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerOver = false;
    }

    private void ShowCurrentFrame()
    {
        if (image == null) image = GetComponent<Image>();
        if (frameSet != null && frameSet.Count > 0)
            image.sprite = frameSet.GetFrame(frameIndex);
    }
}
