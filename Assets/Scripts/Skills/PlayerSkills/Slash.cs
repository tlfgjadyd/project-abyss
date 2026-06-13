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

        // 시각: sprite 프레임 VFX (Resources/VFX/Slash) — 사거리 반영
        SpawnSlashVfx(transform.position, direction, effRange);

        PlayerSkillEvents.OnSkillUsed?.Invoke();
    }

    // VFX 튜닝 상수 (눈으로 보고 조절)
    const float VfxScalePerRange = 0.22f;   // 크기 = 사거리 × 이 값 (과성장 촉수로 사거리↑ → 이펙트도 커짐). 기본 사거리에서 ≈0.8
    const float VfxForwardPerRange = 0.4f;  // 앞으로 띄우는 거리 = 사거리 × 이 값 (사거리↑ → 더 멀리 투사)
    const float VfxAngleOffset = 0f;        // sprite 기본 방향이 +x가 아니면 보정(예: 위 기준이면 -90)

    static GameObject slashVfxPrefab;
    static bool vfxLoaded;

    void SpawnSlashVfx(Vector2 origin, Vector2 dir, float effRange)
    {
        if (!vfxLoaded)
        {
            slashVfxPrefab = Resources.Load<GameObject>("VFX/Slash");
            vfxLoaded = true;
        }
        if (slashVfxPrefab == null) return;

        float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + VfxAngleOffset;
        Vector3 pos = (Vector3)(origin + dir.normalized * (effRange * VfxForwardPerRange));
        var go = Instantiate(slashVfxPrefab, pos, Quaternion.Euler(0f, 0f, angleDeg));
        go.transform.localScale = Vector3.one * (effRange * VfxScalePerRange);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
