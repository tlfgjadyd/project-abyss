using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 감전 필드 (전기 기관 Lv4) — 적 처치 위치에 생성되는 반투명 원형 지속 필드.
/// 1초마다 데미지 + 둔화, 누적 데미지가 임계치(기본 8 = 4초 체류)에 도달하면 2초 기절.
/// 누적/기절 진행은 적별(EnemyStatusEffects)에 저장 → 필드↔필드 연속 이동 시 유지, 이탈 시 리셋.
/// 동시 최대 개수 제한(오래된 것부터 제거). 보스는 기절 면역(자동).
/// </summary>
public class ElectricFieldZone : MonoBehaviour
{
    private const int MaxFields = 3;
    private static readonly List<ElectricFieldZone> active = new List<ElectricFieldZone>();

    private float radius, lifetime, tickInterval, tickDamage;
    private float slowMul, slowDur, stunThreshold, stunDuration;
    private LayerMask enemyLayer;

    private float lifeTimer, tickTimer;
    private static Sprite softCircle;

    public static void Spawn(Vector2 pos, float radius, LayerMask layer,
        float lifetime, float tickInterval, float tickDamage,
        float slowMul, float slowDur, float stunThreshold, float stunDuration)
    {
        // 최대 개수 초과 시 가장 오래된 것 제거
        while (active.Count >= MaxFields)
        {
            var oldest = active[0];
            active.RemoveAt(0);
            if (oldest != null) Destroy(oldest.gameObject);
        }

        var go = new GameObject("ElectricField");
        go.transform.position = pos;
        var z = go.AddComponent<ElectricFieldZone>();
        z.radius = radius; z.enemyLayer = layer;
        z.lifetime = lifetime; z.tickInterval = tickInterval; z.tickDamage = tickDamage;
        z.slowMul = slowMul; z.slowDur = slowDur;
        z.stunThreshold = stunThreshold; z.stunDuration = stunDuration;
        z.Setup();
    }

    void Setup()
    {
        lifeTimer = lifetime;
        tickTimer = tickInterval;
        active.Add(this);

        // 반투명 채움 원 시각 (전기뱀장어 필드 톤)
        var sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = GetSoftCircle();
        sr.color = new Color(0.4f, 0.85f, 1f, 0.18f);
        sr.sortingOrder = -1;
        transform.localScale = Vector3.one * (radius * 2f);
    }

    void OnDestroy()
    {
        active.Remove(this);
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0f) { Destroy(gameObject); return; }

        tickTimer -= Time.deltaTime;
        if (tickTimer <= 0f)
        {
            tickTimer = tickInterval;
            DoTick();
        }
    }

    void DoTick()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
        foreach (var c in hits)
        {
            if (c == null) continue;
            c.GetComponent<IDamageable>()?.TakeDamage(tickDamage);
            var sr = c.GetComponent<IStatusReceiver>();
            if (sr != null) sr.ApplySlow(slowMul, slowDur);
            c.GetComponent<EnemyStatusEffects>()?.AddElectricFieldTick(tickDamage, tickInterval, stunThreshold, stunDuration);
        }
    }

    static Sprite GetSoftCircle()
    {
        if (softCircle != null) return softCircle;
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        float rad = size * 0.5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) - rad, dy = (y + 0.5f) - rad;
                float d = Mathf.Sqrt(dx * dx + dy * dy) / rad;
                float a = d >= 1f ? 0f : Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(0.7f, 1f, d));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        softCircle = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return softCircle;
    }
}
