using System.Collections;
using UnityEngine;

/// <summary>
/// 통합 적 상태이상(DoT) 관리. EnemyBase가 Awake에서 자동 부착하므로 프리팹 수정 불필요.
///
/// - 독(Poison): 갱신형. 인스턴스 1개만 유지. 재적용 시 지속시간 갱신 + 즉발 보너스 1회("터짐").
/// - 출혈(Bleed): 누적형. 적중마다 스택 +1(캡). 틱 데미지 = 1스택 데미지 × 스택수. 지속 공유·갱신.
///
/// 둔화(Slow)/스턴(Stun)/취약(Vulnerability)은 host(EnemyBase/BossBase)가 직접 관리.
/// 틱 데미지는 host.TakeDamage 경유(HitEffect 흰 플래시 재사용).
/// 시각화(머리 위 아이콘/오라)는 후속 단계에서 이 컴포넌트의 IsPoisoned/BleedStacks를 읽어 표시.
///
/// host는 IStatusReceiver 구현체(EnemyBase 또는 BossBase) → 일반 적·보스 공통 재사용.
/// </summary>
public class EnemyStatusEffects : MonoBehaviour
{
    public const int DefaultBleedMaxStacks = 5;

    private IStatusReceiver host;

    // ── Poison ──
    private Coroutine poisonCo;
    /// <summary>현재 중독 상태 여부 (시각화용).</summary>
    public bool IsPoisoned { get; private set; }

    // ── Bleed ──
    private Coroutine bleedCo;
    private int bleedStacks;
    private float bleedPerStackTick;   // 1스택당 1틱 데미지
    private float bleedTickInterval;
    private float bleedEndTime;
    /// <summary>현재 출혈 스택 수 (시각화용, 0이면 출혈 없음).</summary>
    public int BleedStacks => bleedStacks;

    void Awake()
    {
        host = GetComponent<IStatusReceiver>();
    }

    void OnEnable()
    {
        // 풀에서 재사용 시 초기화
        ResetAll();
        if (iconRoot != null) iconRoot.SetActive(false);
    }

    void OnDisable()
    {
        if (iconRoot != null) iconRoot.SetActive(false);
    }

    void OnDestroy()
    {
        if (iconRoot != null) Destroy(iconRoot);
    }

    public void ResetAll()
    {
        if (poisonCo != null) { StopCoroutine(poisonCo); poisonCo = null; }
        if (bleedCo != null)  { StopCoroutine(bleedCo);  bleedCo = null; }
        IsPoisoned = false;
        bleedStacks = 0;
        elecAccum = 0f;
        elecLastTick = 0f;
    }

    // ── 감전 필드 누적(전기기관 Lv4) ──
    // 필드↔필드를 연속 이동하면 누적 유지, 모든 필드 밖으로 나가면(틱 간격 초과) 리셋.
    private float elecAccum;
    private float elecLastTick;

    /// <summary>감전 필드가 1틱마다 호출. 누적 데미지가 임계치 도달 시 기절(보스는 면역).</summary>
    public void AddElectricFieldTick(float dmg, float tickInterval, float stunThreshold, float stunDuration)
    {
        if (host == null || host.IsDead) return;
        // 마지막 틱과 간격이 너무 벌어졌으면(모든 필드 밖으로 나갔다 재진입) 리셋
        if (Time.time - elecLastTick > tickInterval * 1.5f) elecAccum = 0f;
        elecLastTick = Time.time;
        elecAccum += dmg;
        if (elecAccum >= stunThreshold)
        {
            host.Stun(stunDuration);
            elecAccum = 0f;
        }
    }

    // ── 독: 갱신 + 재적용 즉발 보너스 ────────────────
    /// <param name="perTickDamage">1틱 데미지 (이미 attackPower·배율 적용된 값).</param>
    /// <param name="reapplyBonusDamage">이미 중독 중 재적용 시 즉발 1회 보너스 데미지.</param>
    public void ApplyPoison(float perTickDamage, float duration, float tickInterval, float reapplyBonusDamage)
    {
        if (host == null || host.IsDead || !isActiveAndEnabled) return;

        if (IsPoisoned)
        {
            // 재적용: 즉발 보너스 1회 + 지속 갱신(코루틴 재시작)
            if (reapplyBonusDamage > 0f) host.TakeDamage(reapplyBonusDamage);
            if (poisonCo != null) StopCoroutine(poisonCo);
        }
        poisonCo = StartCoroutine(PoisonRoutine(perTickDamage, duration, tickInterval));
    }

    IEnumerator PoisonRoutine(float perTickDamage, float duration, float tickInterval)
    {
        IsPoisoned = true;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            yield return new WaitForSeconds(tickInterval);
            if (host == null || host.IsDead || !gameObject.activeInHierarchy) break;
            host.TakeDamage(perTickDamage);
            elapsed += tickInterval;
        }
        IsPoisoned = false;
        poisonCo = null;
    }

    // ── 출혈: 누적 스택 ──────────────────────────────
    /// <param name="perStackTickDamage">스택 1당 1틱 데미지 (이미 attackPower·배율 적용된 값).</param>
    public void ApplyBleed(float perStackTickDamage, float duration, float tickInterval, int maxStacks)
    {
        if (host == null || host.IsDead || !isActiveAndEnabled) return;

        bleedPerStackTick = perStackTickDamage;
        bleedTickInterval = tickInterval;
        bleedStacks = Mathf.Min(bleedStacks + 1, Mathf.Max(1, maxStacks));
        bleedEndTime = Time.time + duration; // 적중마다 지속 갱신

        if (bleedCo == null)
            bleedCo = StartCoroutine(BleedRoutine());
    }

    IEnumerator BleedRoutine()
    {
        while (Time.time < bleedEndTime)
        {
            yield return new WaitForSeconds(bleedTickInterval);
            if (host == null || host.IsDead || !gameObject.activeInHierarchy) break;
            host.TakeDamage(bleedPerStackTick * bleedStacks);
        }
        bleedStacks = 0;
        bleedCo = null;
    }

    // ─────────────────────────────────────────────────
    //  시각화: 머리 위 상태 아이콘 (오라 없음)
    //  독립 GameObject로 추적 → 부모 scale/회전/flip 무관(보스·산갈치 회전에도 수평 유지).
    //  아이콘 Sprite는 Resources/StatusIcons/*.png를 정적 1회 로드.
    // ─────────────────────────────────────────────────
    private const int IconCount = 5;        // 0 독, 1 출혈, 2 둔화, 3 스턴, 4 취약
    private const float IconWorldSize = 0.27f;
    private const float IconGap = 0.30f;
    private const float HeadMargin = 0.28f;

    private static Sprite[] iconSprites;
    private SpriteRenderer mainSR;
    private GameObject iconRoot;
    private SpriteRenderer[] icons;
    private float headOffsetWorld;
    private bool iconsBuilt;

    static void EnsureSprites()
    {
        if (iconSprites != null) return;
        iconSprites = new Sprite[IconCount];
        iconSprites[0] = Resources.Load<Sprite>("StatusIcons/poison");
        iconSprites[1] = Resources.Load<Sprite>("StatusIcons/bleeding");
        iconSprites[2] = Resources.Load<Sprite>("StatusIcons/slow");
        iconSprites[3] = Resources.Load<Sprite>("StatusIcons/stun");
        iconSprites[4] = Resources.Load<Sprite>("StatusIcons/vuln");
    }

    void EnsureIcons()
    {
        if (iconsBuilt) return;
        iconsBuilt = true;

        EnsureSprites();

        mainSR = GetComponent<SpriteRenderer>();
        if (mainSR == null) mainSR = GetComponentInChildren<SpriteRenderer>();
        int baseSorting = mainSR != null ? mainSR.sortingOrder + 20 : 100;
        headOffsetWorld = mainSR != null ? mainSR.bounds.extents.y + HeadMargin : 0.5f;

        iconRoot = new GameObject(name + "_StatusIcons");
        icons = new SpriteRenderer[IconCount];
        for (int i = 0; i < IconCount; i++)
        {
            var go = new GameObject("icon" + i);
            go.transform.SetParent(iconRoot.transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = iconSprites != null ? iconSprites[i] : null;
            sr.sortingOrder = baseSorting;
            // 스프라이트 원본 크기와 무관하게 월드 크기를 IconWorldSize로 정규화
            if (sr.sprite != null)
            {
                float spriteWorld = Mathf.Max(sr.sprite.bounds.size.x, sr.sprite.bounds.size.y);
                float s = spriteWorld > 0.0001f ? IconWorldSize / spriteWorld : IconWorldSize;
                go.transform.localScale = Vector3.one * s;
            }
            sr.enabled = false;
            icons[i] = sr;
        }
        iconRoot.SetActive(false);
    }

    void LateUpdate()
    {
        EnsureIcons();
        if (iconRoot == null) return;

        if (host == null || host.IsDead)
        {
            if (iconRoot.activeSelf) iconRoot.SetActive(false);
            return;
        }

        bool poison = IsPoisoned;
        bool bleed  = bleedStacks > 0;
        bool slow   = host.MoveSpeedMultiplier < 0.999f;
        bool stun   = host.IsStunned;
        bool vuln   = host.TakeDamageMultiplier > 1.001f;

        bool a0 = poison, a1 = bleed, a2 = slow, a3 = stun, a4 = vuln;
        int count = (a0 ? 1 : 0) + (a1 ? 1 : 0) + (a2 ? 1 : 0) + (a3 ? 1 : 0) + (a4 ? 1 : 0);

        if (count == 0)
        {
            if (iconRoot.activeSelf) iconRoot.SetActive(false);
            return;
        }

        if (!iconRoot.activeSelf) iconRoot.SetActive(true);
        iconRoot.transform.position = transform.position + Vector3.up * headOffsetWorld;
        iconRoot.transform.rotation = Quaternion.identity;

        float startX = -((count - 1) * IconGap) * 0.5f;
        int idx = 0;
        bool[] active = { a0, a1, a2, a3, a4 };
        for (int i = 0; i < IconCount; i++)
        {
            if (active[i])
            {
                icons[i].enabled = true;
                icons[i].transform.localPosition = new Vector3(startX + idx * IconGap, 0f, 0f);
                idx++;
            }
            else
            {
                icons[i].enabled = false;
            }
        }
    }
}
