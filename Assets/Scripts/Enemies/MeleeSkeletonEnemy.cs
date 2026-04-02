using UnityEngine;

public class MeleeSkeletonEnemy : SkeletonEnemyBase
{
    protected override bool PerformAttack(Vector2 attackDirection, float distanceToTarget)
    {
        if (!TryGetDamageableTarget(out IDamageable damageable))
            return false;

        damageable.ReceiveDamage(EffectiveDamage, attackDirection, gameObject);
        return true;
    }
}
