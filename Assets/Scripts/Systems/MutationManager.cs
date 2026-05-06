using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MutationManager : MonoBehaviour
{
    public static MutationManager Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("돌연변이 선택이 뜨는 레벨 (오름차순으로 설정)")]
    [SerializeField] private int[] triggerLevels = { 5, 10 };
    [SerializeField] private MutationData[] mutationPool;

    private readonly HashSet<MutationID> activeMutations = new();
    private MutationData[] pendingOffer;
    private bool hasPendingMutation = false;

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
    }

    void OnDestroy()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.OnLevelChanged -= OnLevelChanged;
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= OnStateChanged;
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

        pendingOffer = available.Take(2).ToArray();
        if (pendingOffer.Length == 0) return;

        hasPendingMutation = true;
        // 레벨업 카드 선택이 완료되어 Playing 상태로 돌아올 때 발동
    }

    void OnStateChanged(GameManager.GameState state)
    {
        if (state == GameManager.GameState.Playing && hasPendingMutation)
        {
            hasPendingMutation = false;
            OnMutationOffered?.Invoke(pendingOffer);
            GameManager.Instance.TriggerMutation();
        }
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

            case MutationID.OvgrownTentacle:
                ApplyOvgrownTentacle(stats, player);
                break;

            case MutationID.MimicryOrgan:
                ApplyMimicryOrgan(stats);
                break;
        }
    }

    // ── 개별 효과 ────────────────────────────────

    /// <summary>과부화 — 공격력 ×1.6, 현재 HP 비율 유지하며 최대 HP -30%</summary>
    void ApplyOverload(PlayerStats stats)
    {
        stats.attackPower *= 1.6f;

        float hpRatio = stats.maxHp > 0f ? stats.currentHp / stats.maxHp : 1f;
        stats.maxHp    *= 0.7f;
        stats.currentHp = Mathf.Clamp(stats.maxHp * hpRatio, 1f, stats.maxHp);
        stats.OnHpChanged?.Invoke(stats.currentHp, stats.maxHp);

        Debug.Log($"[Mutation] 과부화 — 공격력 {stats.attackPower:F1} / 최대HP {stats.maxHp:F1}");
    }

    /// <summary>과성장 촉수 — 전 스킬 범위 ×1.5, 이동속도 -30%</summary>
    void ApplyOvgrownTentacle(PlayerStats stats, GameObject player)
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

    /// <summary>의태 기관 — 카피 스킬 에너지 소모 0, HP 회복 완전 차단</summary>
    void ApplyMimicryOrgan(PlayerStats stats)
    {
        stats.healingBlocked = true;
        if (CopySkillManager.Instance != null)
            CopySkillManager.Instance.FreeCopySkills = true;

        Debug.Log("[Mutation] 의태 기관 — 회복 차단 / 카피 스킬 무료");
    }

    // ── 조회 ─────────────────────────────────────

    public bool HasMutation(MutationID id) => activeMutations.Contains(id);
}
