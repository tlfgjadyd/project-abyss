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
    private HitEffect hitEffect;
    private Coroutine stunCoroutine;
    private Coroutine slowCoroutine;
    private Coroutine vulnerableCoroutine;

    public System.Action<EnemyBase> OnDeath;

    void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        hitEffect = GetComponent<HitEffect>();
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
    }

    void Die()
    {
        isDead = true;
        IsStunned = false;

        ExpOrbPool.Instance?.Spawn(transform.position, data.expAmount);
        BioEnergyManager.Instance?.AddEnergy(data.energyDrop);

        OnDeath?.Invoke(this);
        gameObject.SetActive(false);
    }
}
