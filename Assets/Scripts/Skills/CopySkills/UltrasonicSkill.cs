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
    [Tooltip("부채꼴 필드 색/투명도")]
    [SerializeField] private Color visualColor = new Color(0.5f, 0.8f, 1f, 0.4f);
    [SerializeField] private float visualDuration = 0.3f;

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

        // 시각 피드백 — 반투명 채워진 부채꼴 필드(Mesh) 잠깐 표시
        StartCoroutine(ShowFanEffect(transform.position, dir, range, angle));
    }

    IEnumerator ShowFanEffect(Vector3 origin, Vector2 dir, float r, float angleDeg)
    {
        var fxObj = new GameObject("UltrasonicFanVfx");
        fxObj.transform.position = origin;
        fxObj.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

        var mf = fxObj.AddComponent<MeshFilter>();
        var mr = fxObj.AddComponent<MeshRenderer>();
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = visualColor;
        mr.material = mat;
        mr.sortingOrder = 45;

        // 부채꼴 triangle fan (로컬: 중심 0,0 → +x 기준 ±half 범위, 반경 r)
        const int seg = 32;
        var verts = new Vector3[seg + 2];
        var cols  = new Color[seg + 2];
        var tris  = new int[seg * 3];
        verts[0] = Vector3.zero;
        cols[0]  = Color.white;
        float half = angleDeg * 0.5f * Mathf.Deg2Rad;
        for (int i = 0; i <= seg; i++)
        {
            float a = Mathf.Lerp(-half, half, (float)i / seg);
            verts[i + 1] = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * r;
            cols[i + 1]  = Color.white;
        }
        for (int i = 0; i < seg; i++)
        {
            tris[i * 3]     = 0;
            tris[i * 3 + 1] = i + 1;
            tris[i * 3 + 2] = i + 2;
        }
        var mesh = new Mesh();
        mesh.vertices  = verts;
        mesh.colors    = cols;     // 흰색 → 색/알파는 mat.color(_Color)로 제어
        mesh.triangles = tris;
        mesh.RecalculateBounds();
        mf.mesh = mesh;

        // 페이드 아웃 (mat.color 알파)
        float elapsed = 0f;
        while (elapsed < visualDuration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(visualColor.a, 0f, elapsed / visualDuration);
            mat.color = new Color(visualColor.r, visualColor.g, visualColor.b, a);
            yield return null;
        }

        Destroy(mesh);
        Destroy(fxObj);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
