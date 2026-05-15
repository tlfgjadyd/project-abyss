using UnityEngine;

public enum CopySkillType { Attack, Survival, Special }

/// <summary>플레이어 오브젝트에서 컴포넌트를 찾을 때 사용하는 식별자</summary>
/// <remarks>
/// enum 멤버 순서를 절대 변경하지 말 것. ScriptableObject 에셋이 int로 직렬화되므로
/// 추가는 항상 끝에만. (Day 33 추가: Ultrasonic, DeepPressure, PredatorCharge)
/// </remarks>
public enum CopySkillID
{
    None,
    Berserk,        // 1스테이지 광폭화
    Dash,           // 1스테이지 돌진
    HealingFactor,  // 1스테이지 회복
    Ultrasonic,     // 2스테이지 초음파 (부채꼴 대피해 + 스턴)
    DeepPressure,   // 2스테이지 심해 압박 (주변 디버프)
    PredatorCharge  // 2스테이지 포식 충돌 (직선 돌진, 무적, 출혈)
}

[CreateAssetMenu(fileName = "NewCopySkillData", menuName = "Abyss/Copy Skill Data")]
public class CopySkillData : ScriptableObject
{
    public string skillName;
    [TextArea] public string description;
    public CopySkillType skillType;
    public float energyCost;
    public CopySkillID copySkillID;
}
