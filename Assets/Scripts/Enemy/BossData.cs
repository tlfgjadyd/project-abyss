using UnityEngine;

[CreateAssetMenu(fileName = "NewBossData", menuName = "Abyss/Boss Data")]
public class BossData : ScriptableObject
{
    [Header("기본 스탯")]
    public string bossName = "실험용 실험체";
    public float maxHp = 600f;
    public float moveSpeed = 2.5f;
    public float contactDamage = 15f;
    public float contactCooldown = 1f;

    [Header("페이즈 2")]
    [Range(0f, 1f)]
    [Tooltip("HP 비율이 이 값 이하로 떨어지면 페이즈 2 전환")]
    public float phase2HpRatio = 0.5f;
    public float phase2SpeedMultiplier = 1.6f;
    public float chargeInterval = 4f;
    public float chargeSpeed = 14f;
    public float chargeDuration = 0.35f;

    [Header("카피 스킬 선택지")]
    [Tooltip("보스 처치 후 제시되는 카피 스킬 선택지 (최대 3개)")]
    public CopySkillData[] copySkillOptions;
    [Tooltip("0=Q, 1=E, 2=Space")]
    public int copySkillSlot = 0;
}
