using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ArenaExpansionEditor
{
    private const float GroundSize = 120f;
    private const float PlayHalfSize = 50f;
    private const float WallThickness = 0.5f;

    [MenuItem("Tools/Before We Fight/Expand Arena And Follow Player")]
    public static void ExpandArenaAndFollowPlayer()
    {
        GameObject ground = FindSceneObject(name => name.StartsWith("Ground (tiled editable prefab", StringComparison.Ordinal));
        SpriteRenderer groundRenderer = ground != null ? ground.GetComponent<SpriteRenderer>() : null;
        if (groundRenderer == null) throw new InvalidOperationException("Editable tiled Ground was not found in the open scene.");

        GameObject bounds = FindSceneObject(name => name.StartsWith("Arena Bounds", StringComparison.Ordinal));
        if (bounds == null) throw new InvalidOperationException("Arena Bounds was not found in the open scene.");

        Camera mainCamera = Camera.main;
        TopDownPlayer player = UnityEngine.Object.FindFirstObjectByType<TopDownPlayer>(FindObjectsInactive.Include);
        if (mainCamera == null || player == null) throw new InvalidOperationException("Main Camera or Player was not found.");

        Undo.RecordObject(groundRenderer, "Expand tiled arena ground");
        Undo.RecordObject(ground.transform, "Center tiled arena ground");
        groundRenderer.drawMode = SpriteDrawMode.Tiled;
        groundRenderer.size = new Vector2(GroundSize, GroundSize);
        Vector3 groundPosition = ground.transform.position;
        ground.transform.position = new Vector3(0f, 0f, groundPosition.z);

        Undo.RecordObject(bounds.transform, "Reset arena bounds scale");
        bounds.transform.localScale = Vector3.one;
        ConfigureWall(bounds.transform, "Wall Left", new Vector2(-PlayHalfSize, 0f), new Vector2(WallThickness, PlayHalfSize * 2f));
        ConfigureWall(bounds.transform, "Wall Right", new Vector2(PlayHalfSize, 0f), new Vector2(WallThickness, PlayHalfSize * 2f));
        ConfigureWall(bounds.transform, "Wall Bottom", new Vector2(0f, -PlayHalfSize), new Vector2(PlayHalfSize * 2f, WallThickness));
        ConfigureWall(bounds.transform, "Wall Top", new Vector2(0f, PlayHalfSize), new Vector2(PlayHalfSize * 2f, WallThickness));

        CameraFollow2D follow = mainCamera.GetComponent<CameraFollow2D>();
        if (follow == null) follow = Undo.AddComponent<CameraFollow2D>(mainCamera.gameObject);
        Undo.RecordObject(follow, "Follow player with camera");
        follow.SetTarget(player.transform);

        EditorUtility.SetDirty(groundRenderer);
        EditorUtility.SetDirty(bounds);
        EditorUtility.SetDirty(follow);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Arena expanded: 120x120 tiled ground, 100x100 playable bounds, camera follows Player in the center.");
    }

    private static void ConfigureWall(Transform root, string name, Vector2 position, Vector2 size)
    {
        Transform wall = root.Cast<Transform>().FirstOrDefault(child => child.name == name);
        if (wall == null) throw new InvalidOperationException(name + " was not found under Arena Bounds.");
        BoxCollider2D collider = wall.GetComponent<BoxCollider2D>();
        if (collider == null) throw new InvalidOperationException(name + " has no BoxCollider2D.");

        Undo.RecordObject(wall, "Move " + name);
        Undo.RecordObject(collider, "Resize " + name);
        wall.localPosition = new Vector3(position.x, position.y, 0f);
        wall.localScale = Vector3.one;
        collider.offset = Vector2.zero;
        collider.size = size;
        EditorUtility.SetDirty(wall);
        EditorUtility.SetDirty(collider);
    }

    private static GameObject FindSceneObject(Func<string, bool> predicate)
    {
        return UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(obj => obj.scene.IsValid() && predicate(obj.name));
    }
}
