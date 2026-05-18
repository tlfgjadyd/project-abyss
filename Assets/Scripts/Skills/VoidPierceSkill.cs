using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 공허 관통 — FaceDirection으로 긴 직선 다단 히트 (100E).
/// 3스테이지 산갈치 보스 카피 스킬.
/// BoxCast로 라인 안에 있는 모든 적을 1~N틱 동안 반복 타격.
/// </summary>
public class VoidPierceSkill : CopySkillBase
{
    [Header("Beam")]
    [Tooltip("관통 거리")]
    [SerializeField] private float length = 9f;
    [Tooltip("관통 두께 (BoxCast 반높이의 2배)")]
    [SerializeField] private float width = 1.2f;

    [Header("Damage")]
    [Tooltip("기본 공격력 대비 1회 틱 배율")]
    [SerializeField] private float damageMultiplier = 1.2f;
    [Tooltip("틱 횟수 (다단 히트)")]
    [SerializeField] private int hitTicks = 4;
    [Tooltip("틱 사이 간격 (초)")]
    [SerializeField] private float tickInterval = 0.1f;

    [Header("Layer")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Visual")]
    [SerializeField] private Color beamColor = new Color(0.6f, 0.2f, 1f, 1f);
    [SerializeField] private float visualDuration = 0.5f;
    [SerializeField] private float visualLineWidth = 0.35f;

    private bool isCasting;

    public override bool CanExecute() => !isCasting;

    public override void Execute()
    {
        if (controller == null || stats == null) return;
        StartCoroutine(PierceRoutine());
    }

    IEnumerator PierceRoutine()
    {
        isCasting = true;

        Vector2 origin = transform.position;
        Vector2 dir    = controller.FaceDirection;
        Vector2 end    = origin + dir * length;

        // 시각 빔 표시
        var fxObj = new GameObject("VoidPierceFx");
        var lr = fxObj.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = visualLineWidth;
        lr.endWidth   = visualLineWidth;
        lr.startColor = beamColor;
        lr.endColor   = beamColor;
        lr.positionCount = 2;
        lr.SetPosition(0, origin);
        lr.SetPosition(1, end);

        // 다단 히트
        float damage = stats.attackPower * damageMultiplier;
        for (int t = 0; t < hitTicks; t++)
        {
            HashSet<Collider2D> tickHits = new HashSet<Collider2D>();
            RaycastHit2D[] casts = Physics2D.BoxCastAll(
                origin,
                new Vector2(width, 0.1f),
                Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg,
                dir,
                length,
                enemyLayer
            );
            foreach (var rh in casts)
            {
                if (rh.collider == null || tickHits.Contains(rh.collider)) continue;
                tickHits.Add(rh.collider);
                rh.collider.GetComponent<IDamageable>()?.TakeDamage(damage);
            }

            yield return new WaitForSeconds(tickInterval);
        }

        // 시각 페이드
        float elapsed = 0f;
        while (elapsed < visualDuration && fxObj != null)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, elapsed / visualDuration);
            var c = beamColor;
            c.a = a;
            lr.startColor = c;
            lr.endColor   = c;
            yield return null;
        }

        if (fxObj != null) Destroy(fxObj);
        isCasting = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.6f, 0.2f, 1f, 0.4f);
        Vector3 fwd = transform.right * length;
        Gizmos.DrawLine(transform.position, transform.position + fwd);
    }
}
