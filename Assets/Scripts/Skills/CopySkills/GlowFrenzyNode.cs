using UnityEngine;

/// <summary>
/// GlowFrenzy 발광 노드 인스턴스. 적이 트리거에 닿으면 폭발 → 광역 피해 + 넉백.
/// 일정 시간 후 트리거 없이도 자동 소멸.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class GlowFrenzyNode : MonoBehaviour
{
    private float damage;
    private float explosionRadius;
    private float knockback;
    private float lifetime;
    private LayerMask enemyLayer;
    private bool exploded;
    private float age;

    public void Initialize(float damage, float explosionRadius, float knockback, float lifetime, LayerMask enemyLayer)
    {
        this.damage          = damage;
        this.explosionRadius = explosionRadius;
        this.knockback       = knockback;
        this.lifetime        = lifetime;
        this.enemyLayer      = enemyLayer;
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age >= lifetime)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (exploded) return;
        if (((1 << other.gameObject.layer) & enemyLayer) == 0) return;

        Explode();
    }

    void Explode()
    {
        exploded = true;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);
        foreach (var c in hits)
        {
            c.GetComponent<IDamageable>()?.TakeDamage(damage);

            var rb = c.attachedRigidbody;
            if (rb != null)
            {
                Vector2 dir = ((Vector2)c.transform.position - (Vector2)transform.position).normalized;
                rb.AddForce(dir * knockback, ForceMode2D.Impulse);
            }
        }

        Destroy(gameObject);
    }
}
