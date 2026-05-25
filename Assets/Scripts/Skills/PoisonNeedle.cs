using UnityEngine;
using UnityEngine.Pool;

public class PoisonNeedle : MonoBehaviour
{
    [Header("Stats")]
    public float detectionRadius = 6f;
    public float cooldown = 1.5f;
    public int projectileCount = 1;     // Lv4: 3
    public bool isPiercing = false;     // Lv4

    /// <summary>돌연변이 등에서 곱셈으로 적용. 기본 1.0</summary>
    [HideInInspector] public float damageMultiplier = 1f;

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

        if (stats.IsStunned) return;

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer > 0f) return;

        Transform target = FindNearestEnemy();
        if (target == null) return;

        Fire(target);
        cooldownTimer = cooldown / stats.EffectiveAttackSpeed;
        PlayerSkillEvents.OnSkillUsed?.Invoke();
    }

    Transform FindNearestEnemy()
    {
        float effRange = stats != null ? stats.ApplyAutoTrackLimit(detectionRadius) : detectionRadius;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, effRange, enemyLayer);
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
        proj.damage = stats.attackPower * damageMultiplier;
        proj.isPiercing = isPiercing;
        proj.Fire(direction);
        // 발사 시작점 작은 점 시각 (LineRenderer 8각형 mini)
        SpawnMuzzleFx(transform.position);
    }

    void SpawnMuzzleFx(Vector2 origin)
    {
        var fx = new GameObject("NeedleMuzzleFx");
        fx.transform.position = origin;
        var lr = fx.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = 0.06f; lr.endWidth = 0.06f;
        var color = new Color(0.6f, 1f, 0.3f, 1f);
        lr.startColor = color; lr.endColor = color;
        const int seg = 8; const float r = 0.18f;
        lr.positionCount = seg + 1;
        for (int i = 0; i <= seg; i++)
        {
            float a = i / (float)seg * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f));
        }
        fx.AddComponent<SkillFxFader>().Init(lr, color, 0.12f);
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
