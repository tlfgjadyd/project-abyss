using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 원거리 공격 적 컴포넌트. 사거리 안에서 일정 주기로 투사체 발사.
/// EnemyAI가 이 컴포넌트의 StopDistance를 보고 추적 거리를 유지한다.
/// </summary>
[RequireComponent(typeof(EnemyBase))]
public class EnemyRangedAttack : MonoBehaviour
{
    [Header("발사")]
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private float damage = 5f;

    [Header("거리")]
    [Tooltip("이 거리 안에 들어오면 발사 시작")]
    [SerializeField] private float attackRange = 7f;
    [Tooltip("적정 거리. 이 거리에서 정지하고 발사한다 (EnemyAI가 참조).")]
    [SerializeField] private float stopDistance = 6f;

    private Transform player;
    private EnemyBase enemy;
    private IObjectPool<EnemyProjectile> pool;
    private float cooldown;

    /// <summary>EnemyAI가 거리 유지에 사용</summary>
    public float StopDistance => stopDistance;

    void Awake()
    {
        enemy = GetComponent<EnemyBase>();

        pool = new ObjectPool<EnemyProjectile>(
            createFunc:      () => {
                var p = Instantiate(projectilePrefab);
                p.SetPool(pool);
                return p;
            },
            actionOnGet:     p => p.gameObject.SetActive(true),
            actionOnRelease: p => p.gameObject.SetActive(false),
            actionOnDestroy: p => Destroy(p.gameObject),
            collectionCheck: false,
            defaultCapacity: 8
        );
    }

    void OnEnable()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        cooldown = attackInterval; // 첫 발사까지 대기
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (player == null || enemy == null) return;
        if (enemy.IsStunned) return;
        if (projectilePrefab == null) return;

        cooldown -= Time.deltaTime;
        if (cooldown > 0f) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > attackRange) return;

        Fire(((Vector2)player.position - (Vector2)transform.position).normalized);
        cooldown = attackInterval;
    }

    void Fire(Vector2 dir)
    {
        var proj = pool.Get();
        proj.transform.position = transform.position;
        proj.damage = damage;
        proj.Fire(dir);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = new Color(1f, 1f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}
