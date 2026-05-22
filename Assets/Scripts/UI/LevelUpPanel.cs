using UnityEngine;

public class LevelUpPanel : MonoBehaviour
{
    [SerializeField] private SkillCardUI[] cards;
    [Tooltip("활성 카드 수에 따라 X 위치 재정렬 (1=중앙, 2=±cardSpacing/2, 3=±cardSpacing,0)")]
    [SerializeField] private float cardSpacing = 290f;

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

        int activeCount = Mathf.Min(cards.Length, offered.Length);
        // X 좌표 계산: 활성 카드 수에 따라 좌우 대칭 배치
        float[] xs;
        if (activeCount <= 1) xs = new[] { 0f };
        else if (activeCount == 2) xs = new[] { -cardSpacing * 0.5f, cardSpacing * 0.5f };
        else xs = new[] { -cardSpacing, 0f, cardSpacing };

        int xi = 0;
        for (int i = 0; i < cards.Length; i++)
        {
            bool hasCard = i < offered.Length;
            cards[i].gameObject.SetActive(hasCard);
            if (hasCard)
            {
                var rt = cards[i].transform as RectTransform;
                if (rt != null && xi < xs.Length)
                    rt.anchoredPosition = new Vector2(xs[xi], rt.anchoredPosition.y);
                cards[i].Setup(offered[i]);
                xi++;
            }
        }
    }

    void OnStateChanged(GameManager.GameState state)
    {
        // 중첩 ChangeState 호출 시 매개변수가 stale일 수 있음 → 항상 CurrentState 기준으로 판단
        if (GameManager.Instance.CurrentState != GameManager.GameState.LevelUp)
            gameObject.SetActive(false);
    }
}
