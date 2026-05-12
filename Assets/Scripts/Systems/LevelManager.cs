using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private SkillData[] skillPool;
    [SerializeField] private float baseExpPerLevel = 20f;
    [SerializeField] private SkillID[] startingSkillIDs = { SkillID.Slash };

    [Header("Max Level Rewards")]
    [Tooltip("만렙 보상 카드 — '세포 추출' 선택 시 누적되는 세포 수")]
    [SerializeField] private int cellsPerSelection = 5;
    [Tooltip("만렙 보상 카드 — '세포 강화' 선택 시 회복되는 최대 HP 비율 (0.30 = 30%)")]
    [Range(0f, 1f)]
    [SerializeField] private float hpHealRatioPerSelection = 0.30f;

    public int CellsPerSelection      => cellsPerSelection;
    public float HpHealRatioPerSelection => hpHealRatioPerSelection;

    public int CurrentLevel { get; private set; } = 1;
    public float CurrentExp { get; private set; }
    public float ExpToNextLevel { get; private set; }

    /// <summary>누적 세포 (메타 재화). 6주차에 메타 업그레이드 화면에서 사용 예정.</summary>
    public int CurrentCells { get; private set; }

    private readonly Dictionary<SkillData, int> skillLevels = new();
    private SkillData[] currentOffer;

    /// <summary>한 번에 여러 레벨업이 발생할 때 카드를 순차 표시하기 위한 카운터.</summary>
    private int pendingLevelUps = 0;

    public System.Action<SkillData[]> OnLevelUpOffered;
    public System.Action<SkillData, int> OnSkillSelected;
    public System.Action<int> OnLevelChanged;
    public System.Action<float, float> OnExpChanged;
    public System.Action<int> OnCellsChanged;
    /// <summary>(cellsGained, hpHealed) — 만렙 보상 카드 선택 후 결과 발행. HUD 피드백용.</summary>
    public System.Action<int, float> OnMaxLevelReward;
    /// <summary>만렙 보상 카드를 표시해야 할 때 발행 (MaxLevelRewardPanel이 구독).</summary>
    public System.Action OnMaxLevelRewardOffered;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        ExpToNextLevel = baseExpPerLevel;
        InitializeStartingSkills();
    }

    void Start()
    {
        // Playing 상태로 복귀할 때마다 펜딩 레벨업 1건 처리
        GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }

    void OnGameStateChanged(GameManager.GameState state)
    {
        // 매개변수 대신 현재 상태 기준 (중첩 ChangeState 안전)
        if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
            TryShowPendingLevelUp();
    }

    void InitializeStartingSkills()
    {
        foreach (SkillID skillID in startingSkillIDs)
        {
            SkillData skill = skillPool.FirstOrDefault(s => s != null && s.skillID == skillID);
            if (skill != null)
                skillLevels[skill] = 1;
        }
    }

    public void AddExp(float amount)
    {
        float multiplier = PressureSystem.Instance != null ? PressureSystem.Instance.ExpMultiplier : 1f;
        CurrentExp += amount * multiplier;

        // 한 번에 들어온 EXP로 여러 레벨이 오를 수 있음 — 누적 카운터로 보관
        while (CurrentExp >= ExpToNextLevel)
        {
            CurrentExp -= ExpToNextLevel;
            CurrentLevel++;
            ExpToNextLevel = baseExpPerLevel * CurrentLevel;
            OnLevelChanged?.Invoke(CurrentLevel);
            pendingLevelUps++;
        }

        OnExpChanged?.Invoke(CurrentExp, ExpToNextLevel);

        // 펜딩 레벨업 1건 표시 시도 (나머지는 카드 선택 후 OnGameStateChanged에서 순차 처리)
        TryShowPendingLevelUp();
    }

    /// <summary>펜딩 레벨업이 있고 현재 Playing 상태면 카드 1세트를 표시.</summary>
    void TryShowPendingLevelUp()
    {
        if (pendingLevelUps <= 0) return;
        if (GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        pendingLevelUps--;

        // 모든 일반 스킬 만렙이면 만렙 보상 카드 표시
        if (IsAllSkillsMaxed())
        {
            OnMaxLevelRewardOffered?.Invoke();
            GameManager.Instance.TriggerLevelUp();
            return;
        }

        // 일반 스킬 카드 표시
        currentOffer = PickSkills(3);
        if (currentOffer.Length == 0)
        {
            // skillPool 자체가 비어있는 비정상 케이스 — 다음 펜딩 처리 시도
            TryShowPendingLevelUp();
            return;
        }

        OnLevelUpOffered?.Invoke(currentOffer);
        GameManager.Instance.TriggerLevelUp();
    }

    /// <summary>모든 일반 스킬이 maxLevel에 도달했는지 확인.</summary>
    public bool IsAllSkillsMaxed()
    {
        if (skillPool == null || skillPool.Length == 0) return false;

        foreach (var s in skillPool)
        {
            if (s == null) continue;
            if (GetSkillLevel(s) < s.maxLevel) return false;
        }
        return true;
    }

    /// <summary>만렙 보상 카드 선택. MaxLevelRewardPanel에서 호출.</summary>
    public void SelectMaxLevelReward(MaxLevelRewardType type)
    {
        int cellsGained = 0;
        float hpHealed = 0f;

        if (type == MaxLevelRewardType.Heal)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var stats = player.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    float before = stats.currentHp;
                    stats.Heal(stats.maxHp * hpHealRatioPerSelection);
                    hpHealed = stats.currentHp - before;
                }
            }
        }
        else // Cells
        {
            cellsGained = cellsPerSelection;
            CurrentCells += cellsGained;
            OnCellsChanged?.Invoke(CurrentCells);
        }

        OnMaxLevelReward?.Invoke(cellsGained, hpHealed);
        Debug.Log($"[LevelManager] Max reward '{type}' — 세포 +{cellsGained} / HP +{hpHealed:F1} (누적 세포 {CurrentCells})");

        GameManager.Instance.ResumeGame();
    }

    SkillData[] PickSkills(int count)
    {
        var available = skillPool
            .Where(s => GetSkillLevel(s) < s.maxLevel)
            .ToList();

        for (int i = available.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (available[i], available[j]) = (available[j], available[i]);
        }

        return available.Take(count).ToArray();
    }

    public void SelectSkill(SkillData skill)
    {
        int newLevel = GetSkillLevel(skill) + 1;
        skillLevels[skill] = newLevel;
        OnSkillSelected?.Invoke(skill, newLevel);
        GameManager.Instance.ResumeGame();
    }

    public int GetSkillLevel(SkillData skill)
    {
        return skillLevels.TryGetValue(skill, out int level) ? level : 0;
    }
}
