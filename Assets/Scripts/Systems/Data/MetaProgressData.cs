using UnityEngine;

/// <summary>
/// PlayerPrefs 기반 메타 진행 데이터.
/// 인게임 세포(LevelManager.CurrentCells)는 스테이지 클리어/게임오버 시 여기로 누적된다.
/// 메타 업그레이드는 메인 메뉴에서만 구매. 게임 시작 시 PlayerStats.Awake에서 적용.
///
/// 6주차 단순화: PlayerPrefs(평문). 변조 방지(JSON+해시)는 7주차 이후.
/// </summary>
public static class MetaProgressData
{
    // ── PlayerPrefs 키 ───────────────────────────
    const string KEY_CELLS    = "Meta_TotalCells";
    const string KEY_HP       = "Meta_MaxHpLevel";
    const string KEY_MOVE     = "Meta_MoveSpeedLevel";
    const string KEY_ATK_SPD  = "Meta_AttackSpeedLevel";
    const string KEY_ATK_PWR  = "Meta_AttackPowerLevel";
    const string KEY_PRESSURE = "Meta_PressureResistanceLevel";

    public enum Stat { MaxHp, MoveSpeed, AttackSpeed, AttackPower, PressureResistance }

    // ── 단계별 비용/효과 표 (사전 결정 §2-4) ─────

    static readonly int[] CostHp       = { 5, 15, 35, 75 };
    static readonly int[] CostMove     = { 5, 15, 35, 75 };
    static readonly int[] CostAtkSpd   = { 5, 15, 35, 75 };
    static readonly int[] CostAtkPwr   = { 5, 15, 35, 75 };
    static readonly int[] CostPressure = { 10, 25, 60, 150 };

    /// <summary>각 스탯의 최대 레벨</summary>
    public static int GetMaxLevel(Stat stat) => GetCostTable(stat).Length;

    static int[] GetCostTable(Stat stat) => stat switch
    {
        Stat.MaxHp              => CostHp,
        Stat.MoveSpeed          => CostMove,
        Stat.AttackSpeed        => CostAtkSpd,
        Stat.AttackPower        => CostAtkPwr,
        Stat.PressureResistance => CostPressure,
        _ => CostHp
    };

    // ── 세포 (누적) ──────────────────────────────

    public static int TotalCells
    {
        get => PlayerPrefs.GetInt(KEY_CELLS, 0);
        set { PlayerPrefs.SetInt(KEY_CELLS, Mathf.Max(0, value)); PlayerPrefs.Save(); }
    }

    public static void AddCells(int amount)
    {
        if (amount <= 0) return;
        TotalCells = TotalCells + amount;
    }

    // ── 레벨 조회/설정 ───────────────────────────

    public static int GetLevel(Stat stat)
    {
        string key = KeyOf(stat);
        return Mathf.Clamp(PlayerPrefs.GetInt(key, 0), 0, GetMaxLevel(stat));
    }

    static void SetLevel(Stat stat, int level)
    {
        PlayerPrefs.SetInt(KeyOf(stat), Mathf.Clamp(level, 0, GetMaxLevel(stat)));
        PlayerPrefs.Save();
    }

    static string KeyOf(Stat stat) => stat switch
    {
        Stat.MaxHp              => KEY_HP,
        Stat.MoveSpeed          => KEY_MOVE,
        Stat.AttackSpeed        => KEY_ATK_SPD,
        Stat.AttackPower        => KEY_ATK_PWR,
        Stat.PressureResistance => KEY_PRESSURE,
        _ => KEY_HP
    };

    /// <summary>다음 레벨 구매 비용. 만렙이면 -1.</summary>
    public static int GetNextCost(Stat stat)
    {
        int lv = GetLevel(stat);
        var table = GetCostTable(stat);
        if (lv >= table.Length) return -1;
        return table[lv];
    }

    /// <summary>구매 시도. 성공 시 true (세포 차감 + 레벨 ++).</summary>
    public static bool TryPurchase(Stat stat)
    {
        int cost = GetNextCost(stat);
        if (cost < 0) return false;                  // 만렙
        if (TotalCells < cost) return false;         // 세포 부족

        TotalCells = TotalCells - cost;
        SetLevel(stat, GetLevel(stat) + 1);
        return true;
    }

    // ── 보너스 계산 (PlayerStats.Awake에서 호출) ─

    /// <summary>최대 HP 가산값. 단계별 +10/+20/+30/+40</summary>
    public static float GetMaxHpBonus() => GetLevel(Stat.MaxHp) * 10f;

    /// <summary>이동속도 배율. 단계별 ×1.05/×1.10/×1.15/×1.20</summary>
    public static float GetMoveSpeedMultiplier() => 1f + GetLevel(Stat.MoveSpeed) * 0.05f;

    /// <summary>공격속도 배율. 단계별 ×1.05/×1.10/×1.15/×1.20</summary>
    public static float GetAttackSpeedMultiplier() => 1f + GetLevel(Stat.AttackSpeed) * 0.05f;

    /// <summary>공격력 배율. 단계별 ×1.05/×1.10/×1.15/×1.20</summary>
    public static float GetAttackPowerMultiplier() => 1f + GetLevel(Stat.AttackPower) * 0.05f;

    /// <summary>압력 저항 가산값 (0~0.4). PlayerStats.pressureResistance에 더한다.</summary>
    public static float GetPressureResistanceBonus() => GetLevel(Stat.PressureResistance) * 0.1f;

    /// <summary>디버그/테스트용 — 모든 메타 데이터 초기화.</summary>
    public static void ResetAll()
    {
        PlayerPrefs.DeleteKey(KEY_CELLS);
        PlayerPrefs.DeleteKey(KEY_HP);
        PlayerPrefs.DeleteKey(KEY_MOVE);
        PlayerPrefs.DeleteKey(KEY_ATK_SPD);
        PlayerPrefs.DeleteKey(KEY_ATK_PWR);
        PlayerPrefs.DeleteKey(KEY_PRESSURE);
        PlayerPrefs.Save();
    }
}
