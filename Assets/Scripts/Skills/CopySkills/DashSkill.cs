using System.Collections;
using UnityEngine;

// 돌진 — 마우스 방향으로 짧은 돌진, 돌진 중 무적 (60E)
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

        rb.velocity = AimDirection() * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.velocity           = Vector2.zero;
        controller.IsDashing  = false;
        stats.IsInvincible    = false;
        isDashing             = false;
    }

    // 마우스 방향 조준 (없으면 이동 방향 폴백) — 카피 스킬 공통 패턴
    Vector2 AimDirection()
    {
        var cam = Camera.main;
        if (cam != null)
        {
            Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            Vector2 d = (Vector2)mouseWorld - (Vector2)transform.position;
            if (d.sqrMagnitude > 0.0001f) return d.normalized;
        }
        return controller.FaceDirection;
    }
}
