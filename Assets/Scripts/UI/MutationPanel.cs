using UnityEngine;

public class MutationPanel : MonoBehaviour
{
    [SerializeField] private MutationCardUI[] cards; // 카드 슬롯 2개

    void Start()
    {
        gameObject.SetActive(false);
        MutationManager.Instance.OnMutationOffered  += ShowPanel;
        GameManager.Instance.OnGameStateChanged     += OnStateChanged;

        PrewarmGlyphs();
    }

    /// <summary>풀의 모든 돌연변이 텍스트 글리프를 폰트 아틀라스에 미리 구워 첫 표시 hitch 제거 (피드백 UI-4).</summary>
    void PrewarmGlyphs()
    {
        var mm = MutationManager.Instance;
        if (mm == null || mm.Pool == null || cards == null) return;

        var sb = new System.Text.StringBuilder("패널티: 0123456789%+-×");
        foreach (var m in mm.Pool)
        {
            if (m == null) continue;
            sb.Append(m.mutationName).Append(m.description).Append(m.penaltyDescription);
        }
        string all = sb.ToString();
        foreach (var c in cards)
            if (c != null) c.PrewarmFont(all);
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
        // 중첩 ChangeState 호출 시 매개변수가 stale일 수 있음 → 항상 CurrentState 기준으로 판단
        if (GameManager.Instance.CurrentState != GameManager.GameState.Mutation)
            gameObject.SetActive(false);
    }
}
