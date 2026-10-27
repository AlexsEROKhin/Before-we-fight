using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class GameOverUI : MonoBehaviour
{
    [SerializeField] private string menuScene = "Title";

    private void OnEnable() => Time.timeScale = 0f;
    private void OnDisable() => Time.timeScale = 1f;

    public void RestartArena()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        if (Application.CanStreamedLevelBeLoaded(menuScene)) SceneManager.LoadScene(menuScene);
    }
}
