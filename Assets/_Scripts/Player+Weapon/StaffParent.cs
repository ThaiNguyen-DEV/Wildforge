using System.Collections;
using UnityEngine;

public class StaffParent : WeaponBase
{
    [Header("Staff Specifics")]
    [SerializeField]
    private GameObject projectilePrefab;

    public override void Attack()
    {
        if (attackBlocked) return;

        IsAttacking = true;
        attackBlocked = true;

        // Fire in the direction the weapon is already pointing
        FireProjectile(transform.right);

        StartCoroutine(DelayAttack());
    }

    private IEnumerator DelayAttack()
    {
        yield return new WaitForSeconds(delay);
        attackBlocked = false;
        IsAttacking = false; // Reset attacking state after cooldown
    }

    private void FireProjectile(Vector2 direction)
    {
        if (projectilePrefab == null) return;

        GameObject projectileInstance = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        projectileInstance.GetComponent<Projectile>()?.Launch(direction, transform.parent.gameObject);
    }
}