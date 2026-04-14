using UnityEngine;

public interface IDamageable
{
    void ReceiveDamage(float damage, Vector2 hitDirection, GameObject source);
}
