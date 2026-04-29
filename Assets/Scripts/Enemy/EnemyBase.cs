using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBase : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyData data;
    [SerializeField] private GameObject expOrbPrefab;

    public EnemyData Data => data;

    private float currentHp;
    private float contactTimer;
    private bool isDead;

    public System.Action<EnemyBase> OnDeath;

    void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void OnEnable()
    {
        currentHp = data.maxHp;
        isDead = false;
        contactTimer = 0f;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHp -= amount;
        if (currentHp <= 0f)
            Die();
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

        if (expOrbPrefab != null)
        {
            var orb = Instantiate(expOrbPrefab, transform.position, Quaternion.identity);
            orb.GetComponent<ExpOrb>().expAmount = data.expAmount;
        }

        OnDeath?.Invoke(this);
        gameObject.SetActive(false);
    }
}
