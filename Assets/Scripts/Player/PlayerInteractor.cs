using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Interact / Lantern")]
    [SerializeField] private float interactionDistance = 0.75f;
    [SerializeField] private float interactionRadius = 0.2f;

    [Header("Guitar")]
    [SerializeField] private Vector2 guitarHitboxSize = new(0.9f, 1.15f);
    [SerializeField] private float guitarHitboxDistance = 0.75f;
    [SerializeField, Min(0f)] private float guitarKnockbackAmount = 1f;
    [SerializeField] private LayerMask guitarHitLayers = ~0;

    private Collider2D[] selfColliders;

    private void Awake()
    {
        selfColliders = GetComponents<Collider2D>();
    }

    public bool TryInteract(Vector2 facingDirection, PlayerController player)
    {
        if (!TryCast(facingDirection, out RaycastHit2D hit))
            return false;

        if (!hit.collider.TryGetComponent<IInteractable>(out IInteractable interactable))
            return false;

        interactable.Interact(player);
        return true;
    }

    public bool TryLight(Vector2 facingDirection, PlayerController player)
    {
        if (!TryCast(facingDirection, out RaycastHit2D hit))
            return false;

        if (!hit.collider.TryGetComponent<ILightable>(out ILightable lightable))
            return false;

        lightable.Light(player);
        return true;
    }

    public int TryGuitarHit(Vector2 facingDirection)
    {
        Vector2 direction = DirectionUtility.ToCardinal(facingDirection);
        Vector2 hitboxCenter = GetGuitarHitboxCenter(direction);
        float hitboxAngle = GetGuitarHitboxAngle(direction);
        Collider2D[] hits = Physics2D.OverlapBoxAll(hitboxCenter, guitarHitboxSize, hitboxAngle, guitarHitLayers);

        HashSet<int> processedTargets = new();
        int hitCount = 0;

        foreach (Collider2D hit in hits)
        {
            if (hit == null || IsSelfCollider(hit))
                continue;

            if (!TryGetInterface(hit, out IKnockbackable knockbackable, out Component targetComponent))
                continue;

            if (targetComponent == null || !processedTargets.Add(targetComponent.GetInstanceID()))
                continue;

            knockbackable.ApplyKnockbackFrom(transform.position, guitarKnockbackAmount);
            hitCount++;
        }

        return hitCount;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 origin = transform.position;

        Vector2 direction = Vector2.down;
        PlayerMotor motor = GetComponent<PlayerMotor>();
        if (motor != null && motor.FacingDirection.sqrMagnitude > 0.001f)
            direction = motor.FacingDirection;

        DrawInteractionGizmo(origin, direction);
        DrawGuitarGizmo(direction);
    }

    private void DrawInteractionGizmo(Vector2 origin, Vector2 direction)
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, interactionRadius);
        Gizmos.DrawLine(origin, origin + direction * interactionDistance);
        Gizmos.DrawWireSphere(origin + direction * interactionDistance, interactionRadius);
    }

    private void DrawGuitarGizmo(Vector2 direction)
    {
        Vector2 hitboxCenter = GetGuitarHitboxCenter(direction);
        float hitboxAngle = GetGuitarHitboxAngle(direction);
        Matrix4x4 previousMatrix = Gizmos.matrix;

        Gizmos.color = new Color(1f, 0.35f, 0.15f, 0.55f);
        Gizmos.matrix = Matrix4x4.TRS(hitboxCenter, Quaternion.Euler(0f, 0f, hitboxAngle), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, guitarHitboxSize);
        Gizmos.matrix = previousMatrix;
    }

    private bool TryCast(Vector2 facingDirection, out RaycastHit2D validHit)
    {
        Vector2 direction = DirectionUtility.ToCardinal(facingDirection);
        Vector2 origin = transform.position;
        RaycastHit2D[] hits = Physics2D.CircleCastAll(origin, interactionRadius, direction, interactionDistance);

        foreach (RaycastHit2D hit in hits)
        {
            if (!hit.collider || IsSelfCollider(hit.collider))
                continue;

            validHit = hit;
            return true;
        }

        validHit = default;
        return false;
    }

    private Vector2 GetGuitarHitboxCenter(Vector2 direction)
    {
        return (Vector2)transform.position + direction * guitarHitboxDistance;
    }

    private static float GetGuitarHitboxAngle(Vector2 direction)
    {
        return Mathf.Abs(direction.x) > 0.5f ? 90f : 0f;
    }

    private bool IsSelfCollider(Collider2D collider)
    {
        if (collider == null)
            return false;

        foreach (Collider2D selfCollider in selfColliders)
        {
            if (collider == selfCollider)
                return true;
        }

        return false;
    }

    private static bool TryGetInterface<T>(Collider2D source, out T value, out Component component)
        where T : class
    {
        value = null;
        component = null;

        if (source == null)
            return false;

        component = source.GetComponent(typeof(T));
        if (component == null)
            component = source.GetComponentInParent(typeof(T));
        if (component == null)
            component = source.GetComponentInChildren(typeof(T));
        if (component == null)
            return false;

        value = component as T;
        return value != null;
    }
}
