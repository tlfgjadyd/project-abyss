using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CopySkillSelectCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text typeText;
    [SerializeField] private TMP_Text energyCostText;
    [SerializeField] private Button   button;

    private CopySkillData data;
    private int           targetSlot;

    public void Setup(CopySkillData skillData, int slot)
    {
        data       = skillData;
        targetSlot = slot;

        if (nameText       != null) nameText.text       = skillData.skillName;
        if (descText       != null) descText.text       = skillData.description;
        if (typeText       != null) typeText.text       = skillData.skillType.ToString();
        if (energyCostText != null) energyCostText.text = $"{skillData.energyCost:0}E";

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        // 플레이어에서 해당 스킬 컴포넌트 찾기
        CopySkillBase skill = FindSkillComponent(data.copySkillID);
        if (skill != null)
            skill.data = data; // data 바인딩 (energyCost 등)

        CopySkillManager.Instance.AssignSkill(targetSlot, skill);
        CopySkillSelectPanel.Instance.Hide();

        // 스테이지 클리어
        GameManager.Instance.TriggerStageClear();
    }

    CopySkillBase FindSkillComponent(CopySkillID id)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return null;

        return id switch
        {
            CopySkillID.Berserk        => player.GetComponent<BerserkSkill>(),
            CopySkillID.Dash           => player.GetComponent<DashSkill>(),
            CopySkillID.HealingFactor  => player.GetComponent<HealingFactorSkill>(),
            _                          => null
        };
    }
}
