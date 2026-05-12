using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBase : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyData data;

    public EnemyData Data => data;
    public bool IsStunned { get; private set; }

    private float currentHp;
    private float scaledMaxHp;          // 난이도 스케일 적용 후 최대 HP
    private float scaledContactDamage;  // 난이도 스케일 적용 후 접촉 피해
    private float contactTimer;
    private bool isDead;
    private HitEffect hitEffect;
    private Coroutine stunCoroutine;

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
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHp -= amount;
        hitEffect?.PlayFlash();

        if (currentHp <= 0f)
            Die();
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
