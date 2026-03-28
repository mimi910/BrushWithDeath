using UnityEngine;

public class PistaController : MonoBehaviour
{
    public enum PistaState
    {
        FollowingPlayer,
        Aiming,
        MovingToLantern,
        LatchedToLantern
    }

    [Header("References")]
    [SerializeField] private Transform playerTarget;

    [Header("Follow")]
    [SerializeField] private Vector2 followOffset = new Vector2(0.75f, 0f);
    [SerializeField] private float followMoveSpeed = 8f;

    [Header("Lantern Travel")]
    [SerializeField] private float lanternMoveSpeed = 10f;
    [SerializeField] private float arrivalDistance = 0.05f;

    public PistaState CurrentState { get; private set; } = PistaState.FollowingPlayer;
    public Transform CurrentLanternTarget { get; private set; }

    private void Awake()
    {
        if (playerTarget != null && CurrentState == PistaState.FollowingPlayer)
            transform.position = GetFollowAnchorPosition();
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case PistaState.FollowingPlayer:
                MoveToward(GetFollowAnchorPosition(), followMoveSpeed);
                break;

            case PistaState.Aiming:
                MoveToward(GetFollowAnchorPosition(), followMoveSpeed);
                break;

            case PistaState.MovingToLantern:
                UpdateMoveToLantern();
                break;

            case PistaState.LatchedToLantern:
                SnapToLanternTarget();
                break;
        }
    }

    public void SetPlayerTarget(Transform targetTransform)
    {
        playerTarget = targetTransform;
    }

    public void BeginAiming()
    {
        if (CurrentState == PistaState.LatchedToLantern)
            return;

        CurrentState = PistaState.Aiming;
    }

    public void EndAiming()
    {
        if (CurrentState == PistaState.LatchedToLantern)
            return;

        CurrentState = PistaState.FollowingPlayer;
    }

    public void MoveToLantern(Transform lanternTarget)
    {
        if (lanternTarget == null)
            return;

        CurrentLanternTarget = lanternTarget;
        CurrentState = PistaState.MovingToLantern;
    }

    public void RecallToPlayer()
    {
        CurrentLanternTarget = null;
        CurrentState = PistaState.FollowingPlayer;
    }

    public Vector3 GetFollowAnchorPosition()
    {
        if (playerTarget == null)
            return transform.position;

        return playerTarget.position + (Vector3)followOffset;
    }

    private void UpdateMoveToLantern()
    {
        if (CurrentLanternTarget == null)
        {
            CurrentState = PistaState.FollowingPlayer;
            return;
        }

        MoveToward(CurrentLanternTarget.position, lanternMoveSpeed);

        if ((CurrentLanternTarget.position - transform.position).sqrMagnitude <= arrivalDistance * arrivalDistance)
        {
            transform.position = CurrentLanternTarget.position;
            CurrentState = PistaState.LatchedToLantern;
        }
    }

    private void SnapToLanternTarget()
    {
        if (CurrentLanternTarget == null)
        {
            CurrentState = PistaState.FollowingPlayer;
            return;
        }

        transform.position = CurrentLanternTarget.position;
    }

    private void MoveToward(Vector3 targetPosition, float speed)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(GetFollowAnchorPosition(), 0.1f);

        if (CurrentLanternTarget == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, CurrentLanternTarget.position);
        Gizmos.DrawWireSphere(CurrentLanternTarget.position, 0.12f);
    }
}
