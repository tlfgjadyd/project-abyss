using System.Collections;
using UnityEngine;

/// <summary>
/// 음파 진동 — 주기적으로 플레이어 주위 광역 펄스. 적에게 데미지 + 넉백.
/// BioticExplosion 패턴 (자동 발동) + Slash의 넉백 응용.
/// VFX: 에셋 없이 코드로 그린 충격파 Ring(작게 시작 → 사거리까지 확장 → 페이드 → 소멸).
/// </summary>
public class SonicPulse : MonoBehaviour
{
    [Header("Stats")]
    public float range = 3.5f;
    public float cooldown = 5f;        // Day 48b 너프: 3.5 → 5
    public float knockbackForce = 7f;

    [Tooltip("넉백 활성 여부. SkillEffectApplier가 Lv4 도달 시 true로 설정 (4Lv 보상)")]
    public bool knockbackEnabled = false;

    /// <summary>돌연변이/패시브 곱셈. 기본 1.0</summary>
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
            // 넉백은 Lv4 도달 시에만 (SkillEffectApplier가 knockbackEnabled 토글)
            if (!knockbackEnabled) continue;
            var enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                Vector2 dir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                if (dir.sqrMagnitude < 0.0001f) dir = Vector2.right;
                enemy.ApplyKnockback(dir, knockbackForce, 0.25f);
            }
        }
        SpawnPulseVfx(transform.position);
    }

    // 충격파 Ring VFX 튜닝 (인스펙터 조절)
    [Header("VFX (충격파 Ring)")]
    [SerializeField] private Color ringColor = new Color(0.5f, 0.85f, 1f, 0.7f);
    [Tooltip("사거리까지 펼쳐지는 시간 — 즉발 판정과 시각이 일치하도록 매우 짧게")]
    [SerializeField] private float ringExpandTime = 0.1f;
    [Tooltip("사거리 도달 후 머무르며 사라지는 시간")]
    [SerializeField] private float ringFadeTime = 0.28f;
    [SerializeField] private float ringWidth = 0.18f;
    [SerializeField] private float ringStartRatio = 0.3f;   // 시작 반경 = range × 이 값

    void SpawnPulseVfx(Vector2 origin)
    {
        var go = new GameObject("SonicRing");
        go.transform.position = origin;
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;     // 로컬 원 → transform.localScale로 반경 확장
        lr.loop = true;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = ringWidth; lr.endWidth = ringWidth;
        lr.startColor = ringColor; lr.endColor = ringColor;
        const int seg = 48;
        lr.positionCount = seg;
        for (int i = 0; i < seg; i++)
        {
            float a = i / (float)seg * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f)); // 단위 원(반경 1)
        }
        StartCoroutine(RingRoutine(go, lr));
    }

    IEnumerator RingRoutine(GameObject go, LineRenderer lr)
    {
        float startR = range * ringStartRatio, endR = range;
        Color c0 = ringColor;

        // 1) 사거리까지 빠르게 펼침 (즉발 판정과 시각 일치)
        float t = 0f;
        while (t < ringExpandTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / Mathf.Max(0.0001f, ringExpandTime));
            go.transform.localScale = Vector3.one * Mathf.Lerp(startR, endR, k);
            yield return null;
        }
        go.transform.localScale = Vector3.one * endR;

        // 2) 사거리 유지하며 페이드아웃 (이미 사거리 전체를 덮은 상태로 사그라듦)
        t = 0f;
        while (t < ringFadeTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(c0.a, 0f, Mathf.Clamp01(t / Mathf.Max(0.0001f, ringFadeTime)));
            var c = new Color(c0.r, c0.g, c0.b, a);
            lr.startColor = c; lr.endColor = c;
            yield return null;
        }
        Destroy(go);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.7f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
