using System.Collections;
using UnityEngine;

// 돌진 — FaceDirection으로 짧은 돌진, 돌진 중 무적 (60E)
[RequireComponent(typeof(Rigidbody2D))]
public class DashSkill : CopySkillBase
{
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;

    private Rigidbody2D rb;
    private bool isDashing;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
    }

    public override bool CanExecute() => !isDashing;

    public override void Execute()
    {
        StartCoroutine(DashRoutine());
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;
        controller.IsDashing  = true;
        stats.IsInvincible    = true;

        rb.velocity = controller.FaceDirection * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.velocity           = Vector2.zero;
        controller.IsDashing  = false;
        stats.IsInvincible    = false;
        isDashing             = false;
    }
}
