using UnityEngine;

/// <summary>
/// 발광 기관 — 일정 쿨다운마다 1회 흡수 보호막 활성화.
/// PlayerStats.TakeDamage가 glowShieldActive 체크하여 1회 데미지 무효.
/// 보호막 소모 후 cooldown 진행 → 다시 활성.
/// </summary>
public class GlowOrgan : MonoBehaviour
{
    [Header("Shield")]
    public float cooldown = 12f;

    [Header("Visual")]
    [SerializeField] private Color shieldColor = new Color(1f, 0.95f, 0.4f, 0.7f);
    [SerializeField] private float shieldRadius = 0.6f;

    private PlayerStats stats;
    private float cooldownTimer;
    private GameObject shieldFx;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    void OnEnable()
    {
        if (stats != null) stats.OnGlowShieldConsumed += OnShieldConsumed;
        // 활성화 즉시 보호막 활성
        ActivateShield();
    }

    void OnDisable()
    {
        if (stats != null) stats.OnGlowShieldConsumed -= OnShieldConsumed;
        DestroyShieldFx();
        if (stats != null) stats.glowShieldActive = false;
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (stats.glowShieldActive) return;

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
            ActivateShield();
    }

    void ActivateShield()
    {
        stats.glowShieldActive = true;
        SpawnShieldFx();
    }

    void OnShieldConsumed()
    {
        DestroyShieldFx();
        cooldownTimer = cooldown;
    }

    void SpawnShieldFx()
    {
        if (shieldFx != null) return;
        shieldFx = new GameObject("GlowShieldFx");
        shieldFx.transform.SetParent(transform, false);
        shieldFx.transform.localPosition = Vector3.zero;
        var lr = shieldFx.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startWidth = 0.08f;
        lr.endWidth = 0.08f;
        lr.startColor = shieldColor;
        lr.endColor = shieldColor;
        int seg = 24;
        lr.positionCount = seg + 1;
        for (int i = 0; i <= seg; i++)
        {
            float a = i / (float)seg * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * shieldRadius, Mathf.Sin(a) * shieldRadius, 0f));
        }
    }

    void DestroyShieldFx()
    {
        if (shieldFx != null) { Destroy(shieldFx); shieldFx = null; }
    }
}
