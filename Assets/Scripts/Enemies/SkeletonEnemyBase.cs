using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public abstract class SkeletonEnemyBase : MonoBehaviour, IKnockbackable
{
    [Serializable]
    private class TempoStatModifier
    {
        [Min(0f)] public float moveSpeedMultiplier = 1f;
        [Min(0f)] public float knockbackMultiplier = 1f;
        [Min(0f)] public float damageMultiplier = 1f;
        [Min(0f)] public float attackSpeedMultiplier = 1f;
        [Min(0f)] public float attackRangeMultiplier = 1f;
    }

    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private TempoService tempoService;
    [SerializeField] private Rigidbody2D body;
    [SerializeField] private Animator animator;
    [SerializeField] private Collider2D hitbox;

    [Header("Stats")]
    [SerializeField, Min(0f)] private float moveSpeed = 2.5f;
    [SerializeField, FormerlySerializedAs("knockbackAmount"), Min(0f)] private float knockbackDistance = 1f;
    [SerializeField, Min(0f)] private float damage = 1f;
    [SerializeField, Min(0.01f)] private float attackSpeed = 1f;
    [SerializeField, Min(0f)] private float attackRange = 1.25f;

    [Header("Behavior")]
    [SerializeField, Min(0f)] private float detectionRange = 6f;
    [SerializeField, Min(0.01f)] private float knockbackDuration = 0.12f;
    [SerializeField, Min(0f)] private float deathCleanupDelay = 1.25f;
    [SerializeField] private bool destroyAfterDeath = true;

    [Header("Tempo")]
    [SerializeField] private TempoStatModifier slowTempo = new();
    [SerializeField] private TempoStatModifier midTempo = new();
    [SerializeField] private TempoStatModifier fastTempo = new();
    [SerializeField] private TempoStatModifier intenseTempo = new();

    [Header("Animation")]
    [SerializeField] private string attackTriggerName = "Attack";
    [SerializeField] private string deathTriggerName = "Die";
    [SerializeField] private string deadBoolName = "IsDead";

    [Header("Events")]
    [SerializeField] private UnityEvent onAttack;
    [SerializeField] private UnityEvent onDeath;

    private Vector2 desiredVelocity;
    private Vector2 knockbackVelocity;
    private float attackCooldownTimer;
    private float knockbackTimer;

    protected Transform Target => target;
    protected TempoBand CurrentTempo { get; private set; } = TempoBand.Mid;
    protected Vector2 FacingDirection { get; private set; } = Vector2.down;
    protected bool IsDead { get; private set; }
    protected float EffectiveDamage => damage * GetTempoModifier(CurrentTempo).damageMultiplier;
    protected float EffectiveAttackRange => attackRange * GetTempoModifier(CurrentTempo).attackRangeMultiplier;

    protected virtual void Reset()
    {
        CacheReferences();
    }

    protected virtual void OnValidate()
    {
        CacheReferences();
    }

    protected virtual void Awake()
    {
        CacheReferences();
        ResolveTarget();
    }

    protected virtual void OnEnable()
    {
        SubscribeToTempo();
    }

    protected virtual void OnDisable()
    {
        if (tempoService != null)
            tempoService.TempoUpdated -= HandleTempoUpdated;
    }

    protected virtual void Update()
    {
        if (IsDead)
        {
            desiredVelocity = Vector2.zero;
            UpdateAnimator(Vector2.zero);
            return;
        }

        if (attackCooldownTimer > 0f)
            attackCooldownTimer -= Time.deltaTime;

        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
                knockbackVelocity = Vector2.zero;

            UpdateAnimator(knockbackVelocity);
            return;
        }

        if (!ResolveTarget())
        {
            desiredVelocity = Vector2.zero;
            UpdateAnimator(Vector2.zero);
            return;
        }

        Vector2 toTarget = (Vector2)(target.position - transform.position);
        float distanceToTarget = toTarget.magnitude;

        if (distanceToTarget > Mathf.Epsilon)
            FacingDirection = DirectionUtility.ToCardinal(toTarget);

        if (detectionRange > 0f && distanceToTarget > detectionRange)
        {
            desiredVelocity = Vector2.zero;
            UpdateAnimator(Vector2.zero);
            return;
        }

        TickBehavior(toTarget, distanceToTarget);
        UpdateAnimator(desiredVelocity);
    }

    protected virtual void FixedUpdate()
    {
        if (body == null)
            return;

        if (IsDead)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        body.linearVelocity = knockbackTimer > 0f ? knockbackVelocity : desiredVelocity;
    }

    public void ApplyKnockback(Vector2 direction, float strengthMultiplier = 1f)
    {
        if (IsDead)
            return;

        Vector2 knockbackDirection = direction.sqrMagnitude > Mathf.Epsilon
            ? direction.normalized
            : -FacingDirection;

        float totalDistance = knockbackDistance * GetTempoModifier(CurrentTempo).knockbackMultiplier * Mathf.Max(0f, strengthMultiplier);
        float duration = Mathf.Max(0.01f, knockbackDuration);

        desiredVelocity = Vector2.zero;
        knockbackVelocity = knockbackDirection * (totalDistance / duration);
        knockbackTimer = duration;
    }

    public void ApplyKnockbackFrom(Vector2 sourcePosition, float strengthMultiplier = 1f)
    {
        ApplyKnockback((Vector2)transform.position - sourcePosition, strengthMultiplier);
    }

    public void Kill()
    {
        if (IsDead)
            return;

        IsDead = true;
        desiredVelocity = Vector2.zero;
        knockbackVelocity = Vector2.zero;
        attackCooldownTimer = 0f;
        knockbackTimer = 0f;

        if (tempoService != null)
            tempoService.TempoUpdated -= HandleTempoUpdated;

        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.simulated = false;
        }

        if (hitbox != null)
            hitbox.enabled = false;

        if (animator != null)
        {
            if (!string.IsNullOrWhiteSpace(deadBoolName))
                animator.SetBool(deadBoolName, true);

            if (!string.IsNullOrWhiteSpace(deathTriggerName))
                animator.SetTrigger(deathTriggerName);
        }

        onDeath?.Invoke();

        if (destroyAfterDeath)
            Destroy(gameObject, deathCleanupDelay);
    }

    protected bool TryGetDamageableTarget(out IDamageable damageable)
    {
        return TryGetInterface(target, out damageable);
    }

    protected virtual void TickBehavior(Vector2 toTarget, float distanceToTarget)
    {
        if (distanceToTarget > EffectiveAttackRange)
        {
            desiredVelocity = toTarget.sqrMagnitude > Mathf.Epsilon
                ? toTarget.normalized * GetMoveSpeed()
                : Vector2.zero;
            return;
        }

        desiredVelocity = Vector2.zero;

        if (attackCooldownTimer > 0f)
            return;

        Vector2 attackDirection = toTarget.sqrMagnitude > Mathf.Epsilon ? toTarget.normalized : FacingDirection;
        if (!PerformAttack(attackDirection, distanceToTarget))
            return;

        attackCooldownTimer = GetAttackCooldown();

        if (animator != null && !string.IsNullOrWhiteSpace(attackTriggerName))
            animator.SetTrigger(attackTriggerName);

        onAttack?.Invoke();
    }

    protected abstract bool PerformAttack(Vector2 attackDirection, float distanceToTarget);

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        TryHandleMarigoldContact(other);
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        TryHandleMarigoldContact(collision.collider);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.45f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    private void CacheReferences()
    {
        if (body == null)
            body = GetComponent<Rigidbody2D>();

        if (hitbox == null)
            hitbox = GetComponent<Collider2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void SubscribeToTempo()
    {
        if (tempoService == null)
            tempoService = TempoService.Instance != null ? TempoService.Instance : FindAnyObjectByType<TempoService>();

        if (tempoService == null)
            return;

        tempoService.TempoUpdated -= HandleTempoUpdated;
        tempoService.TempoUpdated += HandleTempoUpdated;
        HandleTempoUpdated(tempoService.GetCurrentSnapshot());
    }

    private void HandleTempoUpdated(TempoStateSnapshot snapshot)
    {
        if (snapshot.UpdateType != TempoUpdateType.Initialized &&
            snapshot.UpdateType != TempoUpdateType.ChannelCompleted)
        {
            return;
        }

        CurrentTempo = snapshot.CurrentTempo;
    }

    private bool ResolveTarget()
    {
        if (target != null)
            return true;

        PlayerDamageReceiver damageReceiver = FindAnyObjectByType<PlayerDamageReceiver>();
        if (damageReceiver != null)
        {
            target = damageReceiver.transform;
            return true;
        }

        PlayerController playerController = FindAnyObjectByType<PlayerController>();
        if (playerController != null)
        {
            target = playerController.transform;
            return true;
        }

        return false;
    }

    private void TryHandleMarigoldContact(Collider2D other)
    {
        if (IsDead || other == null)
            return;

        MarigoldHazard marigoldHazard = other.GetComponentInParent<MarigoldHazard>();
        if (marigoldHazard != null && marigoldHazard.IsActive)
        {
            Kill();
            return;
        }

        LightableTorch torch = other.GetComponentInParent<LightableTorch>();
        if (torch != null &&
            torch.Type == LightableTorch.TorchType.Marigold &&
            torch.IsLit)
        {
            Kill();
        }
    }

    private float GetMoveSpeed()
    {
        return moveSpeed * GetTempoModifier(CurrentTempo).moveSpeedMultiplier;
    }

    private float GetAttackCooldown()
    {
        float effectiveAttackSpeed = attackSpeed * GetTempoModifier(CurrentTempo).attackSpeedMultiplier;
        return 1f / Mathf.Max(0.01f, effectiveAttackSpeed);
    }

    private TempoStatModifier GetTempoModifier(TempoBand tempo)
    {
        TempoStatModifier modifier = tempo switch
        {
            TempoBand.Slow => slowTempo,
            TempoBand.Fast => fastTempo,
            TempoBand.Intense => intenseTempo,
            _ => midTempo
        };

        return modifier ?? new TempoStatModifier();
    }

    private void UpdateAnimator(Vector2 velocity)
    {
        if (animator == null)
            return;

        Vector2 moveDirection = velocity.sqrMagnitude > Mathf.Epsilon
            ? velocity.normalized
            : Vector2.zero;

        animator.SetFloat("MoveX", moveDirection.x);
        animator.SetFloat("MoveY", moveDirection.y);
        animator.SetFloat("FaceX", FacingDirection.x);
        animator.SetFloat("FaceY", FacingDirection.y);
        animator.SetBool("IsMoving", velocity.sqrMagnitude > 0.001f);
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
