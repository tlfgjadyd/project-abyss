using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MutationCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private Button   button;

    private MutationData data;

    public void Setup(MutationData mutationData)
    {
        data = mutationData;

        if (nameText != null) nameText.text = mutationData.mutationName;
        if (descText  != null) descText.text  = mutationData.description;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);
    }

    void OnClick() => MutationManager.Instance.SelectMutation(data);
}
