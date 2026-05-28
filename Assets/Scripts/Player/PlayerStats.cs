using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public float maxHp = 100f;
    public float currentHp;
    public float moveSpeed = 5f;
    public float attackPower = 10f;
    public float attackSpeed = 1f;      // 공격 쿨타임 배율 (1 = 기본)
    public float hpRegenPerSecond = 0f; // 세포 재생 패시브로 증가

    [Header("Passive")]
    [Tooltip("자기 유도 패시브 — ExpOrb 흡인 범위 배율. 기본 1f")]
    public float magneticRangeMultiplier = 1f;

    [Header("Stage")]
    [Tooltip("자동 추적 스킬 사거리 상한. 0 = 무제한. StageManager가 씬 시작 시 StageData에서 설정")]
    [HideInInspector] public float autoTrackRangeLimit = 0f;

    /// <summary>스킬에서 사거리 계산 시 사용. limit=0이면 그대로, 아니면 Min</summary>
    public float ApplyAutoTrackLimit(float baseRange)
    {
        return autoTrackRangeLimit > 0f ? Mathf.Min(baseRange, autoTrackRangeLimit) : baseRange;
    }

    // 압력 시스템
    [HideInInspector] public float pressureResistance     = 0f;  // 0 ~ 0.5  (메타 업그레이드)
    [HideInInspector] public float pressureMoveMultiplier   = 1f;  // PressureSystem이 설정
    [HideInInspector] public float pressureAttackMultiplier = 1f;

    // 보스 디버프 (Day 46 향유고래 페이즈2)
    /// <summary>보스 디버프 — 이동속도 배율 (1=정상, 0.5=절반). 보스 컴포넌트가 일시 설정 후 복원.</summary>
    [HideInInspector] public float bossDebuffMoveMultiplier = 1f;
    /// <summary>보스 디버프 — 받는 데미지 배율 (1=정상, 1.5=취약). TakeDamage에서 곱셈.</summary>
    [HideInInspector] public float bossDebuffVulnMultiplier = 1f;

    /// <summary>압력 + 보스 디버프 반영 실제 이동속도</summary>
    public float EffectiveMoveSpeed   => moveSpeed   * pressureMoveMultiplier * bossDebuffMoveMultiplier;
    /// <summary>압력 페널티가 반영된 실제 공격속도</summary>
    public float EffectiveAttackSpeed => attackSpeed * pressureAttackMultiplier;

    // 이벤트
    public System.Action<float, float> OnHpChanged;  // current, max
    public System.Action OnDeath;

    void Awake()
    {
        // 메타 업그레이드 적용 (인스펙터 기본값에 가산/곱셈)
        maxHp              += MetaProgressData.GetMaxHpBonus();
        moveSpeed          *= MetaProgressData.GetMoveSpeedMultiplier();
        attackSpeed        *= MetaProgressData.GetAttackSpeedMultiplier();
        attackPower        *= MetaProgressData.GetAttackPowerMultiplier();
        pressureResistance += MetaProgressData.GetPressureResistanceBonus();

        currentHp = maxHp;
    }

    void Update()
    {
        if (hpRegenPerSecond > 0f &&
            GameManager.Instance.CurrentState == GameManager.GameState.Playing)
        {
            Heal(hpRegenPerSecond * Time.deltaTime);
        }
    }

    // 돌진 무적 등 외부에서 설정
    public bool IsInvincible { get; set; }

    /// <summary>감각 붕괴 돌연변이 등에서 외부 설정. true 시 이동/공격 정지.</summary>
    public bool IsStunned { get; set; }

    /// <summary>HP 회복 차단 플래그. (현재 미사용 — 의태 기관 페널티는 공격력 ×0.5로 변경됨. 향후 디버프용 예약)</summary>
    [HideInInspector] public bool healingBlocked = false;

    /// <summary>발광 기관 패시브 — 활성 시 다음 1회 데미지 무효 후 비활성. GlowOrganSkill이 관리.</summary>
    [HideInInspector] public bool glowShieldActive = false;
    /// <summary>발광 기관 — 보호막 소모 시 호출되는 콜백 (스킬 컴포넌트가 등록)</summary>
    public System.Action OnGlowShieldConsumed;

    public void TakeDamage(float amount)
    {
        if (currentHp <= 0f) return;
        if (IsInvincible) return;

        // 발광 기관 보호막 — 1회 흡수
        if (glowShieldActive)
        {
            glowShieldActive = false;
            OnGlowShieldConsumed?.Invoke();
            return;
        }

        // 보스 디버프 취약: 받는 데미지 배율 적용
        amount *= bossDebuffVulnMultiplier;

        currentHp = Mathf.Clamp(currentHp - amount, 0f, maxHp);
        AudioManager.Instance?.PlaySFX(SfxId.PlayerHit);
        OnHpChanged?.Invoke(currentHp, maxHp);

        if (currentHp <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (healingBlocked) return;
        if (currentHp >= maxHp) return;

        currentHp = Mathf.Clamp(currentHp + amount, 0f, maxHp);
        OnHpChanged?.Invoke(currentHp, maxHp);
    }

    void Die()
    {
        OnDeath?.Invoke();
        GameManager.Instance.TriggerGameOver();
    }
}
