using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 만렙 보상 카드 한 장. 회복 또는 세포 보상 중 하나를 표시.
/// </summary>
public enum MaxLevelRewardType
{
    Heal,
    Cells
}

public class MaxLevelRewardCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private Button   button;

    private MaxLevelRewardType type;
    private System.Action<MaxLevelRewardType> callback;

    public void Setup(MaxLevelRewardType type, string title, string desc, System.Action<MaxLevelRewardType> onClick)
    {
        this.type = type;
        callback  = onClick;

        if (titleText != null) titleText.text = title;
        if (descText  != null) descText.text  = desc;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    void OnClick()
    {
        callback?.Invoke(type);
    }
}
