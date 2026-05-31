using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossBase : MonoBehaviour, IDamageable
{
    [SerializeField] private BossData data;
    public BossData Data => data;

    public float CurrentHp { get; private set; }
    public float MaxHp     => data.maxHp;
    public bool  IsPhase2  { get; private set; }
    public bool  IsDead    { get; private set; }

    private float contactTimer;
    private HitEffect hitEffect;

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
    }

    void OnEnable()
    {
        CurrentHp     = data.maxHp;
        IsDead        = false;
        IsPhase2      = false;
        contactTimer  = 0f;
        quarterReached = false;
        IsInvincible = false;
    }

    /// <summary>외부(보호막 등)에서 일시 설정. true일 때 TakeDamage 무효화.</summary>
    public bool IsInvincible { get; set; }

    // ── IDamageable ──────────────────────────────

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        if (IsInvincible) return;

        CurrentHp = Mathf.Max(CurrentHp - amount, 0f);
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
