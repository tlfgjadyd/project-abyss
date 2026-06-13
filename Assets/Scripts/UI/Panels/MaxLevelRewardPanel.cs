using UnityEngine;

/// <summary>
/// 모든 일반 스킬 만렙 도달 후 레벨업 시 표시되는 보상 카드 패널.
/// 회복(HP %) vs 세포(+N) 두 카드 중 선택.
/// LevelManager.OnMaxLevelRewardOffered를 구독해 자동 표시.
/// </summary>
public class MaxLevelRewardPanel : MonoBehaviour
{
    public static MaxLevelRewardPanel Instance { get; private set; }

    [SerializeField] private MaxLevelRewardCardUI healCard;
    [SerializeField] private MaxLevelRewardCardUI cellsCard;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        gameObject.SetActive(false);

        if (LevelManager.Instance != null)
            LevelManager.Instance.OnMaxLevelRewardOffered += Show;
    }

    void OnDestroy()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.OnMaxLevelRewardOffered -= Show;
    }

    public void Show()
    {
        if (LevelManager.Instance == null) return;

        gameObject.SetActive(true);

        int healPercent = Mathf.RoundToInt(LevelManager.Instance.HpHealRatioPerSelection * 100f);
        int cells       = LevelManager.Instance.CellsPerSelection;

        if (healCard != null)
            healCard.Setup(
                MaxLevelRewardType.Heal,
                "세포 강화",
                $"HP {healPercent}% 회복",
                OnCardSelected
            );

        if (cellsCard != null)
            cellsCard.Setup(
                MaxLevelRewardType.Cells,
                "세포 추출",
                $"세포 +{cells}",
                OnCardSelected
            );
    }

    void OnCardSelected(MaxLevelRewardType type)
    {
        gameObject.SetActive(false);
        LevelManager.Instance.SelectMaxLevelReward(type);
    }
}
