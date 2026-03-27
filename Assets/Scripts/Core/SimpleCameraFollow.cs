using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float followDistance = 10f;

    private void Awake()
    {
        if (target == null)
            target = FindFirstObjectByType<PlayerController>()?.transform;

        SnapToTarget();
    }

    private void LateUpdate()
    {
        SnapToTarget();
    }

    private void OnValidate()
    {
        if (followDistance < 0f)
            followDistance = 0f;
    }

    private void Reset()
    {
        target = FindFirstObjectByType<PlayerController>()?.transform;
    }

    private void SnapToTarget()
    {
        if (target == null)
            return;

        Vector3 targetPosition = target.position;
        transform.position = new Vector3(targetPosition.x, targetPosition.y, targetPosition.z - followDistance);
    }
}
