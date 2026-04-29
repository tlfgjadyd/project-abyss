using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private SkillData[] skillPool;
    [SerializeField] private float baseExpPerLevel = 20f;

    public int CurrentLevel { get; private set; } = 1;
    public float CurrentExp { get; private set; }
    public float ExpToNextLevel { get; private set; }

    private readonly Dictionary<SkillData, int> skillLevels = new();
    private SkillData[] currentOffer;

    public System.Action<SkillData[]> OnLevelUpOffered;
    public System.Action<int> OnLevelChanged;
    public System.Action<float, float> OnExpChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        ExpToNextLevel = baseExpPerLevel;
    }

    public void AddExp(float amount)
    {
        CurrentExp += amount;
        OnExpChanged?.Invoke(CurrentExp, ExpToNextLevel);

        while (CurrentExp >= ExpToNextLevel)
        {
            CurrentExp -= ExpToNextLevel;
            LevelUp();
        }
    }

    void LevelUp()
    {
        CurrentLevel++;
        ExpToNextLevel = baseExpPerLevel * CurrentLevel;
        OnLevelChanged?.Invoke(CurrentLevel);

        currentOffer = PickSkills(3);
        if (currentOffer.Length == 0) return;

        OnLevelUpOffered?.Invoke(currentOffer);
        GameManager.Instance.TriggerLevelUp();
    }

    SkillData[] PickSkills(int count)
    {
        var available = skillPool
            .Where(s => GetSkillLevel(s) < s.maxLevel)
            .ToList();

        for (int i = available.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (available[i], available[j]) = (available[j], available[i]);
        }

        return available.Take(count).ToArray();
    }

    public void SelectSkill(SkillData skill)
    {
        skillLevels[skill] = GetSkillLevel(skill) + 1;
        // TODO Week 2: 실제 스킬 효과 적용
        GameManager.Instance.ResumeGame();
    }

    public int GetSkillLevel(SkillData skill)
    {
        return skillLevels.TryGetValue(skill, out int level) ? level : 0;
    }
}
