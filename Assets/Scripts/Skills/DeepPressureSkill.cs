using UnityEngine;

/// <summary>
/// 심해 압박 — 주변 적 이동속도 감소 + 받는 피해 증가 디버프 (80E).
/// 2스테이지 향유고래 보스 카피 스킬.
/// </summary>
public class DeepPressureSkill : CopySkillBase
{
    [Header("Stats")]
    [SerializeField] private float radius = 5f;
    [SerializeField] private float duration = 4f;
    [Tooltip("이동속도 배율. 0.5 = -50%")]
    [Range(0f, 1f)]
    [SerializeField] private float slowMultiplier = 0.5f;
    [Tooltip("받는 피해 배율. 1.5 = +50%")]
    [Min(1f)]
    [SerializeField] private float vulnerabilityMultiplier = 1.5f;

    [Header("Layer")]
    [SerializeField] private LayerMask enemyLayer;

    public override void Execute()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<EnemyBase>();
            if (enemy == null) continue;
            enemy.ApplySlow(slowMultiplier, duration);
            enemy.ApplyVulnerability(vulnerabilityMultiplier, duration);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.2f, 0.8f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
