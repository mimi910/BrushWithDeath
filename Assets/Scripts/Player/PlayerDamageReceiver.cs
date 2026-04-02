using System;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class PlayerDamageReceiver : MonoBehaviour, IDamageable
{
    [Serializable]
    public class DamageEvent : UnityEvent<float> { }

    [SerializeField] private PlayerController playerController;
    [SerializeField, Min(0f)] private float invulnerabilityDuration = 0.2f;
    [SerializeField] private bool interruptTempoOnDamage = true;
    [SerializeField] private bool logDamage = true;
    [SerializeField] private UnityEvent onDamaged;
    [SerializeField] private DamageEvent onDamageTaken;

    public float LastDamageReceived { get; private set; }
    public Vector2 LastHitDirection { get; private set; }
    public GameObject LastSource { get; private set; }

    private float nextDamageTime;

    private void Reset()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
    }

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
    }

    public void ReceiveDamage(float damage, Vector2 hitDirection, GameObject source)
    {
        if (Time.time < nextDamageTime)
            return;

        nextDamageTime = Time.time + invulnerabilityDuration;
        LastDamageReceived = damage;
        LastHitDirection = hitDirection;
        LastSource = source;

        if (interruptTempoOnDamage && playerController != null)
            playerController.InterruptTempoFocus(allowGraceCompletion: false);

        if (logDamage)
        {
            string sourceName = source != null ? source.name : "Unknown";
            Debug.Log($"Player took {damage:0.##} damage from {sourceName}.", this);
        }

        onDamageTaken?.Invoke(damage);
        onDamaged?.Invoke();
    }
}
