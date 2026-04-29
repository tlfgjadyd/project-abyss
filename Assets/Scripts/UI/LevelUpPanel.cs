using UnityEngine;

public class LevelUpPanel : MonoBehaviour
{
    [SerializeField] private SkillCardUI[] cards;

    void Start()
    {
        gameObject.SetActive(false);
        LevelManager.Instance.OnLevelUpOffered += ShowPanel;
        GameManager.Instance.OnGameStateChanged += OnStateChanged;
    }

    void OnDestroy()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.OnLevelUpOffered -= ShowPanel;
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= OnStateChanged;
    }

    void ShowPanel(SkillData[] offered)
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
        if (state != GameManager.GameState.LevelUp)
            gameObject.SetActive(false);
    }
}
