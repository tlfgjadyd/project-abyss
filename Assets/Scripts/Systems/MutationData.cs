using UnityEngine;

public enum MutationID
{
    Overload,          // 과부화      — 공격력 ×1.6, 최대 HP -30%
    OvergrownTentacle, // 과성장 촉수  — 전 스킬 범위 ×1.5, 이동속도 -30%
    MimicryOrgan,      // 의태 기관    — 이속 ×2, 1분마다 3초 무적 / 공격력 -50%
    SensoryCollapse,   // 감각 붕괴    — 공격속도 ×2, 에너지 충전 ×1.5 / 스킬 사용 시 5~10% 확률 0.5초 스턴
    ToxicOverload      // 독성 과부화  — 독/감전 +70% / 물리 -30%, 물리 범위 -20%
}

[CreateAssetMenu(fileName = "NewMutation", menuName = "Abyss/Mutation Data")]
public class MutationData : ScriptableObject
{
    public string mutationName;
    [TextArea] public string description;
    [TextArea] public string penaltyDescription;
    public MutationID mutationID;
}
