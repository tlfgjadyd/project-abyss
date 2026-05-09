using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BossBase))]
public class BossAI : MonoBehaviour
{
    [Header("돌진 경고 (Telegraph)")]
    [Tooltip("돌진 직전 정지 + 깜빡임 지속 시간")]
    [SerializeField] private float chargeWarningDuration = 0.4f;
    [Tooltip("경고 시 깜빡임 색상")]
    [SerializeField] private Color chargeWarningColor = Color.yellow;
    [Tooltip("깜빡임 1회 주기")]
    [SerializeField] private float chargeWarningBlinkInterval = 0.08f;

    private Rigidbody2D   rb;
    private BossBase      boss;
    private SpriteRenderer sr;
    private Transform     player;

    private bool  isCharging;
    private float chargeTimer;

    void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        boss = GetComponent<BossBase>();
        sr   = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        boss.OnPhase2Entered += EnterPhase2;
        isCharging  = false;
        chargeTimer = boss.Data.chargeInterval;
    }

    void OnDisable()
    {
        boss.OnPhase2Entered -= EnterPhase2;
    }

    // ── 이동 (FixedUpdate) ────────────────────────

    void FixedUpdate()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (boss.IsDead || player == null || isCharging) return;

        float speed = boss.IsPhase2
            ? boss.Data.moveSpeed * boss.Data.phase2SpeedMultiplier
            : boss.Data.moveSpeed;

        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        rb.velocity = dir * speed;
    }

    // ── 페이즈 2 돌진 (Update) ────────────────────

    void Update()
    {
        if (!boss.IsPhase2 || boss.IsDead || isCharging) return;
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        chargeTimer -= Time.deltaTime;
        if (chargeTimer <= 0f)
        {
            StartCoroutine(ChargeRoutine());
            chargeTimer = boss.Data.chargeInterval;
        }
    }

    void EnterPhase2()
    {
        // 페이즈 2 진입 직후 짧은 간격으로 첫 돌진
        chargeTimer = boss.Data.chargeInterval * 0.3f;
    }

    IEnumerator ChargeRoutine()
    {
        isCharging = true;

        // ── 1. 경고 단계: 정지 + 깜빡임 ─────────
        rb.velocity = Vector2.zero;

        // 현재 색상 보존 (페이즈2 톤이 적용된 상태일 수 있음)
        Color baseColor = sr != null ? sr.color : Color.white;
        float warningElapsed = 0f;
        bool blinkOn = false;

        while (warningElapsed < chargeWarningDuration)
        {
            if (sr != null)
            {
                blinkOn = !blinkOn;
                sr.color = blinkOn ? chargeWarningColor : baseColor;
            }
            float wait = Mathf.Min(chargeWarningBlinkInterval, chargeWarningDuration - warningElapsed);
            yield return new WaitForSeconds(wait);
            warningElapsed += wait;
        }

        // 색상 복귀 (페이즈2 톤 보존)
        if (sr != null) sr.color = baseColor;

        // 보스가 죽거나 일시정지된 경우 돌진 취소
        if (boss.IsDead ||
            GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            isCharging = false;
            yield break;
        }

        // ── 2. 돌진 단계 ───────────────────────
        if (player != null)
        {
            Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
            rb.velocity = dir * boss.Data.chargeSpeed;
        }

        yield return new WaitForSeconds(boss.Data.chargeDuration);

        rb.velocity = Vector2.zero;
        isCharging  = false;
    }
}
