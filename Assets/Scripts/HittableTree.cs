using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public sealed class HittableTree : MonoBehaviour
{
    [SerializeField] private SpriteFrameSet hitFrames;
    [SerializeField, Min(1f)] private float framesPerSecond = 12f;

    private SpriteRenderer spriteRenderer;
    private Coroutine animationRoutine;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (hitFrames != null && hitFrames.Count > 0)
            spriteRenderer.sprite = hitFrames.GetFrame(0);
    }

    public void TakeHit(Vector2 attackDirection)
    {
        if (animationRoutine != null) StopCoroutine(animationRoutine);
        animationRoutine = StartCoroutine(PlayHit());
    }

    private IEnumerator PlayHit()
    {
        if (hitFrames == null || hitFrames.Count == 0) yield break;

        float delay = 1f / framesPerSecond;
        for (int i = 0; i < hitFrames.Count; i++)
        {
            spriteRenderer.sprite = hitFrames.GetFrame(i);
            yield return new WaitForSeconds(delay);
        }

        spriteRenderer.sprite = hitFrames.GetFrame(0);
        animationRoutine = null;
    }
}
