using UnityEngine;

public interface IKnockbackable
{
    void ApplyKnockback(Vector2 direction, float strengthMultiplier = 1f);
    void ApplyKnockbackFrom(Vector2 sourcePosition, float strengthMultiplier = 1f);
}
