using UnityEngine;

// Player 오브젝트에 부착. LevelManager의 OnSkillSelected 이벤트를 수신해 실제 스탯에 반영.
public class SkillEffectApplier : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool testMode = false; // 체크 시 모든 스킬 즉시 활성화

    private PlayerStats stats;
    private Slash slash;
    private PoisonNeedle poisonNeedle;
    private BioticExplosion bioticExplosion;
    private ElectricEngine electricEngine;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
        slash = GetComponent<Slash>();

        poisonNeedle = GetComponent<PoisonNeedle>();
        bioticExplosion = GetComponent<BioticExplosion>();
        electricEngine = GetComponent<ElectricEngine>();

        if (!testMode)
        {
            // 선택형 스킬은 시작 시 비활성화 — 레벨업에서 처음 선택할 때 활성화됨
            if (poisonNeedle != null)    poisonNeedle.enabled    = false;
            if (bioticExplosion != null) bioticExplosion.enabled = false;
            if (electricEngine != null)  electricEngine.enabled  = false;
        }
    }

    void Start()
    {
        LevelManager.Instance.OnSkillSelected += Apply;
    }

    void OnDestroy()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.OnSkillSelected -= Apply;
    }

    void Apply(SkillData skill, int newLevel)
    {
        if (newLevel < 1 || newLevel > skill.levelStats.Length) return;

        SkillLevelStats s = skill.levelStats[newLevel - 1];

        switch (skill.skillID)
        {
            case SkillID.Slash:          ApplySlash(s);          break;
            case SkillID.CellRegen:      ApplyCellRegen(s);      break;
            case SkillID.AccelMutation:  ApplyAccelMutation(s);  break;
            case SkillID.NeuralAccel:    ApplyNeuralAccel(s);    break;
            case SkillID.PoisonNeedle:    ApplyPoisonNeedle(s);    break;
            case SkillID.BioticExplosion: ApplyBioticExplosion(s); break;
            case SkillID.ElectricEngine:  ApplyElectricEngine(s);  break;
        }
    }

    // ── 공격 스킬 ────────────────────────────────

    void ApplySlash(SkillLevelStats s)
    {
        stats.attackPower += s.attackPowerDelta;
        stats.attackSpeed += s.attackSpeedDelta;

        if (slash != null)
        {
            slash.range += s.rangeDelta;
            if (s.knockbackEnabled)
                slash.knockbackEnabled = true;
        }
    }

    // ── 패시브 스킬 ──────────────────────────────

    void ApplyCellRegen(SkillLevelStats s)
    {
        stats.maxHp += s.maxHpDelta;
        stats.currentHp = Mathf.Min(stats.currentHp + s.maxHpDelta, stats.maxHp);
        stats.hpRegenPerSecond += s.hpRegenDelta;
        stats.OnHpChanged?.Invoke(stats.currentHp, stats.maxHp);
    }

    void ApplyAccelMutation(SkillLevelStats s)
    {
        stats.moveSpeed += s.moveSpeedDelta;
    }

    void ApplyNeuralAccel(SkillLevelStats s)
    {
        stats.attackSpeed += s.attackSpeedDelta;
    }

    void ApplyPoisonNeedle(SkillLevelStats s)
    {
        stats.attackPower += s.attackPowerDelta;
        if (poisonNeedle == null) return;

        if (!poisonNeedle.enabled) poisonNeedle.enabled = true;
        poisonNeedle.cooldown = Mathf.Max(0.2f, poisonNeedle.cooldown - s.cooldownDelta);
        poisonNeedle.projectileCount += s.extraProjectiles;
        if (s.piercingEnabled)
            poisonNeedle.isPiercing = true;
    }

    void ApplyBioticExplosion(SkillLevelStats s)
    {
        stats.attackPower += s.attackPowerDelta;
        if (bioticExplosion == null) return;

        if (!bioticExplosion.enabled) bioticExplosion.enabled = true;
        bioticExplosion.range += s.rangeDelta;
        bioticExplosion.stunDuration += s.stunDurationDelta;
    }

    void ApplyElectricEngine(SkillLevelStats s)
    {
        stats.attackPower += s.attackPowerDelta;
        stats.attackSpeed += s.attackSpeedDelta;
        if (electricEngine == null) return;

        if (!electricEngine.enabled) electricEngine.enabled = true;
        electricEngine.chainRadius += s.rangeDelta;
    }
}
