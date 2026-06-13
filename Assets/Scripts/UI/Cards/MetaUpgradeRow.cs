using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 메타 업그레이드 한 행 (스탯명 / 현재 Lv / 다음 비용 / 구매 버튼).
/// MetaUpgradePanel이 5개를 가지고 있고 각각 Stat enum이 인스펙터에서 설정됨.
/// </summary>
public class MetaUpgradeRow : MonoBehaviour
{
    [SerializeField] private MetaProgressData.Stat stat;

    [Header("Display")]
    [SerializeField] private TMP_Text statNameText;
    [SerializeField] private TMP_Text levelText;     // "Lv 2 / 4"
    [SerializeField] private TMP_Text effectText;    // "+20% 이동속도" 등 (현재 효과 또는 다음 효과)
    [SerializeField] private TMP_Text costText;      // "비용: 35" 또는 "MAX"

    [Header("Buttons")]
    [SerializeField] private Button purchaseButton;

    [Header("Display Strings (인스펙터에서 한글 라벨 지정)")]
    [SerializeField] private string statDisplayName = "최대 HP";

    // 외부에서 갱신 요청 시 구독자에게 알림
    public System.Action OnPurchased;

    void Start()
    {
        if (purchaseButton != null)
            purchaseButton.onClick.AddListener(OnPurchaseClicked);

        Refresh();
    }

    public void Refresh()
    {
        int lv      = MetaProgressData.GetLevel(stat);
        int maxLv   = MetaProgressData.GetMaxLevel(stat);
        int nextCost = MetaProgressData.GetNextCost(stat);

        if (statNameText != null) statNameText.text = statDisplayName;
        if (levelText    != null) levelText.text    = $"Lv {lv} / {maxLv}";
        if (effectText   != null) effectText.text   = FormatEffect(stat, lv);

        bool isMax  = nextCost < 0;
        bool canBuy = !isMax && MetaProgressData.TotalCells >= nextCost;

        if (costText != null)
            costText.text = isMax ? "MAX" : $"비용: {nextCost}";

        if (purchaseButton != null)
            purchaseButton.interactable = canBuy;
    }

    void OnPurchaseClicked()
    {
        if (MetaProgressData.TryPurchase(stat))
            OnPurchased?.Invoke();
    }

    static string FormatEffect(MetaProgressData.Stat stat, int lv)
    {
        return stat switch
        {
            MetaProgressData.Stat.MaxHp              => $"+{lv * 10} HP",
            MetaProgressData.Stat.MoveSpeed          => $"+{lv * 5}% 이동속도",
            MetaProgressData.Stat.AttackSpeed        => $"+{lv * 5}% 공격속도",
            MetaProgressData.Stat.AttackPower        => $"+{lv * 5}% 공격력",
            MetaProgressData.Stat.PressureResistance => $"+{lv * 10}% 압력 저항",
            _ => "?"
        };
    }
}
