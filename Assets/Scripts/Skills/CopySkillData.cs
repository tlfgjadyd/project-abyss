using UnityEngine;

public enum CopySkillType { Attack, Survival, Special }

[CreateAssetMenu(fileName = "NewCopySkillData", menuName = "Abyss/Copy Skill Data")]
public class CopySkillData : ScriptableObject
{
    public string skillName;
    [TextArea] public string description;
    public CopySkillType skillType;
    public float energyCost;
}
