using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
[RequireComponent(typeof(Slash))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private LayerMask enemyLayer;

    private PlayerStats stats;
    private Slash slash;
    private float cooldownTimer;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
        slash = GetComponent<Slash>();
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer > 0f)
            return;

        Transform target = FindNearestEnemy();
        if (target == null)
            return;

        Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
        slash.Execute(dir, stats.attackPower);
        cooldownTimer = 1f / stats.EffectiveAttackSpeed;
    }

    Transform FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = hit.transform;
            }
        }
        return nearest;
    }

    /// <summary>과성장 촉수 돌연변이 — 감지 범위에 배율 적용</summary>
    public void ScaleDetectionRadius(float multiplier) => detectionRadius *= multiplier;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
