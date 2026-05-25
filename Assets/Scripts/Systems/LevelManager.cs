using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private SkillData[] skillPool;
    [SerializeField] private float baseExpPerLevel = 20f;
    [SerializeField] private SkillID[] startingSkillIDs = { SkillID.Slash };

    [Header("Skill Slot Limits (뱀서 스타일 빌드 제한)")]
    [Tooltip("동시에 보유할 수 있는 공격 스킬 최대 수. 가득 차면 카드 후보에서 신규 공격 제외")]
    [SerializeField] private int maxAttackSlots = 5;
    [Tooltip("동시에 보유할 수 있는 패시브 스킬 최대 수")]
    [SerializeField] private int maxPassiveSlots = 3;

    public int MaxAttackSlots  => maxAttackSlots;
    public int MaxPassiveSlots => maxPassiveSlots;

    [Header("Max Level Rewards")]
    [Tooltip("만렙 보상 카드 — '세포 추출' 선택 시 누적되는 세포 수")]
    [SerializeField] private int cellsPerSelection = 2;
    [Tooltip("만렙 보상 카드 — '세포 강화' 선택 시 회복되는 최대 HP 비율 (0.30 = 30%)")]
    [Range(0f, 1f)]
    [SerializeField] private float hpHealRatioPerSelection = 0.30f;

    public int CellsPerSelection      => cellsPerSelection;
    public float HpHealRatioPerSelection => hpHealRatioPerSelection;

    public int CurrentLevel { get; private set; } = 1;
    public float CurrentExp { get; private set; }
    public float ExpToNextLevel { get; private set; }

    /// <summary>누적 세포 (메타 재화). 6주차 Day 40: 게임오버/4스 엔딩 시 MetaProgressData에 누적.</summary>
    public int CurrentCells { get; private set; }

    /// <summary>인게임 세포 누적. 보스 처치/스테이지 클리어 등 만렙 카드 외 경로용.</summary>
    public void AddCells(int amount, string source = "")
    {
        if (amount <= 0) return;
        CurrentCells += amount;
        OnCellsChanged?.Invoke(CurrentCells);
        Debug.Log($"[LevelManager] 세포 +{amount} ({source}) — 누적 {CurrentCells}");
    }

    /// <summary>인게임 세포 카운터를 0으로 비운다. 메타로 누적 후 중복 누적 방지용.</summary>
    public void ConsumeAllCells()
    {
        if (CurrentCells == 0) return;
        CurrentCells = 0;
        OnCellsChanged?.Invoke(0);
    }

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
            AudioManager.Instance?.PlaySFX(SfxId.LevelUp);
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

    /// <summary>
    /// 더 이상 제시 가능한 카드가 없는지 확인. 슬롯 제한 도입 후:
    /// - 공격 슬롯 가득 + 보유 공격 모두 만렙
    /// - 패시브 슬롯 가득 + 보유 패시브 모두 만렙
    /// 두 조건 모두 충족 시 만렙 보상 카드로 전환.
    /// </summary>
    public bool IsAllSkillsMaxed()
    {
        if (skillPool == null || skillPool.Length == 0) return false;

        int ownedAttack = 0, ownedPassive = 0;
        bool anyAttackUpgradable = false, anyPassiveUpgradable = false;
        foreach (var s in skillPool)
        {
            if (s == null) continue;
            int lv = GetSkillLevel(s);
            bool isOwned = lv > 0;
            if (s.skillType == SkillType.Attack)
            {
                if (isOwned) ownedAttack++;
                if (lv < s.maxLevel && isOwned) anyAttackUpgradable = true;
            }
            else
            {
                if (isOwned) ownedPassive++;
                if (lv < s.maxLevel && isOwned) anyPassiveUpgradable = true;
            }
        }

        bool attackDone  = (ownedAttack  >= maxAttackSlots)  && !anyAttackUpgradable;
        bool passiveDone = (ownedPassive >= maxPassiveSlots) && !anyPassiveUpgradable;
        return attackDone && passiveDone;
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
        // 현재 보유 공격/패시브 스킬 수 (Lv1+ 획득한 것)
        int ownedAttack  = 0;
        int ownedPassive = 0;
        foreach (var kv in skillLevels)
        {
            if (kv.Key == null || kv.Value <= 0) continue;
            if (kv.Key.skillType == SkillType.Attack)  ownedAttack++;
            else                                       ownedPassive++;
        }
        bool attackSlotsFull  = ownedAttack  >= maxAttackSlots;
        bool passiveSlotsFull = ownedPassive >= maxPassiveSlots;

        var available = new System.Collections.Generic.List<SkillData>();
        foreach (var s in skillPool)
        {
            if (s == null) continue;
            int lv = GetSkillLevel(s);
            if (lv >= s.maxLevel) continue;

            // 슬롯 가득 + 신규 스킬(Lv0)이면 후보에서 제외. 기존 강화는 OK.
            bool isNew = lv == 0;
            if (isNew)
            {
                if (s.skillType == SkillType.Attack  && attackSlotsFull)  continue;
                if (s.skillType == SkillType.Passive && passiveSlotsFull) continue;
            }
            available.Add(s);
        }

        // Fisher-Yates 셔플
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

    // ── PlayerProgressData 연동 (씬 전환 시 상태 복원) ───

    /// <summary>현재 스킬 레벨 Dictionary의 복사본 반환 (Capture용).</summary>
    public Dictionary<SkillData, int> GetSkillLevelsCopy()
    {
        return new Dictionary<SkillData, int>(skillLevels);
    }

    /// <summary>
    /// 새 씬에서 LevelManager의 상태를 한 번에 복원.
    /// 각 스킬에 대해 Lv1~targetLevel까지 OnSkillSelected를 순차 발행하여
    /// SkillEffectApplier의 누적 delta 효과를 재적용한다. (maxHp, attackPower, projectile count 등)
    /// </summary>
    public void SetState(int level, float exp, float expToNext, Dictionary<SkillData, int> levels, int cells)
    {
        CurrentLevel    = level;
        CurrentExp      = exp;
        ExpToNextLevel  = expToNext > 0f ? expToNext : baseExpPerLevel * Mathf.Max(1, level);
        CurrentCells    = cells;

        skillLevels.Clear();
        if (levels != null)
        {
            foreach (var kv in levels)
            {
                if (kv.Key == null) continue;
                skillLevels[kv.Key] = kv.Value;

                // 스킬 효과 재적용: Lv1부터 targetLevel까지 누적 발행
                // (SkillEffectApplier가 += delta 방식이라 한 번씩 호출해야 함)
                for (int lv = 1; lv <= kv.Value; lv++)
                    OnSkillSelected?.Invoke(kv.Key, lv);
            }
        }

        OnLevelChanged?.Invoke(CurrentLevel);
        OnExpChanged?.Invoke(CurrentExp, ExpToNextLevel);
        OnCellsChanged?.Invoke(CurrentCells);
    }
}
