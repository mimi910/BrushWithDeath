using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class SkeletonProjectile : MonoBehaviour
{
    [SerializeField] private Rigidbody2D body;
    [SerializeField, Min(0.01f)] private float speed = 6f;
    [SerializeField, Min(0.05f)] private float lifetime = 3f;
    [SerializeField] private bool destroyOnImpact = true;

    private GameObject owner;
    private Vector2 direction = Vector2.right;
    private float damage = 1f;
    private float lifetimeTimer;

    private void Reset()
    {
        if (body == null)
            body = GetComponent<Rigidbody2D>();
    }

    private void Awake()
    {
        if (body == null)
            body = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        lifetimeTimer = lifetime;
    }

    public void Initialize(GameObject ownerObject, Vector2 moveDirection, float projectileSpeed, float projectileDamage)
    {
        owner = ownerObject;
        direction = moveDirection.sqrMagnitude > Mathf.Epsilon ? moveDirection.normalized : Vector2.right;
        speed = projectileSpeed;
        damage = projectileDamage;
        lifetimeTimer = lifetime;

        if (body != null)
            body.linearVelocity = direction * speed;
    }

    private void Update()
    {
        lifetimeTimer -= Time.deltaTime;
        if (lifetimeTimer <= 0f)
            Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        if (body != null)
        {
            body.linearVelocity = direction * speed;
            return;
        }

        transform.position += (Vector3)(direction * speed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.collider);
    }

    private void HandleHit(Collider2D other)
    {
        if (other == null || IsOwnedCollider(other))
            return;

        if (TryGetInterface(other, out IDamageable damageable))
        {
            damageable.ReceiveDamage(damage, direction, owner != null ? owner : gameObject);

            if (destroyOnImpact)
                Destroy(gameObject);

            return;
        }

        if (!other.isTrigger && destroyOnImpact)
            Destroy(gameObject);
    }

    private bool IsOwnedCollider(Collider2D other)
    {
        if (owner == null)
            return false;

        return other.transform == owner.transform || other.transform.IsChildOf(owner.transform);
    }

    private static bool TryGetInterface<T>(Component source, out T value)
        where T : class
    {
        value = null;

        if (source == null)
            return false;

        value = source.GetComponent(typeof(T)) as T;
        if (value != null)
            return true;

        value = source.GetComponentInParent(typeof(T)) as T;
        if (value != null)
            return true;

        value = source.GetComponentInChildren(typeof(T)) as T;
        return value != null;
    }
}
