using UnityEngine;

public class Slash : MonoBehaviour
{
    [Header("Stats")]
    public float range = 3f;
    [Range(0f, 360f)] public float angle = 100f;

    [Header("Layer")]
    [SerializeField] private LayerMask enemyLayer;

    public void Execute(Vector2 direction, float damage)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
        foreach (var hit in hits)
        {
            Vector2 toEnemy = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
            if (Vector2.Angle(direction, toEnemy) <= angle * 0.5f)
                hit.GetComponent<IDamageable>()?.TakeDamage(damage);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
