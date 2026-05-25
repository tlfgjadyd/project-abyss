using System.Collections;
using UnityEngine;

/// <summary>
/// 초음파 — 마우스 방향 부채꼴 광역 + 스턴 (80E).
/// 2스테이지 향유고래 보스 카피 스킬.
/// Day 48b 재설계: FaceDirection → 마우스 조준, 사거리 대폭 ↑ (화면 끝), 에너지 140→80.
/// </summary>
public class UltrasonicSkill : CopySkillBase
{
    [Header("Stats")]
    [SerializeField] private float range = 14f;             // 6 → 14 (화면 끝까지)
    [Range(0f, 360f)]
    [SerializeField] private float angle = 90f;             // 각도 유지
    [SerializeField] private float damageMultiplier = 5f;
    [SerializeField] private float stunDuration = 1.5f;

    [Header("Layer")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Visual")]
    [SerializeField] private Color visualColor = new Color(0.5f, 0.8f, 1f, 1f);
    [SerializeField] private float visualDuration = 0.3f;
    [SerializeField] private float visualLineWidth = 0.12f;

    public override void Execute()
    {
        if (controller == null || stats == null) return;

        // 마우스 방향 조준 (VoidPierce 패턴 재사용)
        Vector2 origin = transform.position;
        Vector2 dir;
        var cam = Camera.main;
        if (cam != null)
        {
            Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            Vector2 mouseDir = (Vector2)mouseWorld - origin;
            dir = mouseDir.sqrMagnitude > 0.0001f ? mouseDir.normalized : controller.FaceDirection;
        }
        else dir = controller.FaceDirection;

        float damage = stats.attackPower * damageMultiplier;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
        foreach (var hit in hits)
        {
            Vector2 toEnemy = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
            if (Vector2.Angle(dir, toEnemy) > angle * 0.5f) continue;

            hit.GetComponent<IDamageable>()?.TakeDamage(damage);
            hit.GetComponent<EnemyBase>()?.Stun(stunDuration);
        }

        // 시각 피드백 — 부채꼴 호+양변을 잠깐 표시
        StartCoroutine(ShowFanEffect(transform.position, dir, range, angle));
    }

    IEnumerator ShowFanEffect(Vector3 origin, Vector2 dir, float r, float angleDeg)
    {
        var fxObj = new GameObject("UltrasonicFx");
        fxObj.transform.position = origin;

        var lr = fxObj.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.startWidth = visualLineWidth;
        lr.endWidth   = visualLineWidth;
        lr.material   = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = visualColor;
        lr.endColor   = visualColor;

        const int arcSegments = 24;
        int total = arcSegments + 3; // 중심 → 호 (arcSegments+1 점) → 중심
        lr.positionCount = total;

        float halfAngle = angleDeg * 0.5f;
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        lr.SetPosition(0, origin);
        for (int i = 0; i <= arcSegments; i++)
        {
            float t  = (float)i / arcSegments;
            float a  = (baseAngle - halfAngle) + t * angleDeg;
            float ra = a * Mathf.Deg2Rad;
            Vector3 p = origin + new Vector3(Mathf.Cos(ra), Mathf.Sin(ra), 0f) * r;
            lr.SetPosition(i + 1, p);
        }
        lr.SetPosition(total - 1, origin);

        // 페이드 아웃
        float elapsed = 0f;
        while (elapsed < visualDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / visualDuration);
            var c = visualColor;
            c.a = alpha;
            lr.startColor = c;
            lr.endColor   = c;
            yield return null;
        }

        Destroy(fxObj);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
