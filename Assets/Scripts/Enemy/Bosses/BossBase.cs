using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossBase : MonoBehaviour, IStatusReceiver
{
    [SerializeField] private BossData data;
    public BossData Data => data;

    [Header("디버프 저항")]
    [Tooltip("보스가 받는 디버프 저항 (0=완전 적용, 1=완전 면역). DoT 데미지·둔화·취약 효과를 이 비율만큼 약화. 스턴은 항상 면역.")]
    [Range(0f, 1f)]
    [SerializeField] private float debuffResistance = 0.5f;

    public float CurrentHp { get; private set; }
    public float MaxHp     => data.maxHp;
    public bool  IsPhase2  { get; private set; }
    public bool  IsDead    { get; private set; }

    /// <summary>이동속도 배율 (둔화 디버프). BossAI가 이 값을 곱해 사용.</summary>
    public float MoveSpeedMultiplier { get; private set; } = 1f;
    /// <summary>받는 피해 배율 (취약 디버프). TakeDamage 내부에서 곱연산.</summary>
    public float TakeDamageMultiplier { get; private set; } = 1f;
    /// <summary>보스는 스턴 면역 → 항상 false (IStatusReceiver 시각화 조회용).</summary>
    public bool IsStunned => false;

    private float contactTimer;
    private HitEffect hitEffect;
    private EnemyStatusEffects status;   // 독/출혈 DoT (Awake에서 자동 부착, 일반 적과 공유 컴포넌트)
    private Coroutine slowCoroutine;
    private Coroutine vulnerableCoroutine;

    public System.Action<BossBase>  OnBossDeath;
    public System.Action<float, float> OnHpChanged; // current, max
    public System.Action            OnPhase2Entered;
    /// <summary>HP가 0.25 비율 이하로 처음 떨어졌을 때 1회 발행 (연구소장 보호막 등)</summary>
    public System.Action            OnHpQuarterReached;

    private bool quarterReached;

    void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale  = 0f;
        rb.freezeRotation = true;
        hitEffect = GetComponent<HitEffect>();

        // 통합 상태이상(독/출혈) 컴포넌트 자동 부착 — 일반 적과 동일 시스템 재사용
        status = GetComponent<EnemyStatusEffects>();
        if (status == null) status = gameObject.AddComponent<EnemyStatusEffects>();
    }

    void OnEnable()
    {
        CurrentHp     = data.maxHp;
        IsDead        = false;
        IsPhase2      = false;
        contactTimer  = 0f;
        quarterReached = false;
        IsInvincible = false;

        MoveSpeedMultiplier  = 1f;
        TakeDamageMultiplier = 1f;
        slowCoroutine        = null;
        vulnerableCoroutine  = null;
    }

    /// <summary>외부(보호막 등)에서 일시 설정. true일 때 TakeDamage 무효화.</summary>
    public bool IsInvincible { get; set; }

    // ── IDamageable ──────────────────────────────

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        if (IsInvincible) return;

        CurrentHp = Mathf.Max(CurrentHp - amount * TakeDamageMultiplier, 0f);
        hitEffect?.PlayFlash();
        OnHpChanged?.Invoke(CurrentHp, data.maxHp);

        // 페이즈 2 전환
        if (!IsPhase2 && CurrentHp / data.maxHp <= data.phase2HpRatio)
        {
            IsPhase2 = true;
            OnPhase2Entered?.Invoke();
            AudioManager.Instance?.PlaySFX(SfxId.BossPhase2);
            // 카메라 효과 제거 (어지러움 + 마우스 조준 방해) — 사운드만 유지
            Debug.Log($"[Boss] {data.bossName} 페이즈 2 돌입!");
        }

        // HP 25% 임계값 (1회) — 연구소장 보호막 등에서 구독
        if (!quarterReached && CurrentHp / data.maxHp <= 0.25f && CurrentHp > 0f)
        {
            quarterReached = true;
            OnHpQuarterReached?.Invoke();
            Debug.Log($"[Boss] {data.bossName} HP 25% 도달!");
        }

        if (CurrentHp <= 0f)
            Die();
    }

    // ── IStatusReceiver (디버프 + 저항) ───────────
    // 저항 비율만큼 효과를 약화. 스턴은 항상 면역.

    /// <summary>독 — 틱·재적용 보너스 데미지에 저항 적용.</summary>
    public void ApplyPoison(float perTickDamage, float duration, float tickInterval, float reapplyBonusDamage)
    {
        if (IsDead) return;
        float keep = 1f - debuffResistance;
        status?.ApplyPoison(perTickDamage * keep, duration, tickInterval, reapplyBonusDamage * keep);
    }

    /// <summary>출혈 — 스택당 틱 데미지에 저항 적용.</summary>
    public void ApplyBleed(float perStackTickDamage, float duration, float tickInterval, int maxStacks)
    {
        if (IsDead) return;
        float keep = 1f - debuffResistance;
        status?.ApplyBleed(perStackTickDamage * keep, duration, tickInterval, maxStacks);
    }

    /// <summary>둔화 — 감속폭을 저항만큼 약화 (multiplier 0.7, 저항 0.5 → 실효 0.85).</summary>
    public void ApplySlow(float multiplier, float duration)
    {
        if (IsDead) return;
        float effective = 1f - (1f - multiplier) * (1f - debuffResistance);
        if (slowCoroutine != null) StopCoroutine(slowCoroutine);
        slowCoroutine = StartCoroutine(SlowRoutine(effective, duration));
    }

    IEnumerator SlowRoutine(float multiplier, float duration)
    {
        MoveSpeedMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        MoveSpeedMultiplier = 1f;
        slowCoroutine = null;
    }

    /// <summary>취약 — 증가폭을 저항만큼 약화 (multiplier 1.5, 저항 0.5 → 실효 1.25).</summary>
    public void ApplyVulnerability(float multiplier, float duration)
    {
        if (IsDead) return;
        float effective = 1f + (multiplier - 1f) * (1f - debuffResistance);
        if (vulnerableCoroutine != null) StopCoroutine(vulnerableCoroutine);
        vulnerableCoroutine = StartCoroutine(VulnerabilityRoutine(effective, duration));
    }

    IEnumerator VulnerabilityRoutine(float multiplier, float duration)
    {
        TakeDamageMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        TakeDamageMultiplier = 1f;
        vulnerableCoroutine = null;
    }

    /// <summary>스턴 — 보스는 면역 (무시).</summary>
    public void Stun(float duration) { /* 보스 스턴 면역 */ }

    // ── 접촉 데미지 ──────────────────────────────

    void OnCollisionStay2D(Collision2D col)
    {
        if (IsDead) return;

        contactTimer -= Time.deltaTime;
        if (contactTimer > 0f) return;

        var stats = col.gameObject.GetComponent<PlayerStats>();
        if (stats == null) return;

        stats.TakeDamage(data.contactDamage);
        contactTimer = data.contactCooldown;
    }

    // ── 사망 ─────────────────────────────────────

    void Die()
    {
        IsDead = true;
        OnBossDeath?.Invoke(this);
        gameObject.SetActive(false);
    }
}
