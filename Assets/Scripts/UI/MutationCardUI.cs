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
}
