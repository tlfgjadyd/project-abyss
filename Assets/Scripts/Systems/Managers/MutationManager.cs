using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MutationManager : MonoBehaviour
{
    public static MutationManager Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("돌연변이 선택이 뜨는 레벨 (오름차순으로 설정)")]
    [SerializeField] private int[] triggerLevels = { 14, 25 };
    [SerializeField] private MutationData[] mutationPool;

    [Header("Mimicry Organ — 정기 무적 + 버스트")]
    [SerializeField] private float mimicryInvincibilityInterval = 15f;  // Day58: 40→15 (자주 발동)
    [SerializeField] private float mimicryInvincibilityDuration = 3f;
    [Tooltip("무적 중 공격력 배율 (페널티 ×0.5와 합쳐 ×2 = 평소의 4배 버스트)")]
    [SerializeField] private float mimicryBurstAttackMultiplier = 4f;

    [Header("Sensory Collapse — 스킬 사용 시 스턴")]
    [Tooltip("스킬 사용 1회당 스턴 발동 확률 (0.05 = 5%)")]
    [SerializeField] private float sensoryStunChance   = 0.05f;
    [SerializeField] private float sensoryStunDuration = 0.5f;

    private readonly HashSet<MutationID> activeMutations = new();
    private readonly Queue<MutationData[]> pendingOffers = new();
    /// <summary>이미 발동한 trigger 레벨 — 씬 전환 시 PlayerProgressData.Restore가 OnLevelChanged를 재발행해도 중복 트리거 방지</summary>
    private readonly HashSet<int> firedTriggerLevels = new();
    private bool sensoryCollapseActive = false;

    /// <summary>씬 전환 복원 중 true — LevelManager.SetState가 OnLevelChanged를 재발행해도 트리거 억제.
    /// (이미 지난 트리거가 새 씬의 빈 firedTriggerLevels/activeMutations 때문에 재발동하는 버그 방지)</summary>
    public bool SuppressTriggers { get; set; }

    /// <summary>돌연변이 최대 3장 제시 시 발동 (MutationPanel이 구독)</summary>
    public System.Action<MutationData[]> OnMutationOffered;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        LevelManager.Instance.OnLevelChanged    += OnLevelChanged;
        GameManager.Instance.OnGameStateChanged += OnStateChanged;
        PlayerSkillEvents.OnSkillUsed           += OnPlayerSkillUsed;
    }

    void OnDestroy()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.OnLevelChanged -= OnLevelChanged;
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= OnStateChanged;
        PlayerSkillEvents.OnSkillUsed -= OnPlayerSkillUsed;
    }

    // ── 트리거 체크 ──────────────────────────────

    void OnLevelChanged(int level)
    {
        // 씬 전환 복원 중에는 트리거 억제 (이미 지난 레벨이 SetState로 재발행되므로)
        if (SuppressTriggers) return;
        if (!System.Array.Exists(triggerLevels, t => t == level)) return;
        // 같은 trigger 레벨이 두 번 발행되어도 1회만 처리 (씬 전환 Restore 중복 방지)
        if (!firedTriggerLevels.Add(level)) return;

        var available = mutationPool
            .Where(m => !activeMutations.Contains(m.mutationID))
            .ToList();

        // Fisher-Yates 셔플
        for (int i = available.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (available[i], available[j]) = (available[j], available[i]);
        }

        var offer = available.Take(3).ToArray();
        if (offer.Length == 0) return;

        // 큐에 보관 — Playing 상태로 돌아올 때마다 1건씩 발동
        pendingOffers.Enqueue(offer);
    }

    void OnStateChanged(GameManager.GameState state)
    {
        // 매개변수 대신 현재 상태 기준 (중첩 ChangeState 안전)
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;
        if (pendingOffers.Count == 0) return;

        var offer = pendingOffers.Dequeue();
        OnMutationOffered?.Invoke(offer);
        GameManager.Instance.TriggerMutation();
    }

    // ── 선택 & 적용 ──────────────────────────────

    public void SelectMutation(MutationData mutation)
    {
        activeMutations.Add(mutation.mutationID);
        ApplyMutation(mutation);
        GameManager.Instance.ResumeGame();
    }

    void ApplyMutation(MutationData mutation)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        var stats = player.GetComponent<PlayerStats>();

        switch (mutation.mutationID)
        {
            case MutationID.Overload:
                ApplyOverload(stats);
                break;

            case MutationID.OvergrownTentacle:
                ApplyOvergrownTentacle(stats, player);
                break;

            case MutationID.MimicryOrgan:
                ApplyMimicryOrgan(stats);
                break;

            case MutationID.SensoryCollapse:
                ApplySensoryCollapse(stats);
                break;

            case MutationID.ToxicOverload:
                ApplyToxicOverload(stats, player);
                break;
        }
    }

    // ── 개별 효과 ────────────────────────────────

    /// <summary>과부화 — 공격력 ×1.6, 현재 HP 비율 유지하며 최대 HP -45% (하이리스크 하이리턴)</summary>
    void ApplyOverload(PlayerStats stats)
    {
        stats.attackPower *= 1.6f;

        float hpRatio = stats.maxHp > 0f ? stats.currentHp / stats.maxHp : 1f;
        stats.maxHp    *= 0.55f;
        stats.currentHp = Mathf.Clamp(stats.maxHp * hpRatio, 1f, stats.maxHp);
        stats.OnHpChanged?.Invoke(stats.currentHp, stats.maxHp);

        Debug.Log($"[Mutation] 과부화 — 공격력 {stats.attackPower:F1} / 최대HP {stats.maxHp:F1}");
    }

    /// <summary>과성장 촉수 — 전 스킬 범위 ×1.6, 이동속도 -15% (페널티 완화 + 사거리 체감 ↑)</summary>
    void ApplyOvergrownTentacle(PlayerStats stats, GameObject player)
    {
        stats.moveSpeed *= 0.85f;

        const float scale = 1.6f;

        player.GetComponent<PlayerCombat>()?.ScaleDetectionRadius(scale);

        var slash = player.GetComponent<Slash>();
        if (slash != null) slash.range *= scale;

        var needle = player.GetComponent<PoisonNeedle>();
        if (needle != null) needle.detectionRadius *= scale;

        var explosion = player.GetComponent<BioticExplosion>();
        if (explosion != null) explosion.range *= scale;

        var electric = player.GetComponent<ElectricEngine>();
        if (electric != null)
        {
            electric.detectionRadius *= scale;
            electric.chainRadius     *= scale;
        }

        Debug.Log($"[Mutation] 과성장 촉수 — 이동속도 {stats.moveSpeed:F1} / 범위 ×{scale}");
    }

    /// <summary>의태 기관 — 15초마다 3초 무적 + 무적 중 공격력 버스트(×4) / 평소 공격력 ×0.5 (이속 보너스 없음)</summary>
    void ApplyMimicryOrgan(PlayerStats stats)
    {
        stats.attackPower *= 0.5f;   // 평소 페널티 (무적 중에는 ×4로 상쇄+버스트 = 평소의 4배 = 원래의 2배)
        StartCoroutine(MimicryInvincibilityRoutine(stats));

        Debug.Log($"[Mutation] 의태 기관 — 공격력 {stats.attackPower:F1} (평소 ×0.5) / {mimicryInvincibilityInterval}s마다 {mimicryInvincibilityDuration}s 무적+버스트 ×{mimicryBurstAttackMultiplier}");
    }

    IEnumerator MimicryInvincibilityRoutine(PlayerStats stats)
    {
        // 무적 시각화 — 의태(주변에 녹아듦) 컨셉으로 무적 동안 플레이어 반투명.
        var sr = stats.GetComponentInChildren<SpriteRenderer>();
        float baseAlpha = sr != null ? sr.color.a : 1f;
        const float invisAlpha = 0.45f;
        const float fade = 0.15f;

        while (true)
        {
            yield return new WaitForSeconds(mimicryInvincibilityInterval);
            if (stats == null) yield break;

            stats.IsInvincible = true;
            stats.attackPower *= mimicryBurstAttackMultiplier;   // 무적 중 공격력 버스트
            yield return FadeSpriteAlpha(sr, invisAlpha, fade);

            // 페이드 인/아웃 시간을 제외한 나머지 무적 유지
            yield return new WaitForSeconds(Mathf.Max(0f, mimicryInvincibilityDuration - 2f * fade));
            if (stats == null) yield break;

            stats.IsInvincible = false;
            stats.attackPower /= mimicryBurstAttackMultiplier;   // 버스트 해제
            yield return FadeSpriteAlpha(sr, baseAlpha, fade);
        }
    }

    IEnumerator FadeSpriteAlpha(SpriteRenderer sr, float target, float dur)
    {
        if (sr == null) yield break;
        Color c = sr.color;
        float start = c.a;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(start, target, t / dur);
            sr.color = c;
            yield return null;
        }
        c.a = target;
        sr.color = c;
    }

    /// <summary>감각 붕괴 — 공격속도 ×1.5, 에너지 충전 ×1.25 / 스킬 사용 시 5% 확률 0.5초 스턴</summary>
    void ApplySensoryCollapse(PlayerStats stats)
    {
        stats.attackSpeed *= 1.5f;
        if (BioEnergyManager.Instance != null)
            BioEnergyManager.Instance.ChargeRateMultiplier *= 1.25f;

        sensoryCollapseActive = true;

        Debug.Log($"[Mutation] 감각 붕괴 — 공격속도 {stats.attackSpeed:F1} / 에너지 충전 ×1.25 / 스킬 스턴 {sensoryStunChance * 100f:F1}%");
    }

    /// <summary>독성 과부화 — 독/감전/출혈 DoT ×1.7 / 물리 ×0.7, 물리 범위 ×0.8</summary>
    void ApplyToxicOverload(PlayerStats stats, GameObject player)
    {
        // 효과: 독/감전 데미지 ×1.7
        // damageMultiplier가 직접 데미지뿐 아니라 독 DoT(틱·재적용 보너스)에도 그대로 반영됨
        // → "상태이상 추가 피해" TODO는 PoisonNeedle이 DoT 데미지 산정에 damageMultiplier를 쓰면서 자연 해소됨.
        var needle = player.GetComponent<PoisonNeedle>();
        if (needle != null) needle.damageMultiplier *= 1.7f;
        var electric = player.GetComponent<ElectricEngine>();
        if (electric != null) electric.damageMultiplier *= 1.7f;

        // 출혈 DoT도 강화 (초기 설계: DoT 강화 / 직접타격 약화)
        var pstats = player.GetComponent<PlayerStats>();
        if (pstats != null) pstats.bleedDamageMultiplier *= 1.7f;

        // 패널티: 물리 데미지 ×0.7, 범위 ×0.8
        var slash = player.GetComponent<Slash>();
        if (slash != null) { slash.damageMultiplier *= 0.7f; slash.range *= 0.8f; }
        var explosion = player.GetComponent<BioticExplosion>();
        if (explosion != null) { explosion.damageMultiplier *= 0.7f; explosion.range *= 0.8f; }

        Debug.Log("[Mutation] 독성 과부화 — 독/감전/출혈 DoT ×1.7 / 물리 ×0.7, 범위 ×0.8");
    }

    // ── 감각 붕괴: 스킬 사용 hook ─────────────────

    void OnPlayerSkillUsed()
    {
        if (!sensoryCollapseActive) return;
        if (Random.value >= sensoryStunChance) return;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        var stats = player.GetComponent<PlayerStats>();
        if (stats == null || stats.IsStunned) return;

        StartCoroutine(StunRoutine(stats, sensoryStunDuration));
    }

    IEnumerator StunRoutine(PlayerStats stats, float duration)
    {
        stats.IsStunned = true;
        yield return new WaitForSeconds(duration);
        if (stats != null) stats.IsStunned = false;
    }

    // ── 조회 ─────────────────────────────────────

    public bool HasMutation(MutationID id) => activeMutations.Contains(id);

    /// <summary>돌연변이 풀 (UI 글리프 prewarm용).</summary>
    public MutationData[] Pool => mutationPool;

    // ── PlayerProgressData 연동 (씬 전환 시 상태 복원) ───

    /// <summary>현재 보유 돌연변이 ID 목록 반환 (Capture용).</summary>
    public List<MutationID> GetOwnedIDs() => new List<MutationID>(activeMutations);

    /// <summary>
    /// 새 씬에서 돌연변이 상태 복원.
    /// 각 ID마다 ApplyMutation을 호출하여 stats에 곱셈 효과를 재적용한다.
    /// (Awake에서 stats가 base 값으로 초기화된 후 호출되는 것이 전제)
    /// </summary>
    public void RestoreOwnedIDs(List<MutationID> ids)
    {
        if (ids == null) return;

        foreach (var id in ids)
        {
            if (activeMutations.Contains(id)) continue;

            var data = System.Array.Find(mutationPool, m => m != null && m.mutationID == id);
            if (data == null) continue;

            activeMutations.Add(id);
            ApplyMutation(data);
        }
    }
}
