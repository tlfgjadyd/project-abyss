using UnityEngine;
using UnityEngine.Pool;

public class PoisonNeedle : MonoBehaviour
{
    [Header("Stats")]
    public float detectionRadius = 6f;
    public float cooldown = 1.5f;
    public int projectileCount = 1;     // Lv4: 3
    public bool isPiercing = false;     // Lv4

    [Header("Refs")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private LayerMask enemyLayer;

    private PlayerStats stats;
    private IObjectPool<Projectile> pool;
    private float cooldownTimer;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();

        pool = new ObjectPool<Projectile>(
            createFunc:      () => { var p = Instantiate(projectilePrefab); p.SetPool(pool); return p; },
            actionOnGet:     p => p.gameObject.SetActive(true),
            actionOnRelease: p => p.gameObject.SetActive(false),
            actionOnDestroy: p => Destroy(p.gameObject),
            collectionCheck: false,
            defaultCapacity: 20
        );
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer > 0f) return;

        Transform target = FindNearestEnemy();
        if (target == null) return;

        Fire(target);
        cooldownTimer = cooldown / stats.attackSpeed;
    }

    Transform FindNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < minDist) { minDist = dist; nearest = hit.transform; }
        }
        return nearest;
    }

    void Fire(Transform target)
    {
        Vector2 baseDir = ((Vector2)target.position - (Vector2)transform.position).normalized;

        if (projectileCount <= 1)
        {
            SpawnProjectile(baseDir);
            return;
        }

        // 여러 발: 부채꼴로 퍼짐 (간격 20도)
        float spreadAngle = 20f;
        float startAngle = -spreadAngle * (projectileCount - 1) * 0.5f;

        for (int i = 0; i < projectileCount; i++)
            SpawnProjectile(Rotate(baseDir, startAngle + spreadAngle * i));
    }

    void SpawnProjectile(Vector2 direction)
    {
        var proj = pool.Get();
        proj.transform.position = transform.position;
        proj.damage = stats.attackPower;
        proj.isPiercing = isPiercing;
        proj.Fire(direction);
    }

    static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
