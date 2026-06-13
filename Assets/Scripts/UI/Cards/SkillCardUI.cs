using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text typeText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Button button;

    // "이번 레벨" 효과 줄 강조색
    private const string EffectColor = "#7FE0FF";

    private SkillData data;

    public void Setup(SkillData skillData)
    {
        data = skillData;
        int currentLevel = LevelManager.Instance.GetSkillLevel(skillData);

        nameText.text = skillData.skillName;
        typeText.text = skillData.skillType.ToString();
        levelText.text = currentLevel == 0 ? "NEW" : $"Lv.{currentLevel} → {currentLevel + 1}";

        // 정적 설명(정체성 + 최종 효과) + 동적 "이번 레벨" 수치 변화 줄
        string effect = FormatLevelEffect(skillData, currentLevel + 1);
        descText.text = string.IsNullOrEmpty(effect)
            ? skillData.description
            : $"{skillData.description}\n\n<color={EffectColor}>{effect}</color>";

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    /// <summary>목표 레벨에서 새로 적용되는 수치/효과 변화를 짧은 한 줄로 요약.</summary>
    string FormatLevelEffect(SkillData skill, int targetLevel)
    {
        if (skill.levelStats == null || targetLevel < 1 || targetLevel > skill.levelStats.Length)
            return "";

        SkillLevelStats s = skill.levelStats[targetLevel - 1];
        var parts = new List<string>();

        // 자기 유도는 moveSpeedDelta를 '흡인 범위 증분(%)'으로 재사용
        if (skill.skillID == SkillID.MagneticInduction)
        {
            if (s.moveSpeedDelta != 0f) parts.Add($"흡인 범위 +{Mathf.RoundToInt(s.moveSpeedDelta * 100f)}%");
        }
        else
        {
            if (s.attackPowerDelta != 0f) parts.Add($"공격력 +{s.attackPowerDelta:0.#}");
            if (s.attackSpeedDelta != 0f) parts.Add($"공격 속도 +{s.attackSpeedDelta:0.#}");
            if (s.rangeDelta != 0f)       parts.Add($"사거리 +{s.rangeDelta:0.#}");
            if (s.moveSpeedDelta != 0f)   parts.Add($"이동 속도 +{s.moveSpeedDelta:0.#}");
            if (s.maxHpDelta != 0f)       parts.Add($"최대 HP +{s.maxHpDelta:0.#}");
            if (s.hpRegenDelta != 0f)     parts.Add($"회복 +{s.hpRegenDelta:0.##}/s");
        }

        if (s.cooldownDelta != 0f)      parts.Add($"쿨다운 -{s.cooldownDelta:0.#}s");
        if (s.extraProjectiles != 0)    parts.Add($"발사체 +{s.extraProjectiles}");
        if (s.stunDurationDelta != 0f)  parts.Add($"스턴 +{s.stunDurationDelta:0.#}s");
        if (s.knockbackEnabled)         parts.Add("넉백");
        // piercingEnabled는 가시 산탄에서 '출혈' 트리거로 재사용됨
        if (s.piercingEnabled)          parts.Add(skill.skillID == SkillID.SpikeBurst ? "출혈" : "관통");

        return parts.Count == 0 ? "" : string.Join(", ", parts);
    }

    void OnClick() => LevelManager.Instance.SelectSkill(data);
}
