using System.Collections;
using UnityEngine;

/// <summary>
/// 보스 페이즈2 진입 시 시각적 피드백.
/// - SpriteRenderer 색상을 광폭화 톤(붉은 톤)으로 영구 변경
/// - 진입 순간 스케일 펄스 애니메이션
/// - 진입 직후 연속 플래시
/// BossBase.OnPhase2Entered 이벤트를 구독하여 동작.
/// </summary>
[RequireComponent(typeof(BossBase))]
public class BossPhaseEffect : MonoBehaviour
{
    [Header("페이즈2 지속 색상")]
    [Tooltip("페이즈2 진입 후 보스 스프라이트에 영구 적용되는 색상 (광폭화 표현)")]
    [SerializeField] private Color phase2Tint = new Color(1f, 0.5f, 0.5f, 1f);

    [Header("진입 순간 펄스")]
    [Tooltip("진입 순간 스케일 배율 (1.0 = 원본 크기)")]
    [SerializeField] private float pulseScale = 1.3f;
    [SerializeField] private float pulseDuration = 0.3f;

    [Header("진입 순간 플래시")]
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private int flashCount = 3;
    [SerializeField] private float flashInterval = 0.08f;

    private BossBase boss;
    private SpriteRenderer sr;
    private Vector3 originalScale;
    private Color originalColor;
    private Coroutine effectRoutine;

    void Awake()
    {
        boss = GetComponent<BossBase>();
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) originalColor = sr.color;
        originalScale = transform.localScale;
    }

    void OnEnable()
    {
        // 풀에서 재사용 시 상태 초기화
        if (sr != null) sr.color = originalColor;
        transform.localScale = originalScale;

        boss.OnPhase2Entered += HandlePhase2Entered;
    }

    void OnDisable()
    {
        boss.OnPhase2Entered -= HandlePhase2Entered;
        if (effectRoutine != null)
        {
            StopCoroutine(effectRoutine);
            effectRoutine = null;
        }
    }

    void HandlePhase2Entered()
    {
        if (effectRoutine != null) StopCoroutine(effectRoutine);
        effectRoutine = StartCoroutine(Phase2EffectRoutine());
    }

    IEnumerator Phase2EffectRoutine()
    {
        // 1. 연속 플래시 (Phase2Tint <-> flashColor 교차)
        if (sr != null)
        {
            for (int i = 0; i < flashCount; i++)
            {
                sr.color = flashColor;
                yield return new WaitForSeconds(flashInterval);
                sr.color = phase2Tint;
                yield return new WaitForSeconds(flashInterval);
            }
            // 최종 지속 색상 적용
            sr.color = phase2Tint;
        }

        // 2. 스케일 펄스 (병렬로 돌리지 않고 플래시 후 실행: 단순화)
        float t = 0f;
        float half = pulseDuration * 0.5f;

        // 확장
        while (t < half)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / half);
            transform.localScale = Vector3.Lerp(originalScale, originalScale * pulseScale, k);
            yield return null;
        }

        // 복귀
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / half);
            transform.localScale = Vector3.Lerp(originalScale * pulseScale, originalScale, k);
            yield return null;
        }

        transform.localScale = originalScale;
        effectRoutine = null;
    }
}
