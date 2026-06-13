using UnityEngine;

/// <summary>
/// 전기뱀장어 전기 필드 — 주위 일정 반경 안에 플레이어가 있으면 지속 데미지.
/// 시각: 원형 윤곽 + 내부 회전하는 zigzag bolt 4~6개.
/// Enemy_ElectricEel prefab에 부착.
/// </summary>
public class ElectricEelAura : MonoBehaviour
{
    [Header("Damage Field")]
    [Tooltip("플레이어가 이 반경 안에 있으면 데미지 (world units)")]
    [SerializeField] private float radius = 1.5f;
    [Tooltip("틱당 데미지")]
    [SerializeField] private float damagePerTick = 5f;
    [Tooltip("데미지 틱 간격 (초)")]
    [SerializeField] private float tickInterval = 0.5f;

    [Header("Visual")]
    [SerializeField] private int boltCount = 5;
    [SerializeField] private Color color = new Color(0.5f, 0.95f, 1f, 1f);
    [SerializeField] private float lineWidth = 0.06f;
    [SerializeField] private float rotateSpeed = 90f; // 도/초
    [SerializeField] private float flickerInterval = 0.1f;

    private const float BoltReach = 0.5f;   // 내부 전기 길이 = radius × 이 값 (축소)

    private LineRenderer[] bolts;
    private SpriteRenderer field;           // 반투명 채움 원 (윤곽선 대체)
    private static Sprite fieldSprite;      // 부드러운 원 스프라이트 (정적 1회 생성)
    private float angleOffset;
    private float flickerTimer;
    private float damageTimer;
    private Transform player;
    private PlayerStats playerStats;

    void OnEnable()
    {
        angleOffset = 0f;
        damageTimer = tickInterval;
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) { player = p.transform; playerStats = p.GetComponent<PlayerStats>(); }
        EnsureVisuals();
    }

    void OnDisable()
    {
        if (bolts != null)
            foreach (var b in bolts)
                if (b != null) b.gameObject.SetActive(false);
        if (field != null) field.gameObject.SetActive(false);
    }

    /// <summary>부드러운 가장자리의 채움 원 스프라이트 생성 (눈 편한 반투명 필드용).</summary>
    static Sprite GetFieldSprite()
    {
        if (fieldSprite != null) return fieldSprite;
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        float r = size * 0.5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) - r, dy = (y + 0.5f) - r;
                float d = Mathf.Sqrt(dx * dx + dy * dy) / r;   // 0(중심)~1(가장자리)
                // 안쪽은 균일, 0.7~1 구간만 부드럽게 페이드
                float a = d >= 1f ? 0f : Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(0.7f, 1f, d));
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        // PPU = size → 스프라이트 지름 = 1 world unit (이후 radius*2로 스케일)
        fieldSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return fieldSprite;
    }

    void EnsureVisuals()
    {
        // 반투명 채움 원 필드 (윤곽선 제거 — 눈부심 완화)
        if (field == null)
        {
            var go = new GameObject("AuraField");
            go.transform.SetParent(transform, false);
            field = go.AddComponent<SpriteRenderer>();
            field.sprite = GetFieldSprite();
            var fc = color; fc.a = 0.16f;          // 낮은 알파 = 부드러운 필드
            field.color = fc;
            field.sortingOrder = -1;               // 볼트보다 뒤
            go.transform.localScale = Vector3.one * (radius * 2f);
        }
        field.gameObject.SetActive(true);

        // 내부 회전 bolts
        if (bolts == null || bolts.Length != boltCount)
        {
            if (bolts != null)
                foreach (var b in bolts) if (b != null) Destroy(b.gameObject);
            bolts = new LineRenderer[boltCount];
            for (int i = 0; i < boltCount; i++)
            {
                var go = new GameObject("Bolt" + i);
                go.transform.SetParent(transform, false);
                var lr = go.AddComponent<LineRenderer>();
                lr.useWorldSpace = false;
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startWidth = lineWidth;
                lr.endWidth = 0f;
                lr.startColor = color;
                lr.endColor = color;
                bolts[i] = lr;
            }
        }
        foreach (var b in bolts) if (b != null) b.gameObject.SetActive(true);
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        // 데미지 틱
        if (player != null && playerStats != null)
        {
            float sqDist = ((Vector2)player.position - (Vector2)transform.position).sqrMagnitude;
            if (sqDist <= radius * radius)
            {
                damageTimer -= Time.deltaTime;
                if (damageTimer <= 0f)
                {
                    playerStats.TakeDamage(damagePerTick);
                    damageTimer = tickInterval;
                }
            }
            else
            {
                damageTimer = tickInterval; // 밖에 있으면 타이머 리셋
            }
        }

        // 시각 — 회전 + 점멸
        if (bolts == null) return;
        angleOffset += rotateSpeed * Time.deltaTime;
        flickerTimer -= Time.deltaTime;
        bool flicker = false;
        if (flickerTimer <= 0f) { flicker = true; flickerTimer = flickerInterval; }

        for (int i = 0; i < bolts.Length; i++)
        {
            var lr = bolts[i];
            if (lr == null) continue;
            float a = (angleOffset + i * (360f / bolts.Length)) * Mathf.Deg2Rad;
            Vector3 outer = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * (radius * BoltReach);
            const int seg = 4;
            lr.positionCount = seg + 1;
            Vector3 perp = new Vector3(-Mathf.Sin(a), Mathf.Cos(a), 0f);
            for (int j = 0; j <= seg; j++)
            {
                float t = j / (float)seg;
                Vector3 onLine = Vector3.Lerp(Vector3.zero, outer, t);
                float noise = (j == 0 || j == seg) ? 0f : (Random.value - 0.5f) * 0.2f;
                lr.SetPosition(j, onLine + perp * noise);
            }
            if (flicker)
            {
                var c = color;
                c.a = Random.Range(0.25f, 0.6f); // 더 은은하게 (눈부심 완화)
                lr.startColor = c; lr.endColor = c;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.95f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
