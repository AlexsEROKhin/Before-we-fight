using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button), typeof(CanvasGroup))]
public sealed class DelayedSceneButton : MonoBehaviour
{
    [SerializeField] private string nextScene = "Arena";
    [SerializeField, Min(0)] private float showAfterSeconds = 2f;
    [SerializeField] private bool animateBeforeLoading;
    private CanvasGroup visibility;
    private Button button;
    private float revealAt;
    private bool loading;

    private void Awake()
    {
        visibility = GetComponent<CanvasGroup>();
        button = GetComponent<Button>();
        button.onClick.AddListener(LoadScene);
    }

    private void OnEnable()
    {
        loading = false;
        revealAt = Time.unscaledTime + showAfterSeconds;
        SetVisible(showAfterSeconds <= 0f);
    }

    private void Update()
    {
        if (!loading && !visibility.interactable && Time.unscaledTime >= revealAt)
            SetVisible(true);
    }

    private void SetVisible(bool visible)
    {
        visibility.alpha = visible ? 1f : 0f;
        visibility.interactable = visible;
        visibility.blocksRaycasts = visible;
        button.interactable = visible;
    }

    public void LoadScene()
    {
        if (loading || Time.unscaledTime < revealAt) return;
        if (!Application.CanStreamedLevelBeLoaded(nextScene))
        {
            Debug.LogError("Add the scene '" + nextScene + "' to Build Profiles > Scene List.", this);
            return;
        }
        loading = true;
        button.interactable = false;
        UIImageFrameAnimator animator = GetComponent<UIImageFrameAnimator>();
        if (animateBeforeLoading && animator != null)
        {
            animator.PlayOnce(LoadSceneNow);
            return;
        }

        LoadSceneNow();
    }

    private void LoadSceneNow()
    {
        SceneManager.LoadScene(nextScene);
    }

    private void OnDestroy()
    {
        if (button != null) button.onClick.RemoveListener(LoadScene);
    }
}
