using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 공허 관통 — 플레이어 위치에서 마우스 방향으로 5초간 빔을 유지하는 채널링 스킬.
/// 매 틱마다 마우스 방향 재계산하여 회전 가능. 플레이어 이동도 따라감.
/// 1틱 데미지는 낮지만 5초 × ~7틱/초 = 33+회 다단으로 누적.
///
/// Day 44 후속 재설계: FaceDirection 단일 발사 → 마우스 채널링 (5s)
/// </summary>
public class VoidPierceSkill : CopySkillBase
{
    [Header("Beam")]
    [Tooltip("관통 거리")]
    [SerializeField] private float length = 8f;
    [Tooltip("관통 두께 (BoxCast 반높이의 2배)")]
    [SerializeField] private float width = 1.2f;

    [Header("Channeling")]
    [Tooltip("총 채널링 시간 (초)")]
    [SerializeField] private float channelDuration = 5f;
    [Tooltip("틱 사이 간격 — 작을수록 다단 히트 횟수 증가")]
    [SerializeField] private float tickInterval = 0.15f;
    [Tooltip("빔이 마우스를 따라오는 최대 회전 속도 (도/초). 낮을수록 회전 어려움. 180 = 1초에 반바퀴")]
    [SerializeField] private float turnRate = 180f;

    [Header("Damage")]
    [Tooltip("기본 공격력 대비 1회 틱 배율 (다단 히트 누적 가능하므로 낮게)")]
    [SerializeField] private float damageMultiplier = 0.5f;

    [Header("Layer")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Visual")]
    [SerializeField] private Color beamColor = new Color(0.6f, 0.2f, 1f, 1f);
    [SerializeField] private float visualLineWidth = 0.35f;

    private bool isCasting;

    public override bool CanExecute() => !isCasting;

    public override void Execute()
    {
        if (controller == null || stats == null) return;
        StartCoroutine(ChannelRoutine());
    }

    IEnumerator ChannelRoutine()
    {
        isCasting = true;

        // 시각 빔 (매 프레임 갱신)
        var fxObj = new GameObject("VoidPierceFx");
        var lr = fxObj.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = visualLineWidth;
        lr.endWidth   = visualLineWidth;
        lr.startColor = beamColor;
        lr.endColor   = beamColor;
        lr.positionCount = 2;

        float elapsed = 0f;
        float tickAcc = 0f;
        var cam = Camera.main;

        // 초기 방향 — 채널 시작 시점의 마우스 방향. 이후 turnRate로 추적
        Vector2 currentDir = controller.FaceDirection;
        if (cam != null)
        {
            Vector3 mw0 = cam.ScreenToWorldPoint(Input.mousePosition);
            mw0.z = 0f;
            Vector2 md0 = (Vector2)mw0 - (Vector2)transform.position;
            if (md0.sqrMagnitude > 0.0001f) currentDir = md0.normalized;
        }

        while (elapsed < channelDuration)
        {
            // 매 프레임 origin 갱신 (플레이어 이동) + 마우스 방향으로 turnRate 제한 회전
            Vector2 origin = transform.position;
            Vector2 targetDir = currentDir;
            if (cam != null)
            {
                Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
                mouseWorld.z = 0f;
                Vector2 mouseDir = (Vector2)mouseWorld - origin;
                if (mouseDir.sqrMagnitude > 0.0001f) targetDir = mouseDir.normalized;
            }
            // 회전 속도 제한 — 최대 turnRate * deltaTime 도만큼 targetDir 쪽으로
            float maxRad = turnRate * Mathf.Deg2Rad * Time.deltaTime;
            currentDir = Vector3.RotateTowards(currentDir, targetDir, maxRad, 0f);
            Vector2 dir = currentDir;
            Vector2 end = origin + dir * length;

            // 빔 시각 갱신
            if (fxObj != null)
            {
                lr.SetPosition(0, origin);
                lr.SetPosition(1, end);
                // 종료 임박 시 알파 페이드
                float remain = channelDuration - elapsed;
                float a = remain < 0.5f ? Mathf.Clamp01(remain / 0.5f) : 1f;
                var c = beamColor; c.a = a;
                lr.startColor = c;
                lr.endColor   = c;
            }

            // 틱 누적
            tickAcc += Time.deltaTime;
            if (tickAcc >= tickInterval)
            {
                tickAcc -= tickInterval;
                ApplyTick(origin, dir);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (fxObj != null) Destroy(fxObj);
        isCasting = false;
    }

    void ApplyTick(Vector2 origin, Vector2 dir)
    {
        float damage = stats.attackPower * damageMultiplier;
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
    }

    void OnDisable()
    {
        // 채널 중 풀 반환/씬 전환 시 잔존 LineRenderer 정리
        isCasting = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.6f, 0.2f, 1f, 0.4f);
        Vector3 fwd = transform.right * length;
        Gizmos.DrawLine(transform.position, transform.position + fwd);
    }
}
