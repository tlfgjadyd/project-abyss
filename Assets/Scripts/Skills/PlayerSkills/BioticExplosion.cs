using UnityEngine;

public class BioticExplosion : MonoBehaviour
{
    [Header("Stats")]
    public float range = 2.5f;
    public float cooldown = 4f;
    public float stunDuration = 0f;     // 기본 0 → Lv4에서 stunDurationDelta(0.5)로만 스턴 부여

    /// <summary>돌연변이 등에서 곱셈으로 적용. 기본 1.0</summary>
    [HideInInspector] public float damageMultiplier = 1f;

    [Header("Layer")]
    [SerializeField] private LayerMask enemyLayer;

    private PlayerStats stats;
    private float cooldownTimer;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        if (stats.IsStunned) return;

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer > 0f) return;

        Explode();
        cooldownTimer = cooldown / stats.EffectiveAttackSpeed;
        PlayerSkillEvents.OnSkillUsed?.Invoke();
    }

    void Explode()
    {
        float damage = stats.attackPower * damageMultiplier;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
        foreach (var hit in hits)
        {
            hit.GetComponent<IDamageable>()?.TakeDamage(damage);
            if (stunDuration > 0f)
                hit.GetComponent<IStatusReceiver>()?.Stun(stunDuration); // 보스는 Stun 면역(무시)
        }
        // 시각: 폭발 중심에 sprite VFX (범위에 비례한 크기)
        SpawnExplosionVfx(transform.position);
    }

    // VFX 튜닝 상수 (눈으로 보고 조절)
    const float VfxScalePerRange = 3.1f;   // 최종 크기 = range × 이 값 (범위 커지면 폭발도 커짐)
    static GameObject vfxPrefab;
    static bool vfxLoaded;

    void SpawnExplosionVfx(Vector2 origin)
    {
        if (!vfxLoaded)
        {
            vfxPrefab = Resources.Load<GameObject>("VFX/BioticExplosion");
            vfxLoaded = true;
        }
        if (vfxPrefab == null) return;
        var go = Instantiate(vfxPrefab, (Vector3)origin, Quaternion.identity);
        go.transform.localScale = Vector3.one * (range * VfxScalePerRange);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.5f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
