using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 공허 관통 — 플레이어 위치에서 마우스 방향으로 5초간 빔을 유지하는 채널링 스킬.
/// 매 틱 마우스 방향 재계산(turnRate 제한 회전) + 플레이어 이동 추적.
/// 1틱 데미지는 낮지만 5초 다단 누적.
///
/// Day 52: 사거리/두께 강화(length 8→16, width 1.2→2.0). 시각은 단순 보라 빔 유지.
/// (마디 sprite 체인 시도했으나 사용자 피드백으로 단순 빔으로 환원)
/// </summary>
public class VoidPierceSkill : CopySkillBase
{
    [Header("Beam")]
    [Tooltip("관통 거리")]
    [SerializeField] private float length = 16f;
    [Tooltip("관통 두께 (BoxCast 반높이의 2배)")]
    [SerializeField] private float width = 2.0f;

    [Header("Channeling")]
    [SerializeField] private float channelDuration = 5f;
    [SerializeField] private float tickInterval = 0.15f;
    [Tooltip("빔이 마우스를 따라오는 최대 회전 속도 (도/초)")]
    [SerializeField] private float turnRate = 180f;

    [Header("Damage")]
    [SerializeField] private float damageMultiplier = 0.5f;

    [Header("Layer")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Visual")]
    [SerializeField] private Color beamColor = new Color(0.6f, 0.2f, 1f, 1f);
    [Tooltip("빔 시각 두께 (판정 width와 별개)")]
    [SerializeField] private float visualLineWidth = 0.5f;

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

        var fxObj = new GameObject("VoidPierceFx");
        var lr = fxObj.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.numCapVertices = 4;
        lr.startWidth = visualLineWidth;
        lr.endWidth   = visualLineWidth;
        lr.startColor = beamColor;
        lr.endColor   = beamColor;
        lr.positionCount = 2;
        lr.sortingOrder = 60;

        float elapsed = 0f;
        float tickAcc = 0f;
        var cam = Camera.main;

        // 초기 방향 — 채널 시작 시점의 마우스 방향
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
            Vector2 origin = transform.position;
            Vector2 targetDir = currentDir;
            if (cam != null)
            {
                Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
                mouseWorld.z = 0f;
                Vector2 mouseDir = (Vector2)mouseWorld - origin;
                if (mouseDir.sqrMagnitude > 0.0001f) targetDir = mouseDir.normalized;
            }
            float maxRad = turnRate * Mathf.Deg2Rad * Time.deltaTime;
            currentDir = Vector3.RotateTowards(currentDir, targetDir, maxRad, 0f);
            Vector2 dir = currentDir;
            Vector2 end = origin + dir * length;

            lr.SetPosition(0, origin);
            lr.SetPosition(1, end);
            float remain = channelDuration - elapsed;
            float a = remain < 0.5f ? Mathf.Clamp01(remain / 0.5f) : 1f;
            var c = beamColor; c.a = a;
            lr.startColor = c;
            lr.endColor   = c;

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
        isCasting = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.6f, 0.2f, 1f, 0.4f);
        Gizmos.DrawLine(transform.position, transform.position + transform.right * length);
    }
}
