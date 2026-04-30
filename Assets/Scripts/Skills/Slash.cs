using UnityEngine;

public class Slash : MonoBehaviour
{
    [Header("Stats")]
    public float range = 3f;
    [Range(0f, 360f)] public float angle = 100f;

    [Header("Knockback (Lv4)")]
    public bool knockbackEnabled;
    [SerializeField] private float knockbackForce = 5f;

    [Header("Layer")]
    [SerializeField] private LayerMask enemyLayer;

    public void Execute(Vector2 direction, float damage)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
        foreach (var hit in hits)
        {
            Vector2 toEnemy = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
            if (Vector2.Angle(direction, toEnemy) > angle * 0.5f) continue;

            hit.GetComponent<IDamageable>()?.TakeDamage(damage);

            if (knockbackEnabled)
                hit.GetComponent<Rigidbody2D>()?.AddForce(toEnemy * knockbackForce, ForceMode2D.Impulse);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
