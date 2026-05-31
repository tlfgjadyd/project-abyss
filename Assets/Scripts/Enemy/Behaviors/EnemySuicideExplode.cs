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
    [Tooltip("점멸 색상 (선명한 노랑이 피격 흰색 flash와 잘 구분됨)")]
    [SerializeField] private Color warningColor = new Color(1f, 0.85f, 0.1f, 1f);
    [Tooltip("점멸 간격 (초)")]
    [SerializeField] private float blinkInterval = 0.12f;
    [Tooltip("점멸 시 스케일 펌프 (1.0=원본, 1.15=15% 부풀어오름). '곧 터진다' 시각언어")]
    [SerializeField] private float warningScalePump = 1.15f;
    [Tooltip("경고 동안 Hurt 애니메이션 트리거 주기적 재발동 (몸 떨림). 0이면 비활성")]
    [SerializeField] private float hurtAnimRetriggerInterval = 0.25f;

    [Header("폭발")]
    [SerializeField] private float explosionRadius = 2.5f;
    [SerializeField] private float damage = 25f;
    [Tooltip("플레이어/장애물 레이어 마스크 (피해 대상)")]
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Behavior")]
    [Tooltip("true일 경우 플레이어와 접촉 시 즉시 폭발 (경고 단계 스킵). Jellyfish용.")]
    [SerializeField] private bool instantOnContact = false;

    private EnemyBase enemy;
    private SpriteRenderer sr;
    private Animator animator;
    private Transform player;
    private Color originalColor;
    private Vector3 originalScale;
    private bool armed;
    static readonly int HashHurt = Animator.StringToHash("Hurt");

    void Awake()
    {
        enemy = GetComponent<EnemyBase>();
        sr = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
        if (sr != null) originalColor = sr.color;
        originalScale = transform.localScale;
    }

    void OnEnable()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        player = p != null ? p.transform : null;
        armed = false;
        if (sr != null) sr.color = originalColor;
        transform.localScale = originalScale; // 풀 재활용 시 scale 복원
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
            if (instantOnContact)
                Explode();
            else
                StartCoroutine(SuicideRoutine());
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (armed) return;
        if (!instantOnContact) return;
        if (col.gameObject.CompareTag("Player"))
        {
            armed = true;
            Explode();
        }
    }

    IEnumerator SuicideRoutine()
    {
        // 경고: 색 점멸(노랑↔원본) + 스케일 펌프(원본↔확대) + Hurt 애니 주기 트리거.
        // 피격 시 HitEffect.PlayFlash(흰색 0.1s)와 시각언어가 명확히 구분됨.
        float elapsed = 0f;
        float hurtTimer = 0f;
        bool flipped = false;
        while (elapsed < warningDuration)
        {
            if (sr != null)
                sr.color = flipped ? originalColor : warningColor;
            transform.localScale = flipped ? originalScale : originalScale * warningScalePump;
            flipped = !flipped;

            // Hurt 애니메이션 주기 재트리거 → 몸 떨림이 멈추지 않음
            if (hurtAnimRetriggerInterval > 0f && animator != null
                && animator.runtimeAnimatorController != null)
            {
                hurtTimer += blinkInterval;
                if (hurtTimer >= hurtAnimRetriggerInterval)
                {
                    hurtTimer = 0f;
                    foreach (var p in animator.parameters)
                    {
                        if (p.nameHash == HashHurt && p.type == AnimatorControllerParameterType.Trigger)
                        { animator.SetTrigger(HashHurt); break; }
                    }
                }
            }

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
        transform.localScale = originalScale;
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
