using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [HideInInspector] public float damage;
    [HideInInspector] public bool isPiercing;

    // 독 페이로드 (PoisonNeedle이 발사 시 설정). 적중 시 EnemyBase.ApplyPoison 호출.
    [HideInInspector] public bool appliesPoison;
    [HideInInspector] public float poisonTickDamage;
    [HideInInspector] public float poisonDuration;
    [HideInInspector] public float poisonTickInterval;
    [HideInInspector] public float poisonReapplyBonus;

    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float hitKnockback = 2f;

    private Rigidbody2D rb;
    private IObjectPool<Projectile> pool;
    private Vector2 direction;
    private float timer;
    private bool isReturned;

    // 관통 시 같은 적 중복 타격 방지
    private readonly HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void OnEnable()
    {
        timer = lifetime;
        isReturned = false;
        hitEnemies.Clear();
        rb.velocity = Vector2.zero;
        appliesPoison = false; // 풀 재사용 시 페이로드 초기화 (발사 시 다시 설정)
    }

    public void SetPool(IObjectPool<Projectile> pool) => this.pool = pool;

    public void Fire(Vector2 dir)
    {
        direction = dir.normalized;
        rb.velocity = direction * speed;
        // 독침 sprite는 +x(오른쪽)가 뾰족 → 진행 방향으로 회전
        float ang = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, ang);
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
            Return();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (isReturned) return;

        // IDamageable 여부로 적 판정 — Enemy 태그 불필요
        var damageable = col.GetComponent<IDamageable>();
        if (damageable == null) return;

        // 관통 시 동일 적 중복 타격 방지
        if (hitEnemies.Contains(col)) return;
        hitEnemies.Add(col);

        damageable.TakeDamage(damage);
        col.GetComponent<Rigidbody2D>()?.AddForce(direction * hitKnockback, ForceMode2D.Impulse);

        if (appliesPoison)
            col.GetComponent<IStatusReceiver>()?.ApplyPoison(poisonTickDamage, poisonDuration, poisonTickInterval, poisonReapplyBonus);

        if (!isPiercing)
            Return();
    }

    void Return()
    {
        if (isReturned) return;
        isReturned = true;
        rb.velocity = Vector2.zero;
        pool?.Release(this);
    }
}
