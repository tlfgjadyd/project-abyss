using UnityEngine;

/// <summary>
/// 음파 진동 — 주기적으로 플레이어 주위 광역 펄스. 적에게 데미지 + 넉백.
/// BioticExplosion 패턴 (자동 발동) + Slash의 넉백 응용.
/// </summary>
public class SonicPulse : MonoBehaviour
{
    [Header("Stats")]
    public float range = 3.5f;
    public float cooldown = 3.5f;
    public float knockbackForce = 7f;

    /// <summary>돌연변이/패시브 곱셈. 기본 1.0</summary>
    [HideInInspector] public float damageMultiplier = 1f;

    [Header("Layer")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Visual")]
    [SerializeField] private Color pulseColor = new Color(0.4f, 0.7f, 1f, 0.6f);
    [SerializeField] private float visualDuration = 0.3f;

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

        Pulse();
        cooldownTimer = cooldown / stats.EffectiveAttackSpeed;
        PlayerSkillEvents.OnSkillUsed?.Invoke();
    }

    void Pulse()
    {
        float damage = stats.attackPower * damageMultiplier;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
        foreach (var hit in hits)
        {
            hit.GetComponent<IDamageable>()?.TakeDamage(damage);
            // 넉백
            var rb = hit.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
                rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
            }
        }
        SpawnVisual();
    }

    void SpawnVisual()
    {
        // 짧은 시각 표시 — LineRenderer 원형 (16각형)
        var fx = new GameObject("SonicPulseFx");
        fx.transform.position = transform.position;
        var lr = fx.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = 0.08f;
        lr.endWidth = 0.08f;
        lr.startColor = pulseColor;
        lr.endColor = pulseColor;
        int seg = 32;
        lr.positionCount = seg + 1;
        for (int i = 0; i <= seg; i++)
        {
            float a = i / (float)seg * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * range, Mathf.Sin(a) * range, 0f));
        }
        Destroy(fx, visualDuration);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.7f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
