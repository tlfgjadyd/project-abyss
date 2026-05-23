using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 절단 유영 — 1.5초간 초고속 이동 (순간이동급) + 무적. 부딪힌 적에게 출혈 DoT 부여.
/// 3스테이지 산갈치 보스 카피 스킬.
///
/// Day 45 재설계: 4s 가속 + 트레일 → 1.5s 가속 + 무적 + 부딪힌 적 DoT
/// PredatorCharge의 무적 패턴 + BleedSwim 자체 hit list 유지하여 DoT 적용.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class BleedSwimSkill : CopySkillBase
{
    [Header("Swim")]
    [Tooltip("가속 시간")]
    [SerializeField] private float duration = 1.5f;
    [Tooltip("이동속도 배율 (가속 동안)")]
    [SerializeField] private float speedMultiplier = 3.5f;
    [Tooltip("부딪힘 검출 반경 — 매 프레임 OverlapCircleAll")]
    [SerializeField] private float collisionRadius = 0.8f;

    [Header("Bleed DoT")]
    [Tooltip("출혈 지속 시간")]
    [SerializeField] private float bleedDuration = 3f;
    [Tooltip("출혈 틱 간격")]
    [SerializeField] private float bleedTickInterval = 0.3f;
    [Tooltip("1틱 데미지 = attackPower × bleedDamageMultiplier")]
    [SerializeField] private float bleedDamageMultiplier = 0.4f;

    [Header("Layer")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Visual")]
    [SerializeField] private Color bleedColor = new Color(0.9f, 0.1f, 0.3f, 1f);

    private Rigidbody2D rb;
    private bool isSwimming;
    private float baseMoveSpeed;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
    }

    public override bool CanExecute() => !isSwimming;

    public override void Execute()
    {
        if (stats == null) return;
        StartCoroutine(SwimRoutine());
    }

    IEnumerator SwimRoutine()
    {
        isSwimming = true;
        stats.IsInvincible = true;
        baseMoveSpeed = stats.moveSpeed;
        stats.moveSpeed = baseMoveSpeed * speedMultiplier;

        // 중복 부착 방지용 set (이번 가속 동안 1회만 DoT)
        var bled = new HashSet<EnemyBase>();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            // 가속 동안 매 프레임 충돌 검출
            var hits = Physics2D.OverlapCircleAll(transform.position, collisionRadius, enemyLayer);
            foreach (var c in hits)
            {
                if (c == null) continue;
                var eb = c.GetComponent<EnemyBase>();
                if (eb == null || bled.Contains(eb)) continue;
                bled.Add(eb);
                // 출혈 DoT 부착
                StartCoroutine(BleedDoT(eb, stats.attackPower * bleedDamageMultiplier));
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        stats.moveSpeed = baseMoveSpeed;
        stats.IsInvincible = false;
        isSwimming = false;
    }

    IEnumerator BleedDoT(EnemyBase target, float perTickDamage)
    {
        float elapsed = 0f;
        while (elapsed < bleedDuration && target != null && target.gameObject.activeInHierarchy)
        {
            yield return new WaitForSeconds(bleedTickInterval);
            if (target == null || !target.gameObject.activeInHierarchy) yield break;
            (target as IDamageable)?.TakeDamage(perTickDamage);
            elapsed += bleedTickInterval;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.9f, 0.1f, 0.3f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, collisionRadius);
    }
}
