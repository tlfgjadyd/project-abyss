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

    private SkillData data;

    public void Setup(SkillData skillData)
    {
        data = skillData;
        int currentLevel = LevelManager.Instance.GetSkillLevel(skillData);

        nameText.text = skillData.skillName;
        descText.text = skillData.description;
        typeText.text = skillData.skillType.ToString();
        levelText.text = currentLevel == 0 ? "NEW" : $"Lv.{currentLevel} → {currentLevel + 1}";

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    void OnClick() => LevelManager.Instance.SelectSkill(data);
}
