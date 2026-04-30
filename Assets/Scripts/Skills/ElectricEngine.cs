using UnityEngine;

public class ElectricEngine : MonoBehaviour
{
    [Header("Stats")]
    public float detectionRadius = 5f;
    public float cooldown = 2f;
    public float chainRadius = 2.5f;
    public float chainDamageRatio = 0.6f;
    public int maxChainTargets = 3;

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

        Transform primary = FindNearestEnemy();
        if (primary == null) return;

        Fire(primary);
        cooldownTimer = cooldown / stats.attackSpeed;
    }

    Transform FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < minDist) { minDist = dist; nearest = hit.transform; }
        }
        return nearest;
    }

    void Fire(Transform primary)
    {
        // 1차 피해
        primary.GetComponent<IDamageable>()?.TakeDamage(stats.attackPower);

        // 연쇄 피해 (1차 대상 주변)
        Collider2D[] chainHits = Physics2D.OverlapCircleAll(primary.position, chainRadius, enemyLayer);
        int chainCount = 0;
        float chainDamage = stats.attackPower * chainDamageRatio;

        foreach (var hit in chainHits)
        {
            if (hit.transform == primary) continue;
            if (chainCount >= maxChainTargets) break;

            hit.GetComponent<IDamageable>()?.TakeDamage(chainDamage);
            chainCount++;
        }

        // TODO Week 3: Lv4 — 사망 시 감전 필드 생성
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
