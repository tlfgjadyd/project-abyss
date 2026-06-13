using System.Collections;
using UnityEngine;

/// <summary>
/// 심해 압박 — 주변 적 이동속도 감소 + 받는 피해 증가 디버프 (80E).
/// 2스테이지 향유고래 보스 카피 스킬.
/// </summary>
public class DeepPressureSkill : CopySkillBase
{
    [Header("Stats")]
    [SerializeField] private float radius = 5f;
    [SerializeField] private float duration = 4f;
    [Tooltip("이동속도 배율. 0.5 = -50%")]
    [Range(0f, 1f)]
    [SerializeField] private float slowMultiplier = 0.5f;
    [Tooltip("받는 피해 배율. 1.5 = +50%")]
    [Min(1f)]
    [SerializeField] private float vulnerabilityMultiplier = 1.5f;

    [Header("Layer")]
    [SerializeField] private LayerMask enemyLayer;

    public override void Execute()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<IStatusReceiver>();
            if (enemy == null) continue;
            enemy.ApplySlow(slowMultiplier, duration);
            enemy.ApplyVulnerability(vulnerabilityMultiplier, duration);
        }

        SpawnVfx(transform.position, radius);
    }

    // ── 절차적 VFX: 반투명 보라 원이 바깥→안으로 수축하며 페이드 (압박 컨셉, 에셋 0) ──
    [Header("VFX")]
    [SerializeField] private Color vfxColor = new Color(0.5f, 0.2f, 0.85f, 0.45f);
    [SerializeField] private float vfxDuration = 0.6f;

    private static Sprite softCircle;

    void SpawnVfx(Vector2 origin, float r)
    {
        var go = new GameObject("DeepPressureVfx");
        go.transform.position = origin;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetSoftCircle();
        sr.color = vfxColor;
        sr.sortingOrder = 45;
        StartCoroutine(VfxRoutine(go, sr, r));
    }

    IEnumerator VfxRoutine(GameObject go, SpriteRenderer sr, float r)
    {
        float startScale = r * 2f * 1.15f;   // 사거리보다 약간 크게 시작
        float endScale   = r * 2f * 0.2f;    // 중심으로 수축
        Color c0 = vfxColor;
        float t = 0f;
        while (t < vfxDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / vfxDuration);
            go.transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, k);
            float a = Mathf.Lerp(c0.a, 0f, k * k);   // 후반 가속 페이드
            sr.color = new Color(c0.r, c0.g, c0.b, a);
            yield return null;
        }
        Destroy(go);
    }

    /// <summary>부드러운 가장자리의 채움 원 스프라이트 (정적 1회 생성). 지름 1 world unit.</summary>
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
                float a = d >= 1f ? 0f : Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(0.65f, 1f, d));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        softCircle = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return softCircle;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.2f, 0.8f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
