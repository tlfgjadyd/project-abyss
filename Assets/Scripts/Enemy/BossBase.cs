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
    }

    // ── IDamageable ──────────────────────────────

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        CurrentHp = Mathf.Max(CurrentHp - amount, 0f);
        hitEffect?.PlayFlash();
        OnHpChanged?.Invoke(CurrentHp, data.maxHp);

        // 페이즈 2 전환
        if (!IsPhase2 && CurrentHp / data.maxHp <= data.phase2HpRatio)
        {
            IsPhase2 = true;
            OnPhase2Entered?.Invoke();
            Debug.Log($"[Boss] {data.bossName} 페이즈 2 돌입!");
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
