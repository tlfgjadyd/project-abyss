/// <summary>
/// 상태이상(디버프)을 받을 수 있는 대상. 일반 적(EnemyBase)과 보스(BossBase)가 모두 구현.
/// 스킬은 GetComponent&lt;IStatusReceiver&gt;()로 디버프를 부여 → 적·보스 공통 라우팅.
///
/// 보스는 자체 저항 배율을 적용하고 스턴은 면역으로 구현(BossBase 참조).
/// 틱 데미지는 TakeDamage 경유.
/// </summary>
public interface IStatusReceiver : IDamageable
{
    bool IsDead { get; }

    // ── 현재 상태 조회 (시각화용) ──
    /// <summary>스턴 상태 여부 (보스는 항상 false = 면역).</summary>
    bool IsStunned { get; }
    /// <summary>이동속도 배율 (1 미만이면 둔화 중).</summary>
    float MoveSpeedMultiplier { get; }
    /// <summary>받는 피해 배율 (1 초과면 취약 중).</summary>
    float TakeDamageMultiplier { get; }

    /// <summary>독 부여 (갱신형 + 재적용 즉발 보너스).</summary>
    void ApplyPoison(float perTickDamage, float duration, float tickInterval, float reapplyBonusDamage);

    /// <summary>출혈 부여 (누적 스택).</summary>
    void ApplyBleed(float perStackTickDamage, float duration, float tickInterval, int maxStacks);

    /// <summary>이동속도 둔화 (multiplier &lt; 1).</summary>
    void ApplySlow(float multiplier, float duration);

    /// <summary>취약 (받는 피해 배율, multiplier &gt; 1).</summary>
    void ApplyVulnerability(float multiplier, float duration);

    /// <summary>스턴 (보스는 면역).</summary>
    void Stun(float duration);
}
