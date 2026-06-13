using UnityEngine;

public enum MutationID
{
    Overload,          // 과부화      — 공격력 ×1.6 / 최대 HP -45%
    OvergrownTentacle, // 과성장 촉수  — 전 스킬 범위 ×1.6 / 이동속도 -15%
    MimicryOrgan,      // 의태 기관    — 15초마다 3초 무적 + 무적 중 공격력 버스트(평소의 4배) / 평소 공격력 -50%
    SensoryCollapse,   // 감각 붕괴    — 공격속도 ×1.5, 에너지 충전 ×1.25 / 스킬 사용 시 5% 확률 0.5초 스턴
    ToxicOverload      // 독성 과부화  — 독/감전/출혈 DoT ×1.7 / 물리 ×0.7, 물리 범위 ×0.8
}

[CreateAssetMenu(fileName = "NewMutation", menuName = "Abyss/Mutation Data")]
public class MutationData : ScriptableObject
{
    public string mutationName;
    [TextArea] public string description;
    [TextArea] public string penaltyDescription;
    public MutationID mutationID;
}
