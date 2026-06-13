using UnityEngine;

/// <summary>
/// 흡혈 촉수 — 주기적으로 가장 가까운 적 1체에 라인 형태 데미지 + 받은 데미지의 일정 비율 흡혈.
/// 시각: LineRenderer 짧은 표시 (플레이어 → 적).
/// </summary>
public class DrainTentacle : MonoBehaviour
{
    [Header("Stats")]
    public float range = 5f;
    public float cooldown = 2f;
    [Range(0f, 1f)] public float lifestealRatio = 0.1f; // deprecated — lifestealFlat 사용
    [Tooltip("1회 발동당 고정 회복량 (데미지 비례 대신 고정). 10회 = 1HP")]
    public float lifestealFlat = 0.1f;

    [HideInInspector] public float damageMultiplier = 1f;

    [Header("Bleed (Lv4)")]
    [Tooltip("SkillEffectApplier가 Lv4에서 true로 설정")]
    [HideInInspector] public bool bleedEnabled = false;
    public float bleedDuration = 3f;
    public float bleedTickInterval = 0.4f;
    [Tooltip("1스택 1틱 데미지 = attackPower × 이 값 (× 출혈 배율)")]
    public float bleedDamageMultiplier = 0.25f;

    [Header("Layer")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Visual")]
    [SerializeField] private Color tentacleColor = new Color(0.7f, 0.05f, 0.15f, 1f);
    [SerializeField] private float visualDuration = 0.25f;
    [SerializeField] private float visualWidth = 0.2f;

    private PlayerStats stats;
    private float cooldownTimer;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (stats.IsStunned) return;

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer > 0f) return;

        Drain();
        cooldownTimer = cooldown / stats.EffectiveAttackSpeed;
        PlayerSkillEvents.OnSkillUsed?.Invoke();
    }

    void Drain()
    {
        // 가장 가까운 적 1체 (시야 제한 반영)
        float effRange = stats != null ? stats.ApplyAutoTrackLimit(range) : range;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, effRange, enemyLayer);
        if (hits.Length == 0) return;

        Collider2D closest = null;
        float minSq = float.MaxValue;
        foreach (var c in hits)
        {
            if (c == null) continue;
            float d = ((Vector2)c.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (d < minSq) { minSq = d; closest = c; }
        }
        if (closest == null) return;

        float damage = stats.attackPower * damageMultiplier;
        closest.GetComponent<IDamageable>()?.TakeDamage(damage);
        stats.Heal(lifestealFlat); // 고정 회복 (데미지 무관)

        if (bleedEnabled)
        {
            float tick = stats.attackPower * bleedDamageMultiplier * stats.bleedDamageMultiplier;
            closest.GetComponent<IStatusReceiver>()?.ApplyBleed(tick, bleedDuration, bleedTickInterval,
                                                                EnemyStatusEffects.DefaultBleedMaxStacks);
        }

        SpawnVisual(transform.position, closest.transform.position);
    }

    void SpawnVisual(Vector2 a, Vector2 b)
    {
        var fx = new GameObject("DrainFx");
        var lr = fx.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = visualWidth;
        lr.endWidth = visualWidth * 0.5f;
        lr.startColor = tentacleColor;
        lr.endColor = tentacleColor;
        lr.positionCount = 2;
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);
        Destroy(fx, visualDuration);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.7f, 0.05f, 0.15f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
