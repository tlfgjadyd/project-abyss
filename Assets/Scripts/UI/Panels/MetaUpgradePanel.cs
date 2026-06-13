using TMPro;
using UnityEngine;

/// <summary>
/// 메타 업그레이드 패널. 5개의 MetaUpgradeRow를 관리.
/// 구매 시 누적 세포 표시 갱신 + 모든 행 갱신 (다른 행의 buy 가능 여부 변동 반영).
/// </summary>
public class MetaUpgradePanel : MonoBehaviour
{
    [SerializeField] private TMP_Text cellsText;
    [SerializeField] private MetaUpgradeRow[] rows;

    void OnEnable()
    {
        // 패널 활성화될 때마다 전체 갱신 (메타 진입 시점에 최신 데이터)
        RefreshAll();

        if (rows != null)
        {
            foreach (var r in rows)
            {
                if (r != null) r.OnPurchased += RefreshAll;
            }
        }
    }

    void OnDisable()
    {
        if (rows == null) return;
        foreach (var r in rows)
            if (r != null) r.OnPurchased -= RefreshAll;
    }

    void RefreshAll()
    {
        if (cellsText != null)
            cellsText.text = $"누적 세포: {MetaProgressData.TotalCells}";

        if (rows == null) return;
        foreach (var r in rows)
            if (r != null) r.Refresh();
    }
}
