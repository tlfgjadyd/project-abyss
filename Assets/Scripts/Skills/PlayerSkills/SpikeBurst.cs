using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 가시 발사 — 주기적으로 8방향 직선 데미지 (BoxCast 8개).
/// 발사체 prefab 없이 즉시 raycast 판정. 시각은 LineRenderer 8개 짧은 표시.
///
/// Day 50 리워크: 단순 데미지 → 유틸 강화.
///   Lv1~4: 맞은 적에 둔화(이속 ×slowMultiplier, slowDuration초) 부여.
///   Lv4  : 추가로 출혈 DoT 부여 (BleedSwim식). SkillEffectApplier가 bleedEnabled 활성.
/// </summary>
public class SpikeBurst : MonoBehaviour
{
    [Header("Stats")]
    public float range = 6f;
    public float cooldown = 5f;
    public int spikeCount = 8;
    public float spikeWidth = 0.5f;

    [HideInInspector] public float damageMultiplier = 1f;

    [Header("Slow (전 레벨)")]
    [Tooltip("맞은 적 이동속도 배율 (0.6 = 60%)")]
    public float slowMultiplier = 0.6f;
    [Tooltip("둔화 지속 시간")]
    public float slowDuration = 1.5f;

    [Header("Bleed (Lv4)")]
    [Tooltip("SkillEffectApplier가 Lv4에서 true로 설정")]
    [HideInInspector] public bool bleedEnabled = false;
    [Tooltip("출혈 지속 시간")]
    public float bleedDuration = 3f;
    [Tooltip("출혈 틱 간격")]
    public float bleedTickInterval = 0.4f;
    [Tooltip("1스택 1틱 데미지 = attackPower × 이 값")]
    public float bleedDamageMultiplier = 0.25f;

    [Header("Layer")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Visual")]
    [SerializeField] private Color spikeColor = new Color(1f, 0.9f, 0.3f, 1f);
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

        // 이번 발사에서 둔화/출혈을 이미 부여한 대상 (방향 중복 방지)
        var debuffedThisBurst = new HashSet<IStatusReceiver>();

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

                var eb = rh.collider.GetComponent<IStatusReceiver>();
                if (eb != null && !debuffedThisBurst.Contains(eb))
                {
                    debuffedThisBurst.Add(eb);
                    eb.ApplySlow(slowMultiplier, slowDuration);
                    if (bleedEnabled)
                        eb.ApplyBleed(stats.attackPower * bleedDamageMultiplier * stats.bleedDamageMultiplier,
                                      bleedDuration, bleedTickInterval, EnemyStatusEffects.DefaultBleedMaxStacks);
                }
            }
            // 짧은 가시가 origin → range로 슉 뻗어나가는 발사 모션
            SpawnSpikeShot(origin, dir, range);
        }
    }

    // 발사 모션 튜닝 (눈으로 보고 조절)
    [Header("Shot Motion")]
    [Tooltip("가시가 사거리 끝까지 도달하는 시간(작을수록 빠름)")]
    [SerializeField] private float shotTravelTime = 0.09f;
    [Tooltip("날아가는 가시 세그먼트 길이")]
    [SerializeField] private float shotSpikeLength = 0.8f;
    [Tooltip("도달 후 사라지는 페이드 시간")]
    [SerializeField] private float shotFadeTime = 0.06f;

    /// <summary>짧은 가시 세그먼트가 중심에서 바깥으로 빠르게 뻗어나가는 발사 비주얼(에셋 불필요).</summary>
    void SpawnSpikeShot(Vector2 origin, Vector2 dir, float maxRange)
    {
        var fx = new GameObject("SpikeShot");
        var lr = fx.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = spikeVisualWidth;
        lr.endWidth = 0f;
        lr.startColor = spikeColor;
        lr.endColor = spikeColor;
        lr.positionCount = 2;
        StartCoroutine(SpikeShotRoutine(fx, lr, origin, dir, maxRange));
    }

    IEnumerator SpikeShotRoutine(GameObject fx, LineRenderer lr, Vector2 origin, Vector2 dir, float maxRange)
    {
        float spikeLen = Mathf.Min(shotSpikeLength, maxRange * 0.5f);
        float t = 0f;
        while (t < shotTravelTime)
        {
            t += Time.deltaTime;
            float head = Mathf.Lerp(0f, maxRange, t / shotTravelTime);
            float tail = Mathf.Max(0f, head - spikeLen);
            lr.SetPosition(0, origin + dir * tail);
            lr.SetPosition(1, origin + dir * head);
            yield return null;
        }

        // 끝점에서 짧게 페이드
        float f = 0f;
        while (f < shotFadeTime)
        {
            f += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, f / shotFadeTime);
            var c = new Color(spikeColor.r, spikeColor.g, spikeColor.b, a);
            lr.startColor = c; lr.endColor = c;
            yield return null;
        }
        Destroy(fx);
    }
}
