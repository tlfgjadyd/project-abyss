using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 카피 스킬 슬롯 1개의 UI 표시.
/// 키(Q/E/Space), 스킬 이름, 에너지 비용, 사용 가능 여부를 표시.
/// </summary>
public class CopySkillSlotUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text keyText;          // "Q", "E", "Space"
    [SerializeField] private TMP_Text skillNameText;    // 스킬 이름
    [SerializeField] private TMP_Text energyCostText;   // "50E"

    [Header("Visuals")]
    [Tooltip("슬롯 배경 (활성/비활성에 따라 색상 변경)")]
    [SerializeField] private Image background;
    [SerializeField] private Color availableColor   = new Color(0.12f, 0.45f, 0.36f, 0.92f);
    [SerializeField] private Color unavailableColor = new Color(0.4f, 0.4f, 0.4f, 0.6f);
    [SerializeField] private Color emptyColor       = new Color(0.2f, 0.2f, 0.2f, 0.4f);

    [Header("Texts Color")]
    [SerializeField] private Color textAvailableColor   = Color.white;
    [SerializeField] private Color textUnavailableColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    private const string EmptyName = "—";

    public void SetKeyLabel(string keyLabel)
    {
        if (keyText != null) keyText.text = keyLabel;
    }

    /// <summary>슬롯에 스킬이 없는 상태로 표시.</summary>
    public void SetEmpty()
    {
        if (skillNameText  != null) skillNameText.text  = EmptyName;
        if (energyCostText != null) energyCostText.text = "";
        if (background     != null) background.color    = emptyColor;
        ApplyTextColor(textUnavailableColor);
    }

    /// <summary>슬롯에 스킬을 장착한 상태로 표시.</summary>
    public void SetSkill(CopySkillData data)
    {
        if (data == null) { SetEmpty(); return; }

        if (skillNameText  != null) skillNameText.text  = data.skillName;
        if (energyCostText != null) energyCostText.text = $"{data.energyCost:0}E";
    }

    /// <summary>사용 가능 여부에 따라 시각 상태 갱신.</summary>
    public void SetAvailable(bool available)
    {
        if (background != null)
            background.color = available ? availableColor : unavailableColor;

        ApplyTextColor(available ? textAvailableColor : textUnavailableColor);
    }

    void ApplyTextColor(Color c)
    {
        if (skillNameText  != null) skillNameText.color  = c;
        if (energyCostText != null) energyCostText.color = c;
    }
}
