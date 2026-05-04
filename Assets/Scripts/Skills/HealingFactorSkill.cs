using UnityEngine;

// 힐링팩터 — 최대 체력의 일정 비율 즉시 회복 (30E)
public class HealingFactorSkill : CopySkillBase
{
    [SerializeField] private float healRatio = 0.1f;   // 기본 10%

    public override void Execute()
    {
        stats.Heal(stats.maxHp * healRatio);
    }
}
