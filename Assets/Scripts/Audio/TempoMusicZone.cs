using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class TempoMusicZone : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private TempoMusicSet musicSet;
    [SerializeField, Min(0f)] private float transitionDuration = 1f;
    [SerializeField] private int priority;

    private readonly HashSet<int> overlappingPlayerColliderIds = new();

    public TempoMusicSet MusicSet => musicSet;
    public float TransitionDuration => transitionDuration;
    public int Priority => priority;

    private void Awake()
    {
        EnsureTriggerCollider();
    }

    private void Reset()
    {
        EnsureTriggerCollider();
    }

    private void OnValidate()
    {
        EnsureTriggerCollider();
    }

    private void OnDisable()
    {
        if (overlappingPlayerColliderIds.Count > 0)
            TempoMusicDirector.Instance?.ExitZone(this);

        overlappingPlayerColliderIds.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        UpdatePlayerOverlap(other, isEntering: true);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        UpdatePlayerOverlap(other, isEntering: true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        UpdatePlayerOverlap(other, isEntering: false);
    }

    private void UpdatePlayerOverlap(Collider2D other, bool isEntering)
    {
        if (!TryGetPlayer(other, out _))
            return;

        int colliderId = other.GetInstanceID();

        if (isEntering)
        {
            if (!overlappingPlayerColliderIds.Add(colliderId))
                return;

            if (overlappingPlayerColliderIds.Count == 1)
                TempoMusicDirector.Instance?.EnterZone(this);

            return;
        }

        if (!overlappingPlayerColliderIds.Remove(colliderId))
            return;

        if (overlappingPlayerColliderIds.Count == 0)
            TempoMusicDirector.Instance?.ExitZone(this);
    }

    private void EnsureTriggerCollider()
    {
        Collider2D collider2D = GetComponent<Collider2D>();
        if (collider2D != null)
            collider2D.isTrigger = true;
    }

    private static bool TryGetPlayer(Collider2D other, out PlayerController player)
    {
        player = other.GetComponent<PlayerController>();
        if (player != null)
            return true;

        player = other.GetComponentInParent<PlayerController>();
        return player != null;
    }
}
