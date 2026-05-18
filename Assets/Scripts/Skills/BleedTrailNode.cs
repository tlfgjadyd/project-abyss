using UnityEngine;

/// <summary>
/// BleedSwim 트레일 노드. 일정 시간 동안 자신의 반경 안의 적에게 주기적으로 피해.
/// </summary>
public class BleedTrailNode : MonoBehaviour
{
    private float damage;
    private float radius;
    private float lifetime;
    private float tickInterval;
    private LayerMask enemyLayer;
    private Color color;
    private float age;
    private float tickTimer;
    private SpriteRenderer sr;

    public void Initialize(float damage, float radius, float lifetime, float tickInterval, LayerMask enemyLayer, Color color)
    {
        this.damage       = damage;
        this.radius       = radius;
        this.lifetime     = lifetime;
        this.tickInterval = tickInterval;
        this.enemyLayer   = enemyLayer;
        this.color        = color;

        sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color  = color;
        sr.sortingOrder = -1;
        transform.localScale = Vector3.one * radius * 2f / 32f * 32f; // visual scale
        transform.localScale = Vector3.one * radius;
    }

    void Update()
    {
        age += Time.deltaTime;
        if (age >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        // 시각 페이드
        if (sr != null)
        {
            Color c = color;
            c.a *= Mathf.Lerp(1f, 0f, age / lifetime);
            sr.color = c;
        }

        // 데미지 틱
        tickTimer -= Time.deltaTime;
        if (tickTimer <= 0f)
        {
            tickTimer = tickInterval;
            ApplyTickDamage();
        }
    }

    void ApplyTickDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
        foreach (var c in hits)
            c.GetComponent<IDamageable>()?.TakeDamage(damage);
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
