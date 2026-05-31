using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 산갈치 텔레포트 패턴 — 페이즈2 진입 후 일정 간격으로:
///   1) 페이드 아웃 (alpha → 0) + 무적 + 콜라이더 비활성
///   2) 플레이어 근처 랜덤 위치로 이동 (segments도 head 위치로 리셋)
///   3) 페이드 인 (alpha → 1) + 무적 해제 + 콜라이더 활성
/// 3스 시야 제한 컨셉과 정합 — "어둠 속으로 사라졌다가 시야 밖 어딘가에서 다시 나타남"
/// </summary>
[RequireComponent(typeof(BossBase))]
public class BossOarfishTeleport : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("페이즈2 진입 후 첫 텔레포트까지 대기")]
    [SerializeField] private float initialDelay = 5f;
    [SerializeField] private float interval = 12f;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float fadeInDuration = 1f;

    [Header("Spawn Position")]
    [Tooltip("플레이어 기준 텔레포트 거리 (시야 밖 또는 가장자리)")]
    [SerializeField] private float teleportDistance = 6f;
    [Tooltip("거리 변동 (랜덤 ±)")]
    [SerializeField] private float distanceVariance = 1.5f;

    private BossBase boss;
    private OarfishBody body;
    private SpriteRenderer headSr;
    private SpriteRenderer[] segSrs;
    private Transform[] segments;
    private Collider2D headCol;
    private Collider2D[] segCols;
    private Coroutine loopCo;

    void Awake()
    {
        boss = GetComponent<BossBase>();
        body = GetComponent<OarfishBody>();
        headSr = GetComponent<SpriteRenderer>();
        headCol = GetComponent<Collider2D>();
    }

    void OnEnable()
    {
        if (boss != null) boss.OnPhase2Entered += StartLoop;
    }

    void OnDisable()
    {
        if (boss != null) boss.OnPhase2Entered -= StartLoop;
        if (loopCo != null) StopCoroutine(loopCo);
        // 텔레포트 중 OnDisable 호출되면 잔여 상태 복원
        SetVisibility(1f);
        if (boss != null) boss.IsInvincible = false;
    }

    void StartLoop()
    {
        if (loopCo == null) loopCo = StartCoroutine(Loop());
    }

    IEnumerator Loop()
    {
        // OarfishBody Awake에서 segments가 SetParent(null)된 후라 여기서 캐싱
        yield return null; // 한 프레임 대기
        CacheSegments();

        yield return new WaitForSeconds(initialDelay);

        while (boss != null && !boss.IsDead)
        {
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentState == GameManager.GameState.Playing)
            {
                yield return TeleportOnce();
            }
            yield return new WaitForSeconds(interval);
        }
    }

    void CacheSegments()
    {
        var list = new List<Transform>();
        var srs = new List<SpriteRenderer>();
        var cols = new List<Collider2D>();
        // OarfishBody.segments는 SetParent(null) 후 root에 있음. 이름으로 탐색.
        var all = UnityEngine.Object.FindObjectsOfType<Transform>(true);
        foreach (var tr in all)
        {
            if (tr == null || !tr.name.StartsWith("Segment")) continue;
            // 부모가 null이거나 본 보스 root였던 것만 (다른 보스 마디 충돌 방지) — 일단 root parent == null 모두 수집
            if (tr.parent != null) continue;
            list.Add(tr);
            var sr = tr.GetComponent<SpriteRenderer>();
            if (sr != null) srs.Add(sr);
            var col = tr.GetComponent<Collider2D>();
            if (col != null) cols.Add(col);
        }
        segments = list.ToArray();
        segSrs = srs.ToArray();
        segCols = cols.ToArray();
    }

    IEnumerator TeleportOnce()
    {
        // 1. 페이드 아웃 + 무적
        boss.IsInvincible = true;
        SetCollidersEnabled(false);
        float t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            SetVisibility(1f - t / fadeOutDuration);
            yield return null;
        }
        SetVisibility(0f);

        // 2. 텔레포트 — 플레이어 근처 랜덤 위치
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float dist = teleportDistance + Random.Range(-distanceVariance, distanceVariance);
            Vector2 dir = Random.insideUnitCircle.normalized;
            if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
            Vector2 newHeadPos = (Vector2)player.transform.position + dir * dist;
            transform.position = newHeadPos;

            // segments도 head 뒤로 일렬 정렬 (이후 OarfishBody chain follow가 자연스럽게 잡음)
            if (segments != null)
            {
                Vector2 backDir = -dir;
                for (int i = 0; i < segments.Length; i++)
                {
                    if (segments[i] == null) continue;
                    segments[i].position = newHeadPos + backDir * 1.1f * (i + 1);
                }
            }
        }

        // 3. 짧은 일시 정지 후 페이드 인
        yield return new WaitForSeconds(0.3f);

        t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            SetVisibility(t / fadeInDuration);
            yield return null;
        }
        SetVisibility(1f);

        SetCollidersEnabled(true);
        boss.IsInvincible = false;
    }

    void SetVisibility(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);
        if (headSr != null)
        {
            var c = headSr.color; c.a = alpha; headSr.color = c;
        }
        if (segSrs != null)
        {
            foreach (var sr in segSrs)
            {
                if (sr == null) continue;
                var c = sr.color; c.a = alpha; sr.color = c;
            }
        }
    }

    void SetCollidersEnabled(bool on)
    {
        if (headCol != null) headCol.enabled = on;
        if (segCols != null) foreach (var c in segCols) if (c != null) c.enabled = on;
    }
}
