using System.Collections;
using UnityEngine;

public class SwordParent : WeaponBase
{
    [Header("Sword Specifics")]
    public Animator animator;
    public Transform circleOrigin;
    public float radius;

    public override void Attack()
    {
        if (attackBlocked) return;

        animator.SetTrigger("Attack");
        IsAttacking = true;
        attackBlocked = true;
        StartCoroutine(AttackCooldownRoutine());
    }

    private IEnumerator AttackCooldownRoutine()
    {
        // This coroutine simply prevents the AI from spamming the attack trigger.
        // It does NOT reset the IsAttacking flag.
        yield return new WaitForSeconds(delay);
        attackBlocked = false;
    }

    // This method MUST be called by an animation event at the end of the swing.
    public override void ResetAttack()
    {
        IsAttacking = false;
    }

    // This method MUST be called by an animation event during the swing.
    public void DetectColliders()
    {
        foreach (Collider2D collider in Physics2D.OverlapCircleAll(circleOrigin.position, radius))
        {
            if (collider.gameObject == transform.parent.gameObject)
            {
                continue;
            }

            if (collider.TryGetComponent<Health>(out var health))
            {
                health.GetHit(1, transform.parent.gameObject);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 position = circleOrigin == null ? Vector3.zero : circleOrigin.position;
        Gizmos.DrawWireSphere(position, radius);
    }
}
