using System.Collections;
using UnityEngine;

/// <summary>
/// 원거리 레이저 공격 적 컴포넌트. 사거리 안에서 일정 주기로 조준 → 발사.
/// 조준 단계: 얇은 빨간 LineRenderer 표시 (회피 시간 확보).
/// 발사 단계: 즉발 OverlapBox 데미지 + 두꺼운 흰 라인.
/// 디자인: 4스 원거리 전투요원, 연구소장 페이즈2.
/// 페어플레이: aimDuration 동안 시각적으로 회피 가능.
/// </summary>
[RequireComponent(typeof(EnemyBase))]
public class EnemyLaserAttack : MonoBehaviour
{
    [Header("발사")]
    [SerializeField] private float attackInterval = 3f;
    [SerializeField] private float damage = 12f;

    [Header("거리")]
    [Tooltip("이 거리 안에 들어오면 조준 시작")]
    [SerializeField] private float attackRange = 18f;
    [Tooltip("적정 거리. EnemyAI가 참조하여 추적 시 정지")]
    [SerializeField] private float stopDistance = 16f;

    [Header("Timing")]
    [Tooltip("조준 단계 지속 시간 (회피 시간)")]
    [SerializeField] private float aimDuration = 1f;
    [Tooltip("발사 라인 잔상 지속 시간")]
    [SerializeField] private float beamDuration = 0.1f;

    [Header("Hit")]
    [Tooltip("레이저 두께 (OverlapBox 두께)")]
    [SerializeField] private float beamWidth = 0.4f;
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Visual")]
    [SerializeField] private Color aimColor    = new Color(1f, 0.2f, 0.2f, 0.7f);
    [SerializeField] private Color beamColor   = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private float aimWidth    = 0.06f;
    [SerializeField] private float beamLineWidth = 0.3f;

    private Transform player;
    private EnemyBase enemy;
    private float cooldown;
    private bool isCasting;
    private GameObject activeFx; // 진행 중인 조준/발사 라인. 적 사망 시 OnDisable에서 정리.

    /// <summary>EnemyAI가 거리 유지에 사용</summary>
    public float StopDistance => stopDistance;

    void Awake()
    {
        enemy = GetComponent<EnemyBase>();
    }

    void OnEnable()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        player = p != null ? p.transform : null;
        cooldown = attackInterval;
        isCasting = false;
    }

    void OnDisable()
    {
        // 적이 사망(SetActive(false))하면 코루틴이 즉시 중단되어 fxObj가 정리되지 않는다.
        // 이 시점에 명시적으로 파괴.
        if (activeFx != null)
        {
            Destroy(activeFx);
            activeFx = null;
        }
        isCasting = false;
    }

    void Update()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (player == null || enemy == null) return;
        if (enemy.IsStunned) return;
        if (isCasting) return;

        cooldown -= Time.deltaTime;
        if (cooldown > 0f) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > attackRange) return;

        StartCoroutine(FireRoutine());
        cooldown = attackInterval;
    }

    IEnumerator FireRoutine()
    {
        isCasting = true;

        // ── 조준 단계 ─────────────────────
        Vector2 origin = transform.position;
        Vector2 dir    = ((Vector2)player.position - origin).normalized;
        Vector2 end    = origin + dir * attackRange;

        var fxObj = new GameObject("EnemyLaserTelegraph");
        activeFx  = fxObj;
        var lr    = fxObj.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = aimWidth;
        lr.endWidth   = aimWidth;
        lr.startColor = aimColor;
        lr.endColor   = aimColor;
        lr.positionCount = 2;
        lr.SetPosition(0, origin);
        lr.SetPosition(1, end);

        yield return new WaitForSeconds(aimDuration);

        // 스턴/사망 시 취소
        if (enemy == null || !enemy.gameObject.activeInHierarchy || enemy.IsStunned)
        {
            if (fxObj != null) Destroy(fxObj);
            activeFx = null;
            isCasting = false;
            yield break;
        }

        // ── 발사 단계 ─────────────────────
        // 발사 직전 위치/방향 재계산 (적이 이동했을 수 있으므로)
        origin = transform.position;
        end    = origin + dir * attackRange; // 방향은 조준 시점 고정 (회피 가능성 유지)

        lr.startWidth = beamLineWidth;
        lr.endWidth   = beamLineWidth;
        lr.startColor = beamColor;
        lr.endColor   = beamColor;
        lr.SetPosition(0, origin);
        lr.SetPosition(1, end);

        // OverlapBox: 사각형 영역에 들어온 PlayerStats에 데미지
        float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Vector2 boxCenter = origin + dir * (attackRange * 0.5f);
        Vector2 boxSize   = new Vector2(attackRange, beamWidth);
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, angleDeg, hitMask);
        foreach (var hit in hits)
        {
            var stats = hit.GetComponent<PlayerStats>();
            if (stats != null) stats.TakeDamage(damage);
        }

        // 잔상 페이드
        float elapsed = 0f;
        while (elapsed < beamDuration && lr != null)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, elapsed / beamDuration);
            var c = beamColor;
            c.a = a;
            lr.startColor = c;
            lr.endColor   = c;
            yield return null;
        }

        if (fxObj != null) Destroy(fxObj);
        activeFx = null;
        isCasting = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}
