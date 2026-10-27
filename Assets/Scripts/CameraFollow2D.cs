using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public sealed class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 offset;

    public void SetTarget(Transform value)
    {
        target = value;
        SnapToTarget();
    }

    private void LateUpdate()
    {
        SnapToTarget();
    }

    private void SnapToTarget()
    {
        if (target == null) return;
        transform.position = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z);
    }
}
