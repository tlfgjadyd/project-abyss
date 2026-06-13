using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 적이 발사하는 투사체. PlayerStats를 직접 찾아 데미지 적용.
/// (플레이어 측 Projectile은 IDamageable 검색이라 대상이 달라 별도 분리)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    [HideInInspector] public float damage;

    [SerializeField] private float speed    = 6f;
    [SerializeField] private float lifetime = 4f;

    private Rigidbody2D rb;
    private IObjectPool<EnemyProjectile> pool;
    private float timer;
    private bool  isReturned;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale  = 0f;
        rb.freezeRotation = true;
    }

    void OnEnable()
    {
        timer = lifetime;
        isReturned = false;
        rb.velocity = Vector2.zero;
    }

    public void SetPool(IObjectPool<EnemyProjectile> p) => pool = p;

    public void Fire(Vector2 dir)
    {
        Vector2 n = dir.normalized;
        rb.velocity = n * speed;
        // 탄 sprite는 +x(가로)로 긴 타원 → 진행 방향으로 회전 (위/아래로 쏠 때 눕지 않도록)
        float ang = Mathf.Atan2(n.y, n.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, ang);
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        timer -= Time.deltaTime;
        if (timer <= 0f) Return();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (isReturned) return;

        var stats = col.GetComponent<PlayerStats>();
        if (stats == null) return;

        stats.TakeDamage(damage);
        Return();
    }

    void Return()
    {
        if (isReturned) return;
        isReturned = true;
        rb.velocity = Vector2.zero;
        if (pool != null) pool.Release(this);
        else gameObject.SetActive(false);
    }
}
