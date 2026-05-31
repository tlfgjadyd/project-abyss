using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyBase))]
public class EnemyAI : MonoBehaviour
{
    private Rigidbody2D rb;
    private EnemyBase enemy;
    private EnemyRangedAttack rangedAttack; // 있으면 거리 유지 (원거리 적)
    private Transform player;
    private SpriteRenderer sr; // flipX로 진행방향 표현 (sprite는 오른쪽 보고 있다고 가정)

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemy = GetComponent<EnemyBase>();
        rangedAttack = GetComponent<EnemyRangedAttack>();
        sr = GetComponentInChildren<SpriteRenderer>();
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

        // 죽음 애니 재생 중에는 추적 중단(곧 SetActive(false))
        if (enemy.IsDead)
        {
            rb.velocity = Vector2.zero;
            return;
        }

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
                FacePlayer(toPlayer.x);
                return;
            }
            if (distance < stopDist)
            {
                // 적정 거리: 정지 (정지해도 플레이어를 바라봄)
                rb.velocity = Vector2.zero;
                FacePlayer(toPlayer.x);
                return;
            }
        }

        // 기본: 추적
        rb.velocity = toPlayer.normalized * enemy.Data.moveSpeed * enemy.MoveSpeedMultiplier;
        FacePlayer(toPlayer.x);
    }

    /// <summary>플레이어 x 방향 기준으로 sprite flipX (sprite는 우측이 기본).</summary>
    void FacePlayer(float dx)
    {
        if (sr == null) return;
        if (Mathf.Abs(dx) < 0.05f) return; // 떨림 방지 dead-zone
        sr.flipX = dx < 0f;
    }
}
