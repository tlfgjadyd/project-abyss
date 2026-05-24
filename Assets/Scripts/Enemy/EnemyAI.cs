using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyBase))]
public class EnemyAI : MonoBehaviour
{
    private Rigidbody2D rb;
    private EnemyBase enemy;
    private EnemyRangedAttack rangedAttack; // 있으면 거리 유지 (원거리 적)
    private Transform player;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemy = GetComponent<EnemyBase>();
        rangedAttack = GetComponent<EnemyRangedAttack>();
    }

    void OnEnable()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (player == null) return;

        if (enemy.IsStunned)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 넉백 중에는 velocity 덮어쓰기 양보 (rb가 자유 비행)
        if (enemy.IsKnockedBack) return;

        Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
        float distance  = toPlayer.magnitude;

        // 원거리 적: stopDistance 근방에서 정지/후퇴
        if (rangedAttack != null)
        {
            float stopDist = rangedAttack.StopDistance;
            float retreatDist = stopDist * 0.7f;

            if (distance < retreatDist)
            {
                // 너무 가까우면 후퇴
                rb.velocity = -toPlayer.normalized * enemy.Data.moveSpeed * enemy.MoveSpeedMultiplier;
                return;
            }
            if (distance < stopDist)
            {
                // 적정 거리: 정지
                rb.velocity = Vector2.zero;
                return;
            }
        }

        // 기본: 추적
        rb.velocity = toPlayer.normalized * enemy.Data.moveSpeed * enemy.MoveSpeedMultiplier;
    }
}
