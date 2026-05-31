using System.Collections;
using UnityEngine;

/// <summary>
/// 향유고래 페이즈2 광역 슬로우 + 취약 디버프.
/// OnPhase2Entered 구독 → 주기적으로 광역 발동. 사거리 내 플레이어에 일정 시간 디버프 적용.
/// DeepPressureSkill 응용 (역으로 보스 → 플레이어).
/// </summary>
[RequireComponent(typeof(BossBase))]
public class BossWhalePhase2Debuff : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("페이즈2 진입 후 첫 발동까지 대기")]
    [SerializeField] private float initialDelay = 3f;
    [SerializeField] private float interval = 10f;
    [SerializeField] private float warningDuration = 0.8f;

    [Header("Range")]
    [SerializeField] private float radius = 7f;

    [Header("Debuff")]
    [Tooltip("이동속도 배율 (디버프 동안)")]
    [SerializeField] private float slowMultiplier = 0.5f;
    [Tooltip("받는 데미지 배율 (디버프 동안)")]
    [SerializeField] private float vulnMultiplier = 1.5f;
    [SerializeField] private float debuffDuration = 4f;

    [Header("Visual")]
    [SerializeField] private Color warningColor = new Color(0.4f, 0.2f, 0.8f, 0.5f);
    [SerializeField] private Color activeColor  = new Color(0.6f, 0.3f, 1f, 1f);

    private BossBase boss;
    private bool active;
    private Coroutine loop;

    void Awake() { boss = GetComponent<BossBase>(); }

    void OnEnable()
    {
        if (boss != null) boss.OnPhase2Entered += StartLoop;
    }

    void OnDisable()
    {
        if (boss != null) boss.OnPhase2Entered -= StartLoop;
        if (loop != null) StopCoroutine(loop);
        active = false;
    }

    void StartLoop()
    {
        if (loop != null) return;
        active = true;
        loop = StartCoroutine(Loop());
    }

    IEnumerator Loop()
    {
        yield return new WaitForSeconds(initialDelay);
        while (active && boss != null && !boss.IsDead)
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState == GameManager.GameState.Playing)
            {
                yield return CastOnce();
            }
            yield return new WaitForSeconds(interval);
        }
    }

    IEnumerator CastOnce()
    {
        // 경고 원 (worldSpace — 부모 보스 scale 영향 차단)
        var fxObj = new GameObject("WhaleDebuffTelegraph");
        var lr = fxObj.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = 0.15f; lr.endWidth = 0.15f;
        lr.startColor = warningColor; lr.endColor = warningColor;
        int seg = 48;
        lr.positionCount = seg + 1;
        Vector3 center = transform.position;
        for (int i = 0; i <= seg; i++)
        {
            float a = i / (float)seg * Mathf.PI * 2f;
            lr.SetPosition(i, center + new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f));
        }

        yield return new WaitForSeconds(warningDuration);
        if (boss == null || boss.IsDead) { if (fxObj != null) Destroy(fxObj); yield break; }

        lr.startColor = activeColor; lr.endColor = activeColor;

        // 사거리 내 플레이어 검사 + 디버프 적용
        var hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var c in hits)
        {
            var stats = c.GetComponent<PlayerStats>();
            if (stats == null) continue;
            StartCoroutine(ApplyDebuff(stats));
        }

        // 페이드
        float el = 0f;
        const float fade = 0.4f;
        while (el < fade && lr != null)
        {
            el += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, el / fade);
            var c = activeColor; c.a = a;
            lr.startColor = c; lr.endColor = c;
            yield return null;
        }
        if (fxObj != null) Destroy(fxObj);
    }

    IEnumerator ApplyDebuff(PlayerStats stats)
    {
        // 곱셈 누적 (중복 시전 시 더 강한 효과 유지)
        float prevSlow = stats.bossDebuffMoveMultiplier;
        float prevVuln = stats.bossDebuffVulnMultiplier;
        stats.bossDebuffMoveMultiplier = Mathf.Min(prevSlow, slowMultiplier);
        stats.bossDebuffVulnMultiplier = Mathf.Max(prevVuln, vulnMultiplier);

        yield return new WaitForSecondsRealtime(debuffDuration);

        if (stats != null)
        {
            // 단순 복원 (중복 디버프 정확 추적은 단순화 — 마지막 호출이 복원)
            stats.bossDebuffMoveMultiplier = 1f;
            stats.bossDebuffVulnMultiplier = 1f;
        }
    }
}
