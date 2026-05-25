using UnityEngine;

public class Slash : MonoBehaviour
{
    [Header("Stats")]
    public float range = 3f;
    [Range(0f, 360f)] public float angle = 100f;

    /// <summary>돌연변이 등에서 곱셈으로 적용. 기본 1.0</summary>
    [HideInInspector] public float damageMultiplier = 1f;

    [Header("Knockback (Lv4)")]
    public bool knockbackEnabled;
    [SerializeField] private float knockbackForce = 5f;

    [Header("Layer")]
    [SerializeField] private LayerMask enemyLayer;

    public void Execute(Vector2 direction, float baseDamage)
    {
        float damage = baseDamage * damageMultiplier;
        var stats = GetComponent<PlayerStats>();
        float effRange = stats != null ? stats.ApplyAutoTrackLimit(range) : range;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, effRange, enemyLayer);
        foreach (var hit in hits)
        {
            Vector2 toEnemy = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
            if (Vector2.Angle(direction, toEnemy) > angle * 0.5f) continue;

            hit.GetComponent<IDamageable>()?.TakeDamage(damage);

            if (knockbackEnabled)
            {
                var enemy = hit.GetComponent<EnemyBase>();
                if (enemy != null) enemy.ApplyKnockback(toEnemy, knockbackForce, 0.25f);
            }
        }

        // 시각: 부채꼴 호 페이드 (시야 제한 반영한 effRange 사용)
        SpawnSlashFx(transform.position, direction, effRange);

        PlayerSkillEvents.OnSkillUsed?.Invoke();
    }

    void SpawnSlashFx(Vector2 origin, Vector2 dir, float effRange)
    {
        var fx = new GameObject("SlashFx");
        var lr = fx.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = 0.15f; lr.endWidth = 0.05f;
        var color = new Color(1f, 0.85f, 0.7f, 1f);
        lr.startColor = color; lr.endColor = color;
        const int seg = 16;
        lr.positionCount = seg + 1;
        float halfA = angle * 0.5f;
        float baseA = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        for (int i = 0; i <= seg; i++)
        {
            float t = i / (float)seg;
            float a = (baseA - halfA + t * angle) * Mathf.Deg2Rad;
            lr.SetPosition(i, origin + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * effRange);
        }
        fx.AddComponent<SkillFxFader>().Init(lr, color, 0.18f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
