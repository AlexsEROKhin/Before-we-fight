using UnityEngine;

public static class GameCursor
{
    private const string CursorResourcePath = "UI/custom_cursor";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyCustomCursor()
    {
        Texture2D cursorTexture = Resources.Load<Texture2D>(CursorResourcePath);
        if (cursorTexture == null)
        {
            Debug.LogWarning("Custom cursor texture is missing from Resources/UI/custom_cursor.png.");
            return;
        }

        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
    }
}
