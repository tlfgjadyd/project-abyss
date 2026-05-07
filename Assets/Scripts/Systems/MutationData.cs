using UnityEngine;

public enum MutationID
{
    Overload,          // 과부화     — 공격력 ×1.6, 최대 HP -30%
    OvgrownTentacle,   // 과성장 촉수 — 전 스킬 범위 ×1.5, 이동속도 -30%
    MimicryOrgan       // 의태 기관   — 카피 스킬 에너지 소모 0, HP 회복 완전 차단
}

[CreateAssetMenu(fileName = "NewMutation", menuName = "Abyss/Mutation Data")]
public class MutationData : ScriptableObject
{
    public string mutationName;
    [TextArea] public string description;
    [TextArea] public string penaltyDescription;
    public MutationID mutationID;
}
