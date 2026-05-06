using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BossBase))]
public class BossAI : MonoBehaviour
{
    private Rigidbody2D rb;
    private BossBase    boss;
    private Transform   player;

    private bool  isCharging;
    private float chargeTimer;

    void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        boss = GetComponent<BossBase>();
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
