using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyHitFeedback : MonoBehaviour
{
    [SerializeField, Min(.02f)] private float flashSeconds = .12f;
    [SerializeField, Min(0f)] private float nudgeDistance = .08f;
    [SerializeField] private bool visualOnly;
    [SerializeField] private Color hitColor = new(1f,.35f,.35f,1f);
    private SpriteRenderer visual;
    private Coroutine reaction;
    private Color restingColor;
    private Vector3 restingVisualPosition;
    private bool displaced;

    private void Awake()
    {
        visual = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        if (visual != null) restingColor = visual.color;
    }

    public void React(Vector2 attackDirection)
    {
        if (!enabled || visual == null) return;
        if (reaction != null) { StopCoroutine(reaction); ResetVisual(); }
        reaction = StartCoroutine(ReactRoutine(attackDirection.normalized));
    }

    private IEnumerator ReactRoutine(Vector2 direction)
    {
        restingColor = visual.color;
        restingVisualPosition = visual.transform.localPosition;
        bool separateVisual = visual.transform != transform;
        Vector3 nudge = (Vector3)(direction * nudgeDistance);
        visual.color = hitColor;
        if (!visualOnly && separateVisual)
        {
            visual.transform.localPosition += nudge;
            displaced = true;
        }
        yield return new WaitForSeconds(flashSeconds);
        ResetVisual();
        reaction = null;
    }

    private void ResetVisual()
    {
        if (visual == null) return;
        visual.color = restingColor;
        if (displaced) visual.transform.localPosition = restingVisualPosition;
        displaced = false;
    }

    private void OnDisable()
    {
        if (reaction != null) StopCoroutine(reaction);
        reaction = null;
        ResetVisual();
    }
}
