using System.Collections;
using UnityEngine;

/// <summary>
/// 발광 폭주 — 플레이어 주변에 8개 발광 노드 생성 (120E).
/// 적이 노드에 닿으면 폭발 + 광역 피해 + 넉백. 일정 시간 후 자동 소멸.
/// 3스테이지 산갈치 보스 카피 스킬.
/// </summary>
public class GlowFrenzySkill : CopySkillBase
{
    [Header("Nodes")]
    [SerializeField] private int nodeCount = 8;
    [Tooltip("플레이어 중심으로부터 노드까지의 거리")]
    [SerializeField] private float nodeRadius = 2.5f;
    [Tooltip("노드 유지 시간 (이 시간이 지나면 폭발 없이 사라짐)")]
    [SerializeField] private float nodeLifetime = 6f;

    [Header("Trigger")]
    [Tooltip("적이 이 거리 안에 들어오면 폭발")]
    [SerializeField] private float triggerRadius = 0.7f;
    [Tooltip("폭발 광역 반경")]
    [SerializeField] private float explosionRadius = 1.6f;

    [Header("Damage")]
    [Tooltip("기본 공격력 대비 폭발 데미지 배율")]
    [SerializeField] private float damageMultiplier = 2.0f;
    [Tooltip("넉백 강도")]
    [SerializeField] private float knockbackForce = 8f;

    [Header("Layer")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Visual")]
    [SerializeField] private Color nodeColor = new Color(0.4f, 1f, 0.9f, 1f);
    [SerializeField] private float nodeScale = 0.4f;

    public override void Execute()
    {
        Vector2 origin = transform.position;
        for (int i = 0; i < nodeCount; i++)
        {
            float angle = (360f / nodeCount) * i;
            float rad   = angle * Mathf.Deg2Rad;
            Vector2 pos = origin + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * nodeRadius;
            SpawnNode(pos);
        }
    }

    void SpawnNode(Vector2 pos)
    {
        var node = new GameObject("GlowFrenzyNode");
        node.transform.position = pos;
        node.transform.localScale = Vector3.one * nodeScale;

        // 시각 (작은 sprite. 동적 생성 — 둥근 sprite는 1x1 흰 텍스처로)
        var sr = node.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color  = nodeColor;
        sr.sortingOrder = 5;

        // 충돌 검출용
        var col = node.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = triggerRadius / nodeScale; // localScale 보정

        var trigger = node.AddComponent<GlowFrenzyNode>();
        trigger.Initialize(stats != null ? stats.attackPower * damageMultiplier : damageMultiplier,
                           explosionRadius,
                           knockbackForce,
                           nodeLifetime,
                           enemyLayer);
    }

    static Sprite cachedCircle;
    static Sprite CreateCircleSprite()
    {
        if (cachedCircle != null) return cachedCircle;
        const int size = 32;
        var tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Bilinear;
        Color clear = new Color(1, 1, 1, 0);
        Color fill  = Color.white;
        Vector2 c = new Vector2(size * 0.5f, size * 0.5f);
        float r = size * 0.5f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float d = Vector2.Distance(new Vector2(x, y), c);
            tex.SetPixel(x, y, d <= r ? fill : clear);
        }
        tex.Apply();
        cachedCircle = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return cachedCircle;
    }
}
