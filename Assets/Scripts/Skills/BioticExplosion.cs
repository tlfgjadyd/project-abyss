using UnityEngine;

public class BioticExplosion : MonoBehaviour
{
    [Header("Stats")]
    public float range = 2.5f;
    public float cooldown = 4f;
    public float stunDuration = 0.5f;   // Lv4: stunDurationDelta로 증가

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
                hit.GetComponent<EnemyBase>()?.Stun(stunDuration);
        }
        // 시각: 폭발 반경 원 페이드
        SpawnExplosionFx(transform.position, range);
    }

    void SpawnExplosionFx(Vector2 origin, float r)
    {
        var fx = new GameObject("BioticExplosionFx");
        fx.transform.position = origin;
        var lr = fx.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = 0.12f; lr.endWidth = 0.12f;
        var color = new Color(0.3f, 1f, 0.5f, 1f);
        lr.startColor = color; lr.endColor = color;
        const int seg = 32;
        lr.positionCount = seg + 1;
        for (int i = 0; i <= seg; i++)
        {
            float a = i / (float)seg * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f));
        }
        fx.AddComponent<SkillFxFader>().Init(lr, color, 0.3f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.5f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
