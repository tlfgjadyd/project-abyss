using UnityEngine;

public class MutationPanel : MonoBehaviour
{
    [SerializeField] private MutationCardUI[] cards; // 카드 슬롯 2개

    void Start()
    {
        gameObject.SetActive(false);
        MutationManager.Instance.OnMutationOffered  += ShowPanel;
        GameManager.Instance.OnGameStateChanged     += OnStateChanged;
    }

    void OnDestroy()
    {
        if (MutationManager.Instance != null)
            MutationManager.Instance.OnMutationOffered -= ShowPanel;
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= OnStateChanged;
    }

    void ShowPanel(MutationData[] offered)
    {
        gameObject.SetActive(true);

        for (int i = 0; i < cards.Length; i++)
        {
            bool hasCard = i < offered.Length;
            cards[i].gameObject.SetActive(hasCard);
            if (hasCard)
                cards[i].Setup(offered[i]);
        }
    }

    void OnStateChanged(GameManager.GameState state)
    {
        if (state != GameManager.GameState.Mutation)
            gameObject.SetActive(false);
    }
}
