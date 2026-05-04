using UnityEngine;

public class WorldMapCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 minBounds = new Vector2(-7.5f, -3.5f);
    [SerializeField] private Vector2 maxBounds = new Vector2(7.5f, 3.5f);
    [SerializeField] private float smoothTime = 0.18f;

    private Vector3 velocity;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = new Vector3(target.position.x, target.position.y, transform.position.z);
        desired.x = Mathf.Clamp(desired.x, minBounds.x, maxBounds.x);
        desired.y = Mathf.Clamp(desired.y, minBounds.y, maxBounds.y);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }
}
