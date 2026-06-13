using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MutationCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text penaltyText;
    [SerializeField] private Button   button;

    private MutationData data;

    public void Setup(MutationData mutationData)
    {
        data = mutationData;

        if (nameText    != null) nameText.text    = mutationData.mutationName;
        if (descText    != null) descText.text    = mutationData.description;
        if (penaltyText != null) penaltyText.text = string.IsNullOrEmpty(mutationData.penaltyDescription)
                                                    ? "" : $"패널티: {mutationData.penaltyDescription}";

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    void OnClick() => MutationManager.Instance.SelectMutation(data);

    /// <summary>이 카드의 TMP 폰트 아틀라스에 주어진 글자들을 미리 구워둔다(첫 표시 hitch 방지).</summary>
    public void PrewarmFont(string chars)
    {
        if (string.IsNullOrEmpty(chars)) return;
        AddChars(nameText, chars);
        AddChars(descText, chars);
        AddChars(penaltyText, chars);
    }

    static void AddChars(TMP_Text t, string chars)
    {
        if (t != null && t.font != null) t.font.TryAddCharacters(chars);
    }
}
