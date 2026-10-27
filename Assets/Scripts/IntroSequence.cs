using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public sealed class IntroSequence : MonoBehaviour
{
    [SerializeField] private string nextScene = "Title";
    [SerializeField, Min(0)] private float holdAfterAnimation = 0.2f;
    [Header("Hand entrance from below the frame")]
    [SerializeField, Min(0.05f)] private float entranceDuration = 0.6f;
    [SerializeField, Range(0f, 1f)] private float animationStartDuringEntrance = 0.72f;
    [SerializeField, Min(0)] private float hiddenPadding = 0.25f;

    private IEnumerator Start()
    {
        Animator animationPlayer = GetComponent<Animator>();
        Animation oldPreview = GetComponent<Animation>();
        if (oldPreview != null) { oldPreview.Stop(); oldPreview.enabled = false; }
        RuntimeAnimatorController controller = animationPlayer.runtimeAnimatorController;
        if (controller == null || controller.animationClips.Length == 0)
        {
            Debug.LogError("IntroSequence: assign the Hand Intro controller to Animator.", this);
            yield break;
        }
        AnimationClip clip = controller.animationClips[0];
        SpriteRenderer handRenderer = GetComponent<SpriteRenderer>();
        Camera introCamera = Camera.main;
        Vector3 finalPosition = transform.position;
        Vector3 hiddenPosition = finalPosition;

        if (introCamera != null && handRenderer != null && introCamera.orthographic)
        {
            float cameraBottom = introCamera.transform.position.y - introCamera.orthographicSize;
            hiddenPosition.y = cameraBottom - handRenderer.bounds.extents.y - hiddenPadding;
            transform.position = hiddenPosition;
        }

        // Hold the first drawn frame while the complete hand enters the camera.
        // Transform motion is updated every rendered frame, so no extra sprite frames are needed.
        animationPlayer.Play(clip.name, 0, 0f);
        animationPlayer.Update(0f);
        animationPlayer.speed = 0f;
        bool frameAnimationStarted = false;
        float animationRunningFor = 0f;
        float entranceElapsed = 0f;
        while (entranceElapsed < entranceDuration)
        {
            entranceElapsed += Time.deltaTime;
            float linearProgress = Mathf.Clamp01(entranceElapsed / entranceDuration);
            // Quintic smoother-step: soft acceleration and soft braking without a visible jerk.
            float smoothProgress = linearProgress * linearProgress * linearProgress *
                                   (linearProgress * (linearProgress * 6f - 15f) + 10f);
            transform.position = Vector3.LerpUnclamped(hiddenPosition, finalPosition, smoothProgress);
            if (!frameAnimationStarted && linearProgress >= animationStartDuringEntrance)
            {
                animationPlayer.speed = 1f;
                frameAnimationStarted = true;
            }
            if (frameAnimationStarted) animationRunningFor += Time.deltaTime;
            yield return null;
        }
        transform.position = finalPosition;
        if (!frameAnimationStarted) animationPlayer.speed = 1f;

        float remainingAnimation = Mathf.Max(0f, clip.length - animationRunningFor);
        yield return new WaitForSeconds(remainingAnimation + holdAfterAnimation);
        if (Application.CanStreamedLevelBeLoaded(nextScene))
            SceneManager.LoadScene(nextScene);
        else
            Debug.LogError("Add the scene '" + nextScene + "' to Build Profiles > Scene List.", this);
    }
}
