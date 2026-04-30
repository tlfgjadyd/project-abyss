using UnityEngine;

public enum SkillType { Attack, Passive }

public enum SkillID
{
    Slash, PoisonNeedle, BioticExplosion, ElectricEngine,
    CellRegen, AccelMutation, NeuralAccel
}

[System.Serializable]
public struct SkillLevelStats
{
    [Header("Combat")]
    public float attackPowerDelta;
    public float attackSpeedDelta;
    public float rangeDelta;

    [Header("Passive")]
    public float moveSpeedDelta;
    public float maxHpDelta;
    public float hpRegenDelta;

    [Header("Special")]
    public bool knockbackEnabled;       // Slash Lv4
    public bool piercingEnabled;        // PoisonNeedle Lv4
    public int  extraProjectiles;       // PoisonNeedle Lv4: +2
    public float cooldownDelta;         // PoisonNeedle 쿨타임 감소 (초)
    public float stunDurationDelta;     // BioticExplosion Lv4 스턴 시간 증가
}

[CreateAssetMenu(fileName = "NewSkillData", menuName = "Abyss/Skill Data")]
public class SkillData : ScriptableObject
{
    public string skillName;
    [TextArea] public string description;
    public SkillType skillType;
    public SkillID skillID;
    public int maxLevel = 4;

    // index 0 = Lv1 효과, index 1 = Lv2 효과, ...
    public SkillLevelStats[] levelStats;
}
