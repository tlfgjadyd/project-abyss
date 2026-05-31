using System.Collections;
using UnityEngine;

/// <summary>
/// 연구소장 보스의 레이저 공격.
/// 페이즈1부터 발동 (스폰 후 initialDelay), 페이즈2 진입 시 발사 간격 단축으로 위협도 증가.
/// EnemyLaserAttack 패턴 응용. activeFx를 추적해 사망 시 OnDisable에서 정리.
/// </summary>
[RequireComponent(typeof(BossBase))]
public class BossDirectorAttack : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("스폰 후 첫 레이저까지 대기")]
    [SerializeField] private float initialDelay = 2.5f;
    [Tooltip("페이즈1 발사 간격")]
    [SerializeField] private float fireInterval = 4.5f;
    [Tooltip("페이즈2 발사 간격 배율 (낮을수록 자주). fireInterval * 이 값")]
    [SerializeField] private float phase2IntervalMultiplier = 0.55f;
    [SerializeField] private float aimDuration = 1.0f;
    [SerializeField] private float beamDuration = 0.15f;

    [Header("Hit")]
    [SerializeField] private float range = 14f;
    [SerializeField] private float beamWidth = 0.6f;
    [SerializeField] private float damage = 30f;
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Visual")]
    [SerializeField] private Color aimColor    = new Color(1f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color beamColor   = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private float aimWidth    = 0.08f;
    [SerializeField] private float beamLineWidth = 0.45f;

    [Header("Drone Summon (페이즈2)")]
    [Tooltip("Enemy_Drone prefab")]
    [SerializeField] private GameObject dronePrefab;
    [Tooltip("페이즈2 진입 후 첫 드론 소환까지 대기")]
    [SerializeField] private float droneInitialDelay = 3f;
    [SerializeField] private float droneSummonInterval = 6f;
    [SerializeField] private int dronesPerSummon = 2;
    [SerializeField] private float droneSpawnRadius = 2f;

    private BossBase boss;
    private Transform player;
    private bool active;
    private GameObject activeFx;
    private Coroutine fireLoop;
    private Coroutine droneLoop;

    void Awake()
    {
        boss = GetComponent<BossBase>();
    }

    void OnEnable()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        player = p != null ? p.transform : null;
        active = true;

        // 페이즈1부터 발사 시작
        fireLoop = StartCoroutine(FireLoop());

        // 페이즈2 진입 시 드론 소환 루프 시작
        if (boss != null) boss.OnPhase2Entered += StartDroneLoop;
    }

    void OnDisable()
    {
        if (activeFx != null) { Destroy(activeFx); activeFx = null; }
        if (fireLoop != null) { StopCoroutine(fireLoop); fireLoop = null; }
        if (droneLoop != null) { StopCoroutine(droneLoop); droneLoop = null; }
        if (boss != null) boss.OnPhase2Entered -= StartDroneLoop;
        active = false;
    }

    void StartDroneLoop()
    {
        if (droneLoop == null && dronePrefab != null)
            droneLoop = StartCoroutine(DroneSummonLoop());
    }

    IEnumerator DroneSummonLoop()
    {
        yield return new WaitForSeconds(droneInitialDelay);
        while (active && boss != null && !boss.IsDead)
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState == GameManager.GameState.Playing)
            {
                for (int i = 0; i < dronesPerSummon; i++)
                {
                    Vector2 offset = Random.insideUnitCircle.normalized * droneSpawnRadius;
                    Vector2 pos = (Vector2)transform.position + offset;
                    Instantiate(dronePrefab, pos, Quaternion.identity);
                }
            }
            yield return new WaitForSeconds(droneSummonInterval);
        }
    }

    IEnumerator FireLoop()
    {
        yield return new WaitForSeconds(initialDelay);

        while (active && boss != null && !boss.IsDead)
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState == GameManager.GameState.Playing &&
                player != null)
            {
                yield return FireOnce();
            }
            float interval = boss.IsPhase2 ? fireInterval * phase2IntervalMultiplier : fireInterval;
            yield return new WaitForSeconds(interval);
        }
    }

    IEnumerator FireOnce()
    {
        // ── 조준 단계 ─────────────────────
        Vector2 origin = transform.position;
        Vector2 dir    = ((Vector2)player.position - origin).normalized;
        Vector2 end    = origin + dir * range;

        var fxObj = new GameObject("BossDirectorLaserTelegraph");
        activeFx = fxObj;
        var lr   = fxObj.AddComponent<LineRenderer>();
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

        // 보스 사망/일시정지 시 취소
        if (boss == null || boss.IsDead)
        {
            if (fxObj != null) Destroy(fxObj);
            activeFx = null;
            yield break;
        }

        // ── 발사 단계 ─────────────────────
        origin = transform.position;
        end    = origin + dir * range;

        lr.startWidth = beamLineWidth;
        lr.endWidth   = beamLineWidth;
        lr.startColor = beamColor;
        lr.endColor   = beamColor;
        lr.SetPosition(0, origin);
        lr.SetPosition(1, end);

        float angleDeg   = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Vector2 boxCenter = origin + dir * (range * 0.5f);
        Vector2 boxSize   = new Vector2(range, beamWidth);
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
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
