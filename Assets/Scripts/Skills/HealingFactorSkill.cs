using UnityEngine;

// 힐링팩터 — 최대 체력의 일정 비율 즉시 회복 (30E)
// 체력이 최대치일 때는 사용 불가 (에너지 낭비 방지).
public class HealingFactorSkill : CopySkillBase
{
    [SerializeField] private float healRatio = 0.1f;   // 기본 10%

    public override bool CanExecute()
    {
        if (stats == null) return false;
        // 회복 차단 상태(의태 기관 돌연변이 등)이거나 이미 최대 체력이면 사용 불가
        if (stats.healingBlocked) return false;
        return stats.currentHp < stats.maxHp;
    }

    public override void Execute()
    {
        stats.Heal(stats.maxHp * healRatio);
    }
}
