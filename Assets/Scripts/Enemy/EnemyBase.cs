using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBase : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyData data;

    public EnemyData Data => data;
    public bool IsStunned { get; private set; }

    private float currentHp;
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
        currentHp = data.maxHp;
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

        stats.TakeDamage(data.contactDamage);
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
