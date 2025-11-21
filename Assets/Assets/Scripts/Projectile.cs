using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 18.3f;       // Speed of the projectile
    public float lifetime = 5f;       // Max time before auto-destroy
    public int damage = 10;           // Damage dealt to the player
    public LayerMask hitMask;         // Layers projectile can hit (player, walls, etc.)

    private Vector3 targetPosition;   // Target position to move toward
    private bool isLaunched = false;

    void Update()
    {
        if (!isLaunched) return;

        // Move toward target
        Vector3 direction = (targetPosition - transform.position).normalized;
        float step = speed * Time.deltaTime;

        // Check if reached target
        if (Vector3.Distance(transform.position, targetPosition) <= step)
        {
            HitTarget();
            return;
        }

        transform.position += direction * step;
        transform.LookAt(targetPosition); // Optional: orient projectile
    }

    public void Launch(Vector3 targetPos)
    {
        targetPosition = targetPos;
        isLaunched = true;
        Destroy(gameObject, lifetime); // Auto-destroy after lifetime
    }

    private void HitTarget()
    {
        // Detect collision using SphereCast or overlap check
        Collider[] hits = Physics.OverlapSphere(transform.position, 0.2f, hitMask);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                // Apply damage
                Health health = hit.GetComponent<Health>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                }
            }
        }

        Destroy(gameObject); // Destroy projectile on impact
    }
}
