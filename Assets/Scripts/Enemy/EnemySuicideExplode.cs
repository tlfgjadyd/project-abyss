using System.Collections;
using UnityEngine;

/// <summary>
/// 자폭 적 컴포넌트.
/// 플레이어가 triggerRange 안에 들어오면 경고 모션(색 점멸) 후 폭발 → 광범위 피해 + 자기 사망.
/// 디자인: 심해해파리(3스), 향후 연구소장 드론.
/// 페어플레이: warningDuration 동안 시각적으로 회피 가능해야 함.
/// </summary>
[RequireComponent(typeof(EnemyBase))]
public class EnemySuicideExplode : MonoBehaviour
{
    [Header("자폭 트리거")]
    [Tooltip("이 거리 안에 플레이어가 들어오면 자폭 시퀀스 시작")]
    [SerializeField] private float triggerRange = 2.5f;

    [Header("경고")]
    [Tooltip("경고(점멸) 지속 시간. 이 시간 동안 회피 가능해야 함")]
    [SerializeField] private float warningDuration = 1f;
    [Tooltip("점멸 색상")]
    [SerializeField] private Color warningColor = new Color(1f, 0.3f, 0.3f, 1f);
    [Tooltip("점멸 간격 (초)")]
    [SerializeField] private float blinkInterval = 0.1f;

    [Header("폭발")]
    [SerializeField] private float explosionRadius = 2.5f;
    [SerializeField] private float damage = 25f;
    [Tooltip("플레이어/장애물 레이어 마스크 (피해 대상)")]
    [SerializeField] private LayerMask hitMask = ~0;

    private EnemyBase enemy;
    private SpriteRenderer sr;
    private Transform player;
    private Color originalColor;
    private bool armed;

    void Awake()
    {
        enemy = GetComponent<EnemyBase>();
        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
    }

    void OnEnable()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        player = p != null ? p.transform : null;
        armed = false;
        if (sr != null) sr.color = originalColor;
    }

    void Update()
    {
        if (armed) return;
        if (player == null) return;
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (enemy != null && enemy.IsStunned) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= triggerRange)
        {
            armed = true;
            StartCoroutine(SuicideRoutine());
        }
    }

    IEnumerator SuicideRoutine()
    {
        // 경고 점멸
        float elapsed = 0f;
        bool flipped = false;
        while (elapsed < warningDuration)
        {
            if (sr != null)
                sr.color = flipped ? originalColor : warningColor;
            flipped = !flipped;

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        Explode();
    }

    void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, hitMask);
        foreach (var col in hits)
        {
            var stats = col.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.TakeDamage(damage);
            }
        }

        if (sr != null) sr.color = originalColor;
        // 자기 사망 (풀로 반환). EnemyBase의 Die 흐름과 별개라 단순 비활성화.
        gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.6f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, triggerRange);
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
