using UnityEngine;

public class ElectricEngine : MonoBehaviour
{
    [Header("Stats")]
    public float detectionRadius = 5f;
    public float cooldown = 2f;
    public float chainRadius = 2.5f;
    public float chainDamageRatio = 0.6f;
    public int maxChainTargets = 3;

    [Header("Shock (감전 둔화)")]
    [Tooltip("감전된 적 이동속도 배율 (0.7 = 70%)")]
    public float shockSlowMultiplier = 0.7f;
    [Tooltip("감전 둔화 지속 시간")]
    public float shockSlowDuration = 1f;

    [Header("Shock Field (Lv4 — 사망 시 감전 필드)")]
    [Tooltip("SkillEffectApplier가 Lv4에서 true로 설정")]
    [HideInInspector] public bool fieldEnabled = false;
    public float fieldRadius = 3.5f;
    public float fieldLifetime = 7f;
    public float fieldTickInterval = 1f;
    public float fieldTickDamage = 2f;
    public float fieldSlowMultiplier = 0.6f;
    public float fieldSlowDuration = 1.3f;
    public float fieldStunThreshold = 8f;     // 누적 8 = 4틱(4초) 체류 시 기절
    public float fieldStunDuration = 2f;

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

        Transform primary = FindNearestEnemy();
        if (primary == null) return;

        Fire(primary);
        cooldownTimer = cooldown / stats.EffectiveAttackSpeed;
        PlayerSkillEvents.OnSkillUsed?.Invoke();
    }

    Transform FindNearestEnemy()
    {
        float effRange = stats != null ? stats.ApplyAutoTrackLimit(detectionRadius) : detectionRadius;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, effRange, enemyLayer);
        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            float dist = Vector2.Distance(transform.position, hit.transform.position);
            if (dist < minDist) { minDist = dist; nearest = hit.transform; }
        }
        return nearest;
    }

    void Fire(Transform primary)
    {
        float baseDamage = stats.attackPower * damageMultiplier;

        // 1차 피해 + 감전 둔화 + 시각 (Player → primary Z호)
        primary.GetComponent<IDamageable>()?.TakeDamage(baseDamage);
        primary.GetComponent<IStatusReceiver>()?.ApplySlow(shockSlowMultiplier, shockSlowDuration);
        SpawnZigZag(transform.position, primary.position);
        SpawnElectricVfx(primary.position);
        TrySpawnField(primary);

        // 연쇄 피해 (1차 대상 주변)
        Collider2D[] chainHits = Physics2D.OverlapCircleAll(primary.position, chainRadius, enemyLayer);
        int chainCount = 0;
        float chainDamage = baseDamage * chainDamageRatio;

        foreach (var hit in chainHits)
        {
            if (hit.transform == primary) continue;
            if (chainCount >= maxChainTargets) break;

            hit.GetComponent<IDamageable>()?.TakeDamage(chainDamage);
            hit.GetComponent<IStatusReceiver>()?.ApplySlow(shockSlowMultiplier, shockSlowDuration);
            // 연쇄 시각 (primary → 보조 타겟)
            SpawnZigZag(primary.position, hit.transform.position);
            SpawnElectricVfx(hit.transform.position);
            TrySpawnField(hit.transform);
            chainCount++;
        }

        // TODO Week 3: Lv4 — 사망 시 감전 필드 생성
    }

    // VFX 튜닝 상수 (눈으로 보고 조절)
    const float VfxScale = 0.8f;
    static GameObject vfxPrefab;
    static bool vfxLoaded;

    /// <summary>Lv4 — 적이 이번 타격으로 사망했으면 그 위치에 감전 필드 생성</summary>
    void TrySpawnField(Transform enemyT)
    {
        if (!fieldEnabled || enemyT == null) return;
        var eb = enemyT.GetComponent<EnemyBase>();
        if (eb == null || !eb.IsDead) return;   // 일반 적 사망 시에만(보스 제외)
        ElectricFieldZone.Spawn(enemyT.position, fieldRadius, enemyLayer,
            fieldLifetime, fieldTickInterval, fieldTickDamage,
            fieldSlowMultiplier, fieldSlowDuration, fieldStunThreshold, fieldStunDuration);
    }

    /// <summary>타격 지점에 전기 버스트 sprite VFX (Resources/VFX/ElectricEngine)</summary>
    void SpawnElectricVfx(Vector3 pos)
    {
        if (!vfxLoaded)
        {
            vfxPrefab = Resources.Load<GameObject>("VFX/ElectricEngine");
            vfxLoaded = true;
        }
        if (vfxPrefab == null) return;
        var go = Instantiate(vfxPrefab, pos, Quaternion.identity);
        go.transform.localScale = Vector3.one * VfxScale;
    }

    /// <summary>두 점 사이 zigzag(Z 호) LineRenderer + 페이드</summary>
    void SpawnZigZag(Vector3 from, Vector3 to)
    {
        var fx = new GameObject("ElectricFx");
        var lr = fx.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = 0.08f; lr.endWidth = 0.08f;
        var color = new Color(0.5f, 0.9f, 1f, 1f);
        lr.startColor = color; lr.endColor = color;

        // 3~5 segment zigzag (수직 노이즈)
        const int seg = 6;
        lr.positionCount = seg + 1;
        Vector3 axis = to - from;
        Vector3 perp = new Vector3(-axis.y, axis.x, 0f).normalized * 0.3f;
        for (int i = 0; i <= seg; i++)
        {
            float t = i / (float)seg;
            Vector3 onLine = Vector3.Lerp(from, to, t);
            // 양 끝은 정확히 from/to, 중간만 노이즈
            float noise = (i == 0 || i == seg) ? 0f : (Random.value - 0.5f) * 2f;
            lr.SetPosition(i, onLine + perp * noise);
        }
        fx.AddComponent<SkillFxFader>().Init(lr, color, 0.15f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
