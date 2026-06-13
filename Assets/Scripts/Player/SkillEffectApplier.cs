using System.Collections;
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
    // Day 45 신규
    private SonicPulse sonicPulse;
    private SpikeBurst spikeBurst;
    private DrainTentacle drainTentacle;
    private MagneticInduction magnetic;
    private GlowOrgan glowOrgan;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
        slash = GetComponent<Slash>();

        poisonNeedle = GetComponent<PoisonNeedle>();
        bioticExplosion = GetComponent<BioticExplosion>();
        electricEngine = GetComponent<ElectricEngine>();

        sonicPulse = GetComponent<SonicPulse>();
        spikeBurst = GetComponent<SpikeBurst>();
        drainTentacle = GetComponent<DrainTentacle>();
        magnetic = GetComponent<MagneticInduction>();
        glowOrgan = GetComponent<GlowOrgan>();

        if (!testMode)
        {
            // 선택형 스킬은 시작 시 비활성화 — 레벨업에서 처음 선택할 때 활성화됨
            if (poisonNeedle != null)    poisonNeedle.enabled    = false;
            if (bioticExplosion != null) bioticExplosion.enabled = false;
            if (electricEngine != null)  electricEngine.enabled  = false;
            if (sonicPulse != null)      sonicPulse.enabled      = false;
            if (spikeBurst != null)      spikeBurst.enabled      = false;
            if (drainTentacle != null)   drainTentacle.enabled   = false;
            if (magnetic != null)        magnetic.enabled        = false;
            if (glowOrgan != null)       glowOrgan.enabled       = false;
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
        if (accelBurstEnabled)
            EnemyBase.OnAnyEnemyKilled -= OnEnemyKilledAccel;
    }

    void Apply(SkillData skill, int newLevel)
    {
        if (newLevel < 1 || newLevel > skill.levelStats.Length) return;

        SkillLevelStats s = skill.levelStats[newLevel - 1];

        switch (skill.skillID)
        {
            case SkillID.Slash:          ApplySlash(s);          break;
            case SkillID.CellRegen:      ApplyCellRegen(s);      break;
            case SkillID.AccelMutation:  ApplyAccelMutation(s, newLevel);  break;
            case SkillID.NeuralAccel:    ApplyNeuralAccel(s);    break;
            case SkillID.PoisonNeedle:    ApplyPoisonNeedle(s);    break;
            case SkillID.BioticExplosion: ApplyBioticExplosion(s); break;
            case SkillID.ElectricEngine:  ApplyElectricEngine(s, newLevel);  break;
            // Day 45 신규
            case SkillID.SonicPulse:        ApplySonicPulse(s);     break;
            case SkillID.SpikeBurst:        ApplySpikeBurst(s);     break;
            case SkillID.DrainTentacle:     ApplyDrainTentacle(s, newLevel);  break;
            case SkillID.MagneticInduction: ApplyMagnetic(s, newLevel);       break;
            case SkillID.GlowOrgan:         ApplyGlowOrgan(s);      break;
        }
    }

    // ── Day 45 신규 스킬 ─────────────────────────

    void ApplySonicPulse(SkillLevelStats s)
    {
        stats.attackPower += s.attackPowerDelta;
        if (sonicPulse == null) return;
        if (!sonicPulse.enabled) sonicPulse.enabled = true;
        sonicPulse.range += s.rangeDelta;
        sonicPulse.cooldown = Mathf.Max(0.5f, sonicPulse.cooldown - s.cooldownDelta);
        // knockbackEnabled 플래그가 SkillData에 켜져 있으면 활성 (Lv4 한정 보상)
        if (s.knockbackEnabled) sonicPulse.knockbackEnabled = true;
    }

    void ApplySpikeBurst(SkillLevelStats s)
    {
        stats.attackPower += s.attackPowerDelta;
        if (spikeBurst == null) return;
        if (!spikeBurst.enabled) spikeBurst.enabled = true;
        spikeBurst.range += s.rangeDelta;
        spikeBurst.cooldown = Mathf.Max(0.5f, spikeBurst.cooldown - s.cooldownDelta);
        spikeBurst.spikeCount += s.extraProjectiles;
        // piercingEnabled 플래그를 Lv4 출혈 부여 트리거로 재사용 (SonicPulse knockbackEnabled 패턴)
        if (s.piercingEnabled) spikeBurst.bleedEnabled = true;
    }

    void ApplyDrainTentacle(SkillLevelStats s, int level)
    {
        stats.attackPower += s.attackPowerDelta;
        if (drainTentacle == null) return;
        if (!drainTentacle.enabled) drainTentacle.enabled = true;
        drainTentacle.range += s.rangeDelta;
        drainTentacle.cooldown = Mathf.Max(0.3f, drainTentacle.cooldown - s.cooldownDelta);
        // 흡혈량 레벨 비례 (Lv1 0.1 ~ Lv4 0.4), Lv4 출혈 부여
        drainTentacle.lifestealFlat = 0.1f * level;
        if (level >= 4) drainTentacle.bleedEnabled = true;
    }

    void ApplyMagnetic(SkillLevelStats s, int level)
    {
        if (magnetic == null) return;
        if (!magnetic.enabled) magnetic.enabled = true;
        // moveSpeedDelta 슬롯을 흡인 범위 배율 증분으로 재사용 (Lv마다 +0.2 정도)
        stats.magneticRangeMultiplier += s.moveSpeedDelta;
        // Lv4 — 주기적 전체 흡인 활성
        if (level >= 4) magnetic.EnableVacuum();
    }

    void ApplyGlowOrgan(SkillLevelStats s)
    {
        if (glowOrgan == null) return;
        if (!glowOrgan.enabled) glowOrgan.enabled = true;
        // cooldownDelta로 쿨다운 감소
        glowOrgan.cooldown = Mathf.Max(3f, glowOrgan.cooldown - s.cooldownDelta);
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

    void ApplyAccelMutation(SkillLevelStats s, int level)
    {
        stats.moveSpeed += s.moveSpeedDelta;
        // Lv4 — 적 처치 시 순간 가속 (2초간 +1)
        if (level >= 4 && !accelBurstEnabled)
        {
            accelBurstEnabled = true;
            EnemyBase.OnAnyEnemyKilled += OnEnemyKilledAccel;
        }
    }

    // ── 가속변이 Lv4: 처치 시 순간 가속 ──
    const float AccelBurstSpeed = 1f;
    const float AccelBurstDuration = 2f;
    private bool accelBurstEnabled;
    private bool accelBurstActive;
    private float accelBurstEndTime;

    void OnEnemyKilledAccel()
    {
        accelBurstEndTime = Time.time + AccelBurstDuration;
        if (!accelBurstActive)
        {
            accelBurstActive = true;
            stats.moveSpeed += AccelBurstSpeed;
            StartCoroutine(AccelBurstRoutine());
        }
    }

    IEnumerator AccelBurstRoutine()
    {
        while (Time.time < accelBurstEndTime) yield return null;
        if (stats != null) stats.moveSpeed -= AccelBurstSpeed;
        accelBurstActive = false;
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

    void ApplyElectricEngine(SkillLevelStats s, int level)
    {
        stats.attackPower += s.attackPowerDelta;
        stats.attackSpeed += s.attackSpeedDelta;
        if (electricEngine == null) return;

        if (!electricEngine.enabled) electricEngine.enabled = true;
        electricEngine.chainRadius += s.rangeDelta;
        // Lv4 — 사망 시 감전 필드 생성
        if (level >= 4) electricEngine.fieldEnabled = true;
    }
}
