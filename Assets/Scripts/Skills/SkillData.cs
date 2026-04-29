using UnityEngine;

public enum SkillType { Attack, Passive }

[CreateAssetMenu(fileName = "NewSkillData", menuName = "Abyss/Skill Data")]
public class SkillData : ScriptableObject
{
    public string skillName;
    [TextArea] public string description;
    public SkillType skillType;
    public int maxLevel = 4;
}
