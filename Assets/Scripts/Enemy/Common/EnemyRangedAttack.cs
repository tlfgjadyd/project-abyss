using System.Collections;
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

    [Header("조준 단계 (시야 밖 위치 단서)")]
    [Tooltip("발사 직전 SpriteRenderer 점멸 시간. 시야 제한 스테이지에서 회피 단서 역할.")]
    [SerializeField] private float aimDuration = 0.3f;
    [Tooltip("점멸 시 보일 색상 (보통 노랑/주황 계열).")]
    [SerializeField] private Color aimFlashColor = new Color(1f, 0.85f, 0.2f, 1f);

    private Transform player;
    private EnemyBase enemy;
    private IObjectPool<EnemyProjectile> pool;
    private float cooldown;
    private SpriteRenderer sr;
    private Coroutine aimRoutine;

    /// <summary>EnemyAI가 거리 유지에 사용</summary>
    public float StopDistance => stopDistance;

    void Awake()
    {
        enemy = GetComponent<EnemyBase>();
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = GetComponentInChildren<SpriteRenderer>();

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
        aimRoutine = null;
    }

    void OnDisable()
    {
        // 풀 반환 시 점멸 잔상 방지
        if (aimRoutine != null) { StopCoroutine(aimRoutine); aimRoutine = null; }
        if (sr != null) sr.color = Color.white;
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (player == null || enemy == null) return;
        if (enemy.IsStunned) return;
        if (projectilePrefab == null) return;
        if (aimRoutine != null) return; // 조준 중에는 cooldown 진행 안 함

        cooldown -= Time.deltaTime;
        if (cooldown > 0f) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > attackRange) return;

        aimRoutine = StartCoroutine(AimAndFire());
    }

    IEnumerator AimAndFire()
    {
        // 조준 단계: SpriteRenderer 점멸 → 시야 제한 스테이지에서 위치 단서
        Color original = sr != null ? sr.color : Color.white;
        float t = 0f;
        while (t < aimDuration)
        {
            if (sr != null)
            {
                // 4Hz 점멸 (aimDuration 0.3s 기준 약 1.2회 점멸)
                float phase = Mathf.PingPong(t * 8f, 1f);
                sr.color = Color.Lerp(original, aimFlashColor, phase);
            }
            t += Time.deltaTime;
            yield return null;
        }
        if (sr != null) sr.color = original;

        // 발사 — 발사 직전 플레이어 위치 기준 (조준 중 이동도 추적)
        if (player != null && projectilePrefab != null)
        {
            Fire(((Vector2)player.position - (Vector2)transform.position).normalized);
        }
        cooldown = attackInterval;
        aimRoutine = null;
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
