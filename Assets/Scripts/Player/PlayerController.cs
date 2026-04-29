using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerStats))]

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerStats stats;

    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.right; // 마지막 이동 방향 (공격 방향 기준)

    // 외부에서 방향 참조용 (PlayerCombat에서 사용)
    public Vector2 FaceDirection => lastMoveDir;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();

        rb.gravityScale = 0f;       // 2D 탑다운
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            rb.velocity = Vector2.zero;
            moveInput = Vector2.zero;
            return;
        }

        GatherInput();
    }

    void FixedUpdate()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        Move();
    }

    void GatherInput()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();

        if (moveInput != Vector2.zero)
            lastMoveDir = moveInput;
    }

    void Move()
    {
        rb.velocity = moveInput * stats.moveSpeed;
    }
}
