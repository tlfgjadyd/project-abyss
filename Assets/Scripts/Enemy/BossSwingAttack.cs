using System.Collections;
using UnityEngine;

/// <summary>
/// 산갈치 보스 휘두르는 공격 — 일정 간격으로 플레이어 방향 부채꼴 광역 공격.
/// 조준 단계(라인 표시) → 공격 단계(즉발 OverlapCircle + 각도 필터).
/// Slash 구조 응용. UltrasonicSkill의 부채꼴 판정 패턴 참고.
/// 페어플레이: warningDuration 동안 시각적으로 회피 가능.
/// </summary>
[RequireComponent(typeof(BossBase))]
public class BossSwingAttack : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float attackInterval = 5f;
    [SerializeField] private float warningDuration = 0.7f;

    [Header("Fan")]
    [SerializeField] private float range = 5f;
    [Range(0f, 360f)]
    [SerializeField] private float angle = 100f;

    [Header("Damage")]
    [SerializeField] private float damage = 25f;

    [Header("Phase 2")]
    [Tooltip("페이즈2 진입 시 attackInterval 배율 (낮을수록 자주)")]
    [SerializeField] private float phase2IntervalMultiplier = 0.6f;

    [Header("Visual")]
    [SerializeField] private Color warningColor = new Color(1f, 0.6f, 0.2f, 0.7f);
    [SerializeField] private Color attackColor  = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private float lineWidth = 0.18f;

    private BossBase boss;
    private Transform player;
    private float cooldown;

    void Awake()
    {
        boss = GetComponent<BossBase>();
    }

    void OnEnable()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        player = p != null ? p.transform : null;
        cooldown = attackInterval;
    }

    void Update()
    {
        if (boss == null || boss.IsDead) return;
        if (player == null) return;
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        cooldown -= Time.deltaTime;
        if (cooldown <= 0f)
        {
            float interval = boss.IsPhase2 ? attackInterval * phase2IntervalMultiplier : attackInterval;
            cooldown = interval;
            StartCoroutine(SwingRoutine());
        }
    }

    IEnumerator SwingRoutine()
    {
        // 조준 단계: 시작 시점의 플레이어 방향으로 고정
        Vector2 origin = transform.position;
        Vector2 dir    = ((Vector2)player.position - origin).normalized;

        var fxObj = new GameObject("BossSwingTelegraph");
        var lr    = fxObj.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = lineWidth;
        lr.endWidth   = lineWidth;
        lr.startColor = warningColor;
        lr.endColor   = warningColor;
        DrawFan(lr, origin, dir, range, angle);

        yield return new WaitForSeconds(warningDuration);

        if (boss == null || boss.IsDead) { Destroy(fxObj); yield break; }

        // 공격 단계: 라인 색 강조 + 판정
        lr.startColor = attackColor;
        lr.endColor   = attackColor;

        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, range);
        foreach (var hit in hits)
        {
            var stats = hit.GetComponent<PlayerStats>();
            if (stats == null) continue;

            Vector2 toTarget = ((Vector2)hit.transform.position - origin).normalized;
            if (Vector2.Angle(dir, toTarget) > angle * 0.5f) continue;

            stats.TakeDamage(damage);
        }

        // 짧은 잔상 후 페이드 아웃
        float elapsed = 0f;
        const float fade = 0.25f;
        while (elapsed < fade && lr != null)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, elapsed / fade);
            var c = attackColor;
            c.a = a;
            lr.startColor = c;
            lr.endColor   = c;
            yield return null;
        }

        if (fxObj != null) Destroy(fxObj);
    }

    void DrawFan(LineRenderer lr, Vector3 origin, Vector2 dir, float r, float angleDeg)
    {
        const int arcSegments = 24;
        int total = arcSegments + 3;
        lr.positionCount = total;

        float halfAngle = angleDeg * 0.5f;
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        lr.SetPosition(0, origin);
        for (int i = 0; i <= arcSegments; i++)
        {
            float t  = (float)i / arcSegments;
            float a  = (baseAngle - halfAngle) + t * angleDeg;
            float ra = a * Mathf.Deg2Rad;
            Vector3 p = origin + new Vector3(Mathf.Cos(ra), Mathf.Sin(ra), 0f) * r;
            lr.SetPosition(i + 1, p);
        }
        lr.SetPosition(total - 1, origin);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
