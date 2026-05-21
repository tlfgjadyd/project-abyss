using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MutationManager : MonoBehaviour
{
    public static MutationManager Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("돌연변이 선택이 뜨는 레벨 (오름차순으로 설정)")]
    [SerializeField] private int[] triggerLevels = { 15, 25 };
    [SerializeField] private MutationData[] mutationPool;

    [Header("Mimicry Organ — 정기 무적")]
    [SerializeField] private float mimicryInvincibilityInterval = 60f;
    [SerializeField] private float mimicryInvincibilityDuration = 3f;

    [Header("Sensory Collapse — 스킬 사용 시 스턴")]
    [Tooltip("스킬 사용 1회당 스턴 발동 확률 (0.05 = 5%)")]
    [SerializeField] private float sensoryStunChance   = 0.05f;
    [SerializeField] private float sensoryStunDuration = 0.5f;

    private readonly HashSet<MutationID> activeMutations = new();
    private readonly Queue<MutationData[]> pendingOffers = new();
    private bool sensoryCollapseActive = false;

    /// <summary>돌연변이 2장 제시 시 발동 (MutationPanel이 구독)</summary>
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
        if (!System.Array.Exists(triggerLevels, t => t == level)) return;

        var available = mutationPool
            .Where(m => !activeMutations.Contains(m.mutationID))
            .ToList();

        // Fisher-Yates 셔플
        for (int i = available.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (available[i], available[j]) = (available[j], available[i]);
        }

        var offer = available.Take(2).ToArray();
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

    /// <summary>과부화 — 공격력 ×1.6, 현재 HP 비율 유지하며 최대 HP -30%</summary>
    void ApplyOverload(PlayerStats stats)
    {
        stats.attackPower *= 1.2f;

        float hpRatio = stats.maxHp > 0f ? stats.currentHp / stats.maxHp : 1f;
        stats.maxHp    *= 0.7f;
        stats.currentHp = Mathf.Clamp(stats.maxHp * hpRatio, 1f, stats.maxHp);
        stats.OnHpChanged?.Invoke(stats.currentHp, stats.maxHp);

        Debug.Log($"[Mutation] 과부화 — 공격력 {stats.attackPower:F1} / 최대HP {stats.maxHp:F1}");
    }

    /// <summary>과성장 촉수 — 전 스킬 범위 ×1.5, 이동속도 -30%</summary>
    void ApplyOvergrownTentacle(PlayerStats stats, GameObject player)
    {
        stats.moveSpeed *= 0.7f;

        const float scale = 1.5f;

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

    /// <summary>의태 기관 — 이속 ×2, 1분마다 3초 무적 / 공격력 ×0.5</summary>
    void ApplyMimicryOrgan(PlayerStats stats)
    {
        stats.moveSpeed   *= 2f;
        stats.attackPower *= 0.5f;
        StartCoroutine(MimicryInvincibilityRoutine(stats));

        Debug.Log($"[Mutation] 의태 기관 — 이속 {stats.moveSpeed:F1} / 공격력 {stats.attackPower:F1} / {mimicryInvincibilityInterval}s마다 {mimicryInvincibilityDuration}s 무적");
    }

    IEnumerator MimicryInvincibilityRoutine(PlayerStats stats)
    {
        while (true)
        {
            yield return new WaitForSeconds(mimicryInvincibilityInterval);
            if (stats == null) yield break;

            stats.IsInvincible = true;
            yield return new WaitForSeconds(mimicryInvincibilityDuration);
            if (stats == null) yield break;
            stats.IsInvincible = false;
        }
    }

    /// <summary>감각 붕괴 — 공격속도 ×2, 에너지 충전 ×1.5 / 스킬 사용 시 5~10% 확률 0.5초 스턴</summary>
    void ApplySensoryCollapse(PlayerStats stats)
    {
        stats.attackSpeed *= 1.5f;
        if (BioEnergyManager.Instance != null)
            BioEnergyManager.Instance.ChargeRateMultiplier *= 1.25f;

        sensoryCollapseActive = true;

        Debug.Log($"[Mutation] 감각 붕괴 — 공격속도 {stats.attackSpeed:F1} / 에너지 충전 ×1.5 / 스킬 스턴 {sensoryStunChance * 100f:F1}%");
    }

    /// <summary>독성 과부화 — 독/감전 +70% / 물리 -30%, 물리 범위 -20%</summary>
    void ApplyToxicOverload(PlayerStats stats, GameObject player)
    {
        // 효과: 독/감전 데미지 ×1.7
        var needle = player.GetComponent<PoisonNeedle>();
        if (needle != null) needle.damageMultiplier *= 1.7f;
        var electric = player.GetComponent<ElectricEngine>();
        if (electric != null) electric.damageMultiplier *= 1.7f;

        // 패널티: 물리 데미지 ×0.7, 범위 ×0.8
        var slash = player.GetComponent<Slash>();
        if (slash != null) { slash.damageMultiplier *= 0.7f; slash.range *= 0.8f; }
        var explosion = player.GetComponent<BioticExplosion>();
        if (explosion != null) { explosion.damageMultiplier *= 0.7f; explosion.range *= 0.8f; }

        // (※ "상태이상 추가 피해 +100%"는 dot/상태이상 시스템 도입 후 적용 — TODO)

        Debug.Log("[Mutation] 독성 과부화 — 독/감전 ×1.7 / 물리 ×0.7, 범위 ×0.8");
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
