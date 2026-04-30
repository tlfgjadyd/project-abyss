using UnityEngine;

public class BioticExplosion : MonoBehaviour
{
    [Header("Stats")]
    public float range = 2.5f;
    public float cooldown = 4f;
    public float stunDuration = 0.5f;   // Lv4: stunDurationDelta로 증가

    [Header("Layer")]
    [SerializeField] private LayerMask enemyLayer;

    private PlayerStats stats;
    private float cooldownTimer;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer > 0f) return;

        Explode();
        cooldownTimer = cooldown / stats.attackSpeed;
    }

    void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
        foreach (var hit in hits)
        {
            hit.GetComponent<IDamageable>()?.TakeDamage(stats.attackPower);
            if (stunDuration > 0f)
                hit.GetComponent<EnemyBase>()?.Stun(stunDuration);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.5f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
