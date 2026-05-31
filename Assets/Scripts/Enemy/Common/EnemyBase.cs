using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBase : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyData data;

    public EnemyData Data => data;
    public bool IsStunned { get; private set; }

    /// <summary>이동속도 배율 (DeepPressure 등 슬로우 디버프). EnemyAI가 이 값을 곱해 사용.</summary>
    public float MoveSpeedMultiplier { get; private set; } = 1f;

    /// <summary>받는 피해 배율 (DeepPressure 등 취약 디버프). TakeDamage 내부에서 곱연산.</summary>
    public float TakeDamageMultiplier { get; private set; } = 1f;

    private float currentHp;
    private float scaledMaxHp;          // 난이도 스케일 적용 후 최대 HP
    private float scaledContactDamage;  // 난이도 스케일 적용 후 접촉 피해
    private float contactTimer;
    private bool isDead;
    /// <summary>죽음 직후 Death 애니 재생 중. EnemyAI가 추적 중단해야 함.</summary>
    public bool IsDead => isDead;
    private HitEffect hitEffect;
    private Animator animator;            // 있으면 Hurt/Attack/Death 트리거 호출
    private Coroutine stunCoroutine;
    private Coroutine slowCoroutine;
    private Coroutine vulnerableCoroutine;

    // Animator 파라미터 해시 (있는 컨트롤러만 반응 — 없는 파라미터는 무시됨)
    static readonly int HashHurt   = Animator.StringToHash("Hurt");
    static readonly int HashAttack = Animator.StringToHash("Attack");
    static readonly int HashDeath  = Animator.StringToHash("Death");

    /// <summary>넉백 종료 시각 (Time.time 기준). EnemyAI가 velocity 덮어쓰기 양보.</summary>
    public float KnockbackUntil { get; private set; }
    public bool IsKnockedBack => Time.time < KnockbackUntil;

    public System.Action<EnemyBase> OnDeath;

    void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        hitEffect = GetComponent<HitEffect>();
        animator = GetComponent<Animator>();
    }

    /// <summary>Animator 트리거를 안전하게 호출 (해당 파라미터가 컨트롤러에 없으면 무시).</summary>
    void TriggerAnim(int hash)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return;
        foreach (var p in animator.parameters)
        {
            if (p.nameHash == hash && p.type == AnimatorControllerParameterType.Trigger)
            {
                animator.SetTrigger(hash);
                return;
            }
        }
    }

    void OnEnable()
    {
        // 스폰 시점의 난이도 스케일을 적용 (이후 이 적의 수치는 고정)
        float hpScale     = DifficultyManager.Instance != null ? DifficultyManager.Instance.CurrentHpScale     : 1f;
        float damageScale = DifficultyManager.Instance != null ? DifficultyManager.Instance.CurrentDamageScale : 1f;

        scaledMaxHp         = data.maxHp        * hpScale;
        scaledContactDamage = data.contactDamage * damageScale;

        currentHp = scaledMaxHp;
        isDead = false;
        contactTimer = 0f;
        IsStunned = false;
        stunCoroutine = null;

        // 풀에서 재사용 시 디버프 초기화
        MoveSpeedMultiplier = 1f;
        TakeDamageMultiplier = 1f;
        slowCoroutine = null;
        vulnerableCoroutine = null;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHp -= amount * TakeDamageMultiplier;
        hitEffect?.PlayFlash();

        if (currentHp <= 0f)
            Die();
        else
            TriggerAnim(HashHurt);
    }

    /// <summary>이동속도 디버프 적용 (DeepPressure 등). multiplier=1보다 작으면 슬로우.</summary>
    public void ApplySlow(float multiplier, float duration)
    {
        if (isDead) return;
        if (slowCoroutine != null) StopCoroutine(slowCoroutine);
        slowCoroutine = StartCoroutine(SlowRoutine(multiplier, duration));
    }

    IEnumerator SlowRoutine(float multiplier, float duration)
    {
        MoveSpeedMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        MoveSpeedMultiplier = 1f;
        slowCoroutine = null;
    }

    /// <summary>취약 디버프 적용 (받는 피해 증가). multiplier=1.5이면 받는 피해 +50%.</summary>
    public void ApplyVulnerability(float multiplier, float duration)
    {
        if (isDead) return;
        if (vulnerableCoroutine != null) StopCoroutine(vulnerableCoroutine);
        vulnerableCoroutine = StartCoroutine(VulnerabilityRoutine(multiplier, duration));
    }

    IEnumerator VulnerabilityRoutine(float multiplier, float duration)
    {
        TakeDamageMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        TakeDamageMultiplier = 1f;
        vulnerableCoroutine = null;
    }

    /// <summary>넉백 적용 — Rigidbody2D에 즉시 force + 일정 시간 EnemyAI 양보.</summary>
    public void ApplyKnockback(Vector2 dir, float force, float duration = 0.25f)
    {
        if (isDead) return;
        var rb = GetComponent<Rigidbody2D>();
        if (rb == null) return;
        rb.velocity = Vector2.zero;
        rb.AddForce(dir.normalized * force, ForceMode2D.Impulse);
        KnockbackUntil = Time.time + duration;
    }

    public void Stun(float duration)
    {
        if (isDead) return;
        if (stunCoroutine != null) StopCoroutine(stunCoroutine);
        stunCoroutine = StartCoroutine(StunRoutine(duration));
    }

    IEnumerator StunRoutine(float duration)
    {
        IsStunned = true;
        yield return new WaitForSeconds(duration);
        IsStunned = false;
        stunCoroutine = null;
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (isDead) return;

        contactTimer -= Time.deltaTime;
        if (contactTimer > 0f) return;

        var stats = col.gameObject.GetComponent<PlayerStats>();
        if (stats == null) return;

        stats.TakeDamage(scaledContactDamage);
        contactTimer = data.contactCooldown;
        TriggerAnim(HashAttack);
    }

    void Die()
    {
        isDead = true;
        IsStunned = false;

        ExpOrbPool.Instance?.Spawn(transform.position, data.expAmount);
        BioEnergyManager.Instance?.AddEnergy(data.energyDrop);
        AudioManager.Instance?.PlaySFX(SfxId.EnemyDeath);

        // 사망 폭발 시각 — Sprite 색상 기반
        var sr = GetComponentInChildren<SpriteRenderer>();
        Color c = sr != null ? sr.color : new Color(1f, 0.6f, 0.3f, 1f);
        DeathExplosion.Spawn(transform.position, c, 0.6f);

        OnDeath?.Invoke(this);

        // Death 애니메이션이 있으면 잠깐 보여주고 비활성화. 없으면 즉시.
        if (HasAnimParam(HashDeath))
        {
            // 충돌/AI 정지 위해 콜라이더 비활성, velocity 0
            var rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;
            foreach (var col in GetComponents<Collider2D>()) col.enabled = false;
            TriggerAnim(HashDeath);
            StartCoroutine(DeactivateAfter(0.6f));
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    bool HasAnimParam(int hash)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return false;
        foreach (var p in animator.parameters)
            if (p.nameHash == hash && p.type == AnimatorControllerParameterType.Trigger) return true;
        return false;
    }

    IEnumerator DeactivateAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        // 다음 OnEnable에서 collider 재활성
        foreach (var col in GetComponents<Collider2D>()) col.enabled = true;
        gameObject.SetActive(false);
    }
}
