using UnityEngine;

public enum CopySkillType { Attack, Survival, Special }

/// <summary>플레이어 오브젝트에서 컴포넌트를 찾을 때 사용하는 식별자</summary>
public enum CopySkillID { None, Berserk, Dash, HealingFactor }

[CreateAssetMenu(fileName = "NewCopySkillData", menuName = "Abyss/Copy Skill Data")]
public class CopySkillData : ScriptableObject
{
    public string skillName;
    [TextArea] public string description;
    public CopySkillType skillType;
    public float energyCost;
    public CopySkillID copySkillID;
}
