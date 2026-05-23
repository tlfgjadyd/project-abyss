using UnityEngine;

/// <summary>
/// 가시 발사 — 주기적으로 8방향 직선 데미지 (BoxCast 8개).
/// 발사체 prefab 없이 즉시 raycast 판정. 시각은 LineRenderer 8개 짧은 표시.
/// </summary>
public class SpikeBurst : MonoBehaviour
{
    [Header("Stats")]
    public float range = 6f;
    public float cooldown = 5f;
    public int spikeCount = 8;
    public float spikeWidth = 0.5f;

    [HideInInspector] public float damageMultiplier = 1f;

    [Header("Layer")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Visual")]
    [SerializeField] private Color spikeColor = new Color(1f, 0.9f, 0.3f, 1f);
    [SerializeField] private float visualDuration = 0.2f;
    [SerializeField] private float spikeVisualWidth = 0.15f;

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

        Burst();
        cooldownTimer = cooldown / stats.EffectiveAttackSpeed;
        PlayerSkillEvents.OnSkillUsed?.Invoke();
    }

    void Burst()
    {
        float damage = stats.attackPower * damageMultiplier;
        Vector2 origin = transform.position;

        for (int i = 0; i < spikeCount; i++)
        {
            float ang = (i / (float)spikeCount) * Mathf.PI * 2f;
            Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
            // 직선 BoxCast
            var hits = Physics2D.BoxCastAll(origin, new Vector2(spikeWidth, 0.1f),
                ang * Mathf.Rad2Deg, dir, range, enemyLayer);
            foreach (var rh in hits)
            {
                if (rh.collider == null) continue;
                rh.collider.GetComponent<IDamageable>()?.TakeDamage(damage);
            }
            SpawnSpikeVisual(origin, origin + dir * range);
        }
    }

    void SpawnSpikeVisual(Vector2 a, Vector2 b)
    {
        var fx = new GameObject("SpikeFx");
        var lr = fx.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = spikeVisualWidth;
        lr.endWidth = 0f;
        lr.startColor = spikeColor;
        lr.endColor = spikeColor;
        lr.positionCount = 2;
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);
        Destroy(fx, visualDuration);
    }
}
