using System.Collections;
using UnityEngine;

/// <summary>
/// 향유고래 보스 초음파 광역 공격 — 페이즈1부터 일정 주기로 보스 주위 원형 광역.
/// BossSwingAttack 패턴 응용 (조준 → 즉발). 부채꼴 대신 360° 전방위.
/// 페이즈2 진입 시 발사 간격 단축.
/// </summary>
[RequireComponent(typeof(BossBase))]
public class BossWhaleUltrasonic : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float initialDelay = 4f;
    [SerializeField] private float attackInterval = 6f;
    [SerializeField] private float warningDuration = 0.8f;
    [Tooltip("페이즈2 진입 시 attackInterval 배율")]
    [SerializeField] private float phase2IntervalMultiplier = 0.6f;

    [Header("Pulse")]
    [SerializeField] private float radius = 2.7f;
    [SerializeField] private float damage = 22f;

    [Header("Visual")]
    [SerializeField] private Color warningColor = new Color(0.3f, 0.8f, 1f, 0.6f);
    [SerializeField] private Color pulseColor   = new Color(0.6f, 0.95f, 1f, 1f);
    [SerializeField] private float ringWidth = 0.12f;
    [SerializeField] private float pulseFade = 0.3f;

    private BossBase boss;
    private Transform player;
    private bool running;

    void Awake() { boss = GetComponent<BossBase>(); }

    void OnEnable()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        player = p != null ? p.transform : null;
        running = true;
        StartCoroutine(Loop());
    }

    void OnDisable() { running = false; }

    IEnumerator Loop()
    {
        yield return new WaitForSeconds(initialDelay);
        while (running && boss != null && !boss.IsDead)
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState == GameManager.GameState.Playing &&
                player != null)
            {
                yield return PulseOnce();
            }
            float interval = boss.IsPhase2 ? attackInterval * phase2IntervalMultiplier : attackInterval;
            yield return new WaitForSeconds(interval);
        }
    }

    IEnumerator PulseOnce()
    {
        // 경고: 보스 중심에 원형 라인 표시. worldSpace로 부모 scale 영향 차단.
        var fxObj = new GameObject("WhaleUltrasonicTelegraph");
        var lr = fxObj.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = ringWidth;
        lr.endWidth = ringWidth;
        lr.startColor = warningColor;
        lr.endColor = warningColor;
        DrawCircleWorld(lr, transform.position, radius, 48);

        yield return new WaitForSeconds(warningDuration);

        if (boss == null || boss.IsDead) { if (fxObj != null) Destroy(fxObj); yield break; }

        // 발사: 색 강조 + 판정
        lr.startColor = pulseColor;
        lr.endColor = pulseColor;

        var hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var hit in hits)
        {
            var stats = hit.GetComponent<PlayerStats>();
            if (stats != null) stats.TakeDamage(damage);
        }

        // 페이드
        float elapsed = 0f;
        while (elapsed < pulseFade && lr != null)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, elapsed / pulseFade);
            var c = pulseColor; c.a = a;
            lr.startColor = c; lr.endColor = c;
            yield return null;
        }
        if (fxObj != null) Destroy(fxObj);
    }

    void DrawCircleWorld(LineRenderer lr, Vector3 center, float r, int seg)
    {
        lr.positionCount = seg + 1;
        for (int i = 0; i <= seg; i++)
        {
            float a = i / (float)seg * Mathf.PI * 2f;
            lr.SetPosition(i, center + new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f));
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
