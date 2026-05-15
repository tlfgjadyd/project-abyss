using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 포식 충돌 — FaceDirection으로 긴 직선 돌진, 돌진 중 무적 + 경로상 적 대피해 + 스턴 (90E).
/// 2스테이지 향유고래 보스 카피 스킬. DashSkill의 강화 버전.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PredatorChargeSkill : CopySkillBase
{
    [Header("Charge")]
    [SerializeField] private float chargeSpeed = 25f;
    [SerializeField] private float duration    = 0.4f;
    [Tooltip("경로상 적 피해 검출 반경")]
    [SerializeField] private float hitRadius = 1.5f;

    [Header("Damage")]
    [Tooltip("기본 공격력 대비 배율")]
    [SerializeField] private float damageMultiplier = 3f;
    [SerializeField] private float stunDuration = 1f;

    [Header("Layer")]
    [SerializeField] private LayerMask enemyLayer;

    private Rigidbody2D rb;
    private bool isCharging;
    private readonly HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
    }

    public override bool CanExecute() => !isCharging;

    public override void Execute()
    {
        StartCoroutine(ChargeRoutine());
    }

    IEnumerator ChargeRoutine()
    {
        isCharging = true;
        controller.IsDashing = true;
        stats.IsInvincible   = true;
        hitEnemies.Clear();

        rb.velocity = controller.FaceDirection * chargeSpeed;

        float t = 0f;
        while (t < duration)
        {
            // 경로상 적 피해 (중복 타격 방지)
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, hitRadius, enemyLayer);
            float damage = stats.attackPower * damageMultiplier;

            foreach (var hit in hits)
            {
                if (hitEnemies.Contains(hit)) continue;
                hitEnemies.Add(hit);
                hit.GetComponent<IDamageable>()?.TakeDamage(damage);
                hit.GetComponent<EnemyBase>()?.Stun(stunDuration);
            }

            t += Time.deltaTime;
            yield return null;
        }

        rb.velocity          = Vector2.zero;
        controller.IsDashing = false;
        stats.IsInvincible   = false;
        isCharging           = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.8f, 0.2f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, hitRadius);
    }
}
